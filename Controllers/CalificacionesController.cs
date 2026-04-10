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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var profesorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var profesorEmail = User.FindFirst(ClaimTypes.Email).Value;

            var entrega = await context.Entregas
                .Include(e => e.Tarea)
                .FirstOrDefaultAsync(e => e.Id == entregaId);

            if (entrega == null)
                return NotFound(new { error = "Entrega no encontrada" });

            // Verificar que el profesor es dueño del curso
            var curso = await context.Cursos.FindAsync(entrega.Tarea.CursoId);
            if (curso == null || curso.ProfesorId != profesorId)
            {
                logger.LogWarning("Profesor {ProfesorId} intenta calificar sin ser dueño del curso {CursoId}",
                    profesorId, entrega.Tarea.CursoId);
                return Forbid();
            }

            var calificacionExistente = await context.Calificaciones
                .FirstOrDefaultAsync(c => c.EntregaId == entregaId);

            if (calificacionExistente != null)
            {
                calificacionExistente.Puntaje = request.Puntaje;
                calificacionExistente.Retroalimentacion = request.Retroalimentacion?.Trim() ?? "";
                calificacionExistente.CalificadoEn = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
            else
            {
                var calificacion = new Calificacion
                {
                    EntregaId = entregaId,
                    Puntaje = request.Puntaje,
                    Retroalimentacion = request.Retroalimentacion?.Trim() ?? "",
                    CalificadoEn = DateTime.UtcNow
                };
                await context.Calificaciones.AddAsync(calificacion);
                await context.SaveChangesAsync();
            }

            // Notificación
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

            securityLogger.LogSensitiveDataAccess(profesorEmail, "CALIFICAR_ENTREGA", $"ENTREGA_{entregaId}");

            return Ok(new { mensaje = "Entrega calificada exitosamente", puntaje = request.Puntaje });
        }

        /// <summary>
        /// Obtener calificación de una entrega
        /// </summary>
        [HttpGet("entrega/{entregaId}")]
        public async Task<IActionResult> ObtenerCalificacion(int entregaId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var entrega = await context.Entregas.AsNoTracking().FirstOrDefaultAsync(e => e.Id == entregaId);

            if (entrega == null) return NotFound(new { error = "Entrega no encontrada" });

            if (entrega.AlumnoId != userId)
            {
                var userRole = User.FindFirst(ClaimTypes.Role).Value;
                if (userRole != "Profesor") return Forbid();
            }

            var calificacion = await context.Calificaciones.AsNoTracking().FirstOrDefaultAsync(c => c.EntregaId == entregaId);
            if (calificacion == null) return Ok(new { mensaje = "Entrega aún no calificada" });

            return Ok(new { id = calificacion.Id, puntaje = calificacion.Puntaje, retroalimentacion = calificacion.Retroalimentacion, calificadoEn = calificacion.CalificadoEn });
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
                .Include(c => c.Entrega).ThenInclude(e => e.Tarea)
                .Where(c => c.Entrega.AlumnoId == userId)
                .OrderByDescending(c => c.CalificadoEn)
                .Select(c => new {
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
