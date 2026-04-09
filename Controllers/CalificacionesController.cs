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
    public class CalificacionesController : ControllerBase
    {
        private readonly AppDbContext context;
        private readonly ISecurityLogger securityLogger;
        private readonly ILogger<CalificacionesController> logger;

        public CalificacionesController(
            AppDbContext context,
            ISecurityLogger securityLogger,
            ILogger<CalificacionesController> logger)
        {
            this.context = context;
            this.securityLogger = securityLogger;
            this.logger = logger;
        }

        /// <summary>
        /// Calificar entrega (solo profesores)
        /// </summary>
        [HttpPost("{entregaId}/calificar")]
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> CalificarEntrega(
            int entregaId,
            [FromBody] CrearCalificacionDTO request)
        {
            // ModelState validado automáticamente
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var profesorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var profesorEmail = User.FindFirst(ClaimTypes.Email).Value;

            logger.LogInformation("Profesor {ProfesorId} calificando entrega {EntregaId}",
                profesorId, entregaId);

            // Verificar que la entrega existe
            var entrega = await context.Entregas
                .Include(e => e.Tarea)
                .FirstOrDefaultAsync(e => e.Id == entregaId);

            if (entrega == null)
            {
                logger.LogWarning("Intento de calificar entrega inexistente: {EntregaId}", entregaId);
                return NotFound(new { error = "Entrega no encontrada" });
            }

            // Verificar que el profesor es dueño del curso
            var esProfesorDelCurso = await context.Inscripciones
                .AnyAsync(i => i.CursoId == entrega.Tarea.CursoId && 
                              i.UsuarioId == profesorId && 
                              i.Rol == "Profesor");

            if (!esProfesorDelCurso)
            {
                logger.LogWarning("Profesor {ProfesorId} intenta calificar sin permisos en curso {CursoId}",
                    profesorId, entrega.Tarea.CursoId);
                return Forbid();
            }

            // Verificar si ya existe calificación
            var calificacionExistente = await context.Calificaciones
                .FirstOrDefaultAsync(c => c.EntregaId == entregaId);

            if (calificacionExistente != null)
            {
                // Actualizar calificación existente
                calificacionExistente.Puntaje = request.Puntaje;
                calificacionExistente.Retroalimentacion = request.Retroalimentacion?.Trim() ?? "";
                calificacionExistente.CalificadoEn = DateTime.UtcNow;

                try
                {
                    await context.SaveChangesAsync();
                    logger.LogInformation("Calificación actualizada: {CalificacionId}", calificacionExistente.Id);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error actualizando calificación");
                    return StatusCode(500, new { error = "Error al actualizar calificación" });
                }
            }
            else
            {
                // Crear nueva calificación
                var calificacion = new Calificacion
                {
                    EntregaId = entregaId,
                    Puntaje = request.Puntaje,
                    Retroalimentacion = request.Retroalimentacion?.Trim() ?? "",
                    CalificadoEn = DateTime.UtcNow
                };

                try
                {
                    await context.Calificaciones.AddAsync(calificacion);
                    await context.SaveChangesAsync();

                    logger.LogInformation("Calificación creada: {CalificacionId}", calificacion.Id);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error creando calificación");
                    return StatusCode(500, new { error = "Error al crear calificación" });
                }
            }

            // Crear notificación para el alumno
            var notificacion = new Notificacion
            {
                UsuarioId = entrega.AlumnoId,
                Titulo = "Tarea Calificada",
                Mensaje = $"Tu entrega fue calificada: {request.Puntaje} puntos",
                Leida = false,
                CreadoEn = DateTime.UtcNow
            };

            await context.Notificaciones.AddAsync(notificacion);
            await context.SaveChangesAsync();

            securityLogger.LogSensitiveDataAccess(
                profesorEmail, "CALIFICAR_ENTREGA", $"ENTREGA_{entregaId}");

            return Ok(new
            {
                mensaje = "Entrega calificada exitosamente",
                puntaje = request.Puntaje,
                retroalimentacion = request.Retroalimentacion
            });
        }

        /// <summary>
        /// Obtener calificación de una entrega
        /// </summary>
        [HttpGet("entrega/{entregaId}")]
        public async Task<IActionResult> ObtenerCalificacion(int entregaId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var entrega = await context.Entregas
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == entregaId);

            if (entrega == null)
                return NotFound(new { error = "Entrega no encontrada" });

            // El alumno solo puede ver su propia calificación
            if (entrega.AlumnoId != userId)
            {
                var userRole = User.FindFirst(ClaimTypes.Role).Value;
                if (userRole != "Profesor")
                {
                    return Forbid();
                }
            }

            var calificacion = await context.Calificaciones
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.EntregaId == entregaId);

            if (calificacion == null)
                return Ok(new { mensaje = "Entrega aún no calificada" });

            return Ok(new
            {
                id = calificacion.Id,
                puntaje = calificacion.Puntaje,
                retroalimentacion = calificacion.Retroalimentacion,
                calificadoEn = calificacion.CalificadoEn
            });
        }

        /// <summary>
        /// Obtener mis calificaciones (para alumnos)
        /// </summary>
        [HttpGet("mias")]
        public async Task<IActionResult> ObtenerMisCalificaciones()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var calificaciones = await context.Calificaciones
                .AsNoTracking()
                .Include(c => c.Entrega)
                .ThenInclude(e => e.Tarea)
                .Where(c => c.Entrega.AlumnoId == userId)
                .OrderByDescending(c => c.CalificadoEn)
                .Select(c => new
                {
                    id = c.Id,
                    tarea = c.Entrega.Tarea.Titulo,
                    puntaje = c.Puntaje,
                    puntajeMaximo = c.Entrega.Tarea.PuntajeMaximo,
                    retroalimentacion = c.Retroalimentacion,
                    calificadoEn = c.CalificadoEn
                })
                .ToListAsync();

            return Ok(calificaciones);
        }
    }
}

