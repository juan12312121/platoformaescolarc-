using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using PlataformaEscolar.API.Data;
using PlataformaEscolar.API.Models;
using PlataformaEscolar.API.DTOs;
using PlataformaEscolar.API.Security;

namespace PlataformaEscolar.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ComentariosController : ControllerBase
    {
        private readonly AppDbContext context;
        private readonly ISecurityLogger securityLogger;
        private readonly ILogger<ComentariosController> logger;

        public ComentariosController(
            AppDbContext context,
            ISecurityLogger securityLogger,
            ILogger<ComentariosController> logger)
        {
            this.context = context;
            this.securityLogger = securityLogger;
            this.logger = logger;
        }

        /// <summary>
        /// Crear comentario con validación contra XSS
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CrearComentario([FromBody] CrearComentarioDTO request)
        {
            // ModelState validado automáticamente por Fluent Validation
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userEmail = User.FindFirst(ClaimTypes.Email).Value;

            logger.LogInformation("Usuario {UserId} creando comentario en tarea {TareaId}",
                userId, request.TareaId);

            // Verificar que la tarea existe
            var tareaExiste = await context.Tareas
                .AnyAsync(t => t.Id == request.TareaId);

            if (!tareaExiste)
            {
                logger.LogWarning("Intento de comentar en tarea inexistente: {TareaId}", request.TareaId);
                return NotFound(new { error = "Tarea no encontrada" });
            }

            var comentario = new Comentario
            {
                UsuarioId = userId,
                TareaId = request.TareaId,
                Contenido = request.Contenido.Trim(),  // Eliminar espacios
                CreadoEn = DateTime.UtcNow
            };

            try
            {
                await context.Comentarios.AddAsync(comentario);
                await context.SaveChangesAsync();

                securityLogger.LogSensitiveDataAccess(
                    userEmail, "CREAR_COMENTARIO", $"TAREA_{request.TareaId}");

                logger.LogInformation("Comentario creado exitosamente: {ComentarioId}", comentario.Id);

                return Ok(new
                {
                    id = comentario.Id,
                    contenido = comentario.Contenido,
                    creadoEn = comentario.CreadoEn,
                    mensaje = "Comentario creado exitosamente"
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creando comentario para usuario {UserId}", userId);
                return StatusCode(500, new { error = "Error al crear comentario" });
            }
        }

        /// <summary>
        /// Obtener comentarios de una tarea
        /// </summary>
        [HttpGet("tarea/{tareaId}")]
        [AllowAnonymous]
        public async Task<IActionResult> ObtenerComentarios(int tareaId)
        {
            var comentarios = await context.Comentarios
                .AsNoTracking()
                .Where(c => c.TareaId == tareaId)
                .Include(c => c.Usuario)
                .OrderByDescending(c => c.CreadoEn)
                .Select(c => new
                {
                    id = c.Id,
                    usuario = c.Usuario.Nombre,
                    contenido = c.Contenido,
                    creadoEn = c.CreadoEn
                })
                .ToListAsync();

            return Ok(comentarios);
        }

        /// <summary>
        /// Eliminar comentario (solo propietario o profesor)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarComentario(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userRole = User.FindFirst(ClaimTypes.Role).Value;

            var comentario = await context.Comentarios
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comentario == null)
                return NotFound(new { error = "Comentario no encontrado" });

            // Solo el propietario o profesor pueden eliminar
            if (comentario.UsuarioId != userId && userRole != "Profesor")
            {
                logger.LogWarning("Intento no autorizado de eliminar comentario {ComentarioId} por usuario {UserId}",
                    id, userId);
                return Forbid();
            }

            try
            {
                context.Comentarios.Remove(comentario);
                await context.SaveChangesAsync();

                logger.LogInformation("Comentario eliminado: {ComentarioId}", id);
                return Ok(new { mensaje = "Comentario eliminado" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error eliminando comentario {ComentarioId}", id);
                return StatusCode(500, new { error = "Error al eliminar comentario" });
            }
        }
    }
}

