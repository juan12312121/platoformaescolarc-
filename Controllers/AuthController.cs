using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using PlataformaEscolar.API.Data;
using PlataformaEscolar.API.Models;
using PlataformaEscolar.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using PlataformaEscolar.API.Security;

namespace PlataformaEscolar.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext context;
        private readonly IJwtService jwtService;
        private readonly ISecurityLogger securityLogger;
        private readonly ILogger<AuthController> logger;

        public AuthController(
            AppDbContext context,
            IJwtService jwtService,
            ISecurityLogger securityLogger,
            ILogger<AuthController> logger)
        {
            this.context = context;
            this.jwtService = jwtService;
            this.securityLogger = securityLogger;
            this.logger = logger;
        }

        /// <summary>
        /// Registrar nuevo usuario con validaciones de seguridad
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
        {
            // ===== PASO 1: VALIDACIÓN AUTOMÁTICA =====
            if (!ModelState.IsValid)
            {
                logger.LogWarning("Registro fallido - ModelState inválido para: {Email}", request.Correo);
                return BadRequest(ModelState);
            }

            logger.LogInformation("Intento de registro: {Email}", request.Correo);

            // ===== PASO 2: VERIFICAR EMAIL DUPLICADO =====
            var correoNormalizado = request.Correo.Trim().ToLower();
            var usuarioExistente = await context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Correo == correoNormalizado);

            if (usuarioExistente != null)
            {
                securityLogger.LogSuspiciousActivity(
                    "REGISTRO_EMAIL_DUPLICADO",
                    $"Intento de registro con email ya existente: {request.Correo}");
                
                return Conflict(new
                {
                    error = "El correo ya está registrado",
                    code = "EMAIL_EXISTS"
                });
            }

            // ===== PASO 3: HASH DE CONTRASEÑA =====
            var passwordHasher = new PasswordHasher<Usuario>();
            var usuario = new Usuario
            {
                Nombre = request.Nombre.Trim(),
                Correo = correoNormalizado,
                Rol = request.Rol,
                CreadoEn = DateTime.UtcNow,
                FailedLoginAttempts = 0,
                BloqueadoHasta = null
            };

            usuario.PasswordHash = passwordHasher.HashPassword(usuario, request.Password);

            // ===== PASO 4: GUARDAR EN BD =====
            try
            {
                await context.Usuarios.AddAsync(usuario);
                await context.SaveChangesAsync();

                logger.LogInformation("Registro exitoso: {Email} - Rol: {Rol}",
                    request.Correo, request.Rol);

                return Ok(new
                {
                    id = usuario.Id,
                    nombre = usuario.Nombre,
                    correo = usuario.Correo,
                    rol = usuario.Rol,
                    mensaje = "Usuario registrado exitosamente"
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al registrar usuario: {Email}", request.Correo);
                return StatusCode(500, new
                {
                    error = "Error al registrar usuario",
                    code = "REGISTRATION_ERROR"
                });
            }
        }

        /// <summary>
        /// Login con protección contra fuerza bruta
        /// Bloquea cuenta después de 5 intentos fallidos
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            // ===== PASO 1: VALIDACIÓN AUTOMÁTICA =====
            if (!ModelState.IsValid)
            {
                logger.LogWarning("Login fallido - ModelState inválido");
                return BadRequest(ModelState);
            }

            var correoNormalizado = request.Correo.Trim().ToLower();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "DESCONOCIDA";

            logger.LogInformation("Intento de login: {Email} desde IP: {IP}",
                request.Correo, ipAddress);

            // ===== PASO 2: BUSCAR USUARIO =====
            var usuario = await context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == correoNormalizado);

            if (usuario == null)
            {
                securityLogger.LogLoginAttempt(request.Correo, false, ipAddress);
                logger.LogWarning("Login fallido - Usuario no encontrado: {Email}", request.Correo);

                // No revelar que el email existe
                return Unauthorized(new
                {
                    error = "Credenciales inválidas",
                    code = "INVALID_CREDENTIALS"
                });
            }

            // ===== PASO 3: VERIFICAR SI ESTÁ BLOQUEADO =====
            usuario.DesbloquearSiEsNecesario();

            if (usuario.EstaBloqueado())
            {
                var minutosRestantes = Math.Ceiling(
                    (usuario.BloqueadoHasta.Value - DateTime.UtcNow).TotalMinutes);

                securityLogger.LogSuspiciousActivity(
                    "INTENTO_LOGIN_CUENTA_BLOQUEADA",
                    $"Email: {request.Correo}, IP: {ipAddress}");

                logger.LogWarning("Login fallido - Cuenta bloqueada: {Email}", request.Correo);

                return Unauthorized(new
                {
                    error = $"Cuenta bloqueada temporalmente. Intenta en {minutosRestantes} minutos",
                    code = "ACCOUNT_LOCKED",
                    minutosRestantes = minutosRestantes
                });
            }

            // ===== PASO 4: VERIFICAR CONTRASEÑA =====
            var passwordHasher = new PasswordHasher<Usuario>();
            var result = passwordHasher.VerifyHashedPassword(
                usuario, usuario.PasswordHash, request.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                // Incrementar contador de intentos fallidos
                usuario.FailedLoginAttempts++;

                // Bloquear después de 5 intentos
                if (usuario.FailedLoginAttempts >= 5)
                {
                    usuario.BloqueadoHasta = DateTime.UtcNow.AddMinutes(15);
                    logger.LogWarning("Cuenta bloqueada por intentos fallidos: {Email}", request.Correo);
                    securityLogger.LogSuspiciousActivity(
                        "CUENTA_BLOQUEADA_INTENTOS",
                        $"Email: {request.Correo}, IP: {ipAddress}");
                }

                await context.SaveChangesAsync();
                securityLogger.LogLoginAttempt(request.Correo, false, ipAddress);

                logger.LogWarning("Login fallido - Contraseña incorrecta: {Email} " +
                    "(Intento {Attempt}/5)", request.Correo, usuario.FailedLoginAttempts);

                return Unauthorized(new
                {
                    error = "Credenciales inválidas",
                    code = "INVALID_CREDENTIALS"
                });
            }

            // ===== PASO 5: LOGIN EXITOSO =====
            usuario.FailedLoginAttempts = 0;
            usuario.BloqueadoHasta = null;
            await context.SaveChangesAsync();

            var token = jwtService.GenerarToken(usuario);
            securityLogger.LogLoginAttempt(request.Correo, true, ipAddress);

            logger.LogInformation("Login exitoso: {Email} - Token generado", request.Correo);

            return Ok(new
            {
                token = token,
                id = usuario.Id,
                nombre = usuario.Nombre,
                correo = usuario.Correo,
                rol = usuario.Rol,
                mensaje = "Login exitoso"
            });
        }

        /// <summary>
        /// Obtener perfil del usuario autenticado
        /// </summary>
        [HttpGet("perfil")]
        [Authorize]
        public async Task<IActionResult> ObtenerPerfil()
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var usuario = await context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null)
                return NotFound(new { error = "Usuario no encontrado" });

            securityLogger.LogSensitiveDataAccess(
                usuario.Correo, "VER_PERFIL", "DATOS_PERSONALES");

            return Ok(new
            {
                id = usuario.Id,
                nombre = usuario.Nombre,
                correo = usuario.Correo,
                rol = usuario.Rol,
                creadoEn = usuario.CreadoEn
            });
        }

        /// <summary>
        /// Cambiar contraseña del usuario autenticado
        /// </summary>
        [HttpPost("cambiar-contrasena")]
        [Authorize]
        public async Task<IActionResult> CambiarContrasena(
            [FromBody] CambiarContrasenaDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var usuario = await context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null)
                return NotFound(new { error = "Usuario no encontrado" });

            // Verificar contraseña actual
            var passwordHasher = new PasswordHasher<Usuario>();
            var result = passwordHasher.VerifyHashedPassword(
                usuario, usuario.PasswordHash, request.ContrasenaActual);

            if (result == PasswordVerificationResult.Failed)
            {
                logger.LogWarning("Intento de cambio de contraseña con contraseña actual incorrecta: {Email}",
                    usuario.Correo);
                return Unauthorized(new { error = "La contraseña actual es incorrecta" });
            }

            // Cambiar contraseña
            usuario.PasswordHash = passwordHasher.HashPassword(usuario, request.ContrasenaNueva);
            await context.SaveChangesAsync();

            securityLogger.LogSensitiveDataAccess(
                usuario.Correo, "CAMBIAR_CONTRASENA", "CREDENCIALES");

            logger.LogInformation("Contraseña cambiada exitosamente: {Email}", usuario.Correo);

            return Ok(new { mensaje = "Contraseña actualizada exitosamente" });
        }
    }

    /// <summary>
    /// DTO para cambiar contraseña
    /// </summary>
    public class CambiarContrasenaDTO
    {
        [Required(ErrorMessage = "La contraseña actual es requerida")]
        public string ContrasenaActual { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [StringLength(50, MinimumLength = 8)]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "La contraseña debe contener mayúscula, minúscula, número y carácter especial")]
        public string ContrasenaNueva { get; set; }

        [Required(ErrorMessage = "La confirmación es requerida")]
        [Compare("ContrasenaNueva", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmarContrasena { get; set; }
    }
}


