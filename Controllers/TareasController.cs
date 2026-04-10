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
    public class TareasController : ControllerBase
    {
        private readonly AppDbContext context;
        private readonly ISecurityLogger securityLogger;
        private readonly ILogger<TareasController> logger;

        public TareasController(
            AppDbContext context,
            ISecurityLogger securityLogger,
            ILogger<TareasController> logger)
        {
            this.context = context;
            this.securityLogger = securityLogger;
            this.logger = logger;
        }

        /// <summary>
        /// Crear tarea (solo profesores)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> CrearTarea([FromBody] CrearTareaDTO request)
        {
            // ModelState validado automáticamente
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var profesorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var profesorEmail = User.FindFirst(ClaimTypes.Email).Value;

            logger.LogInformation("Profesor {ProfesorId} creando tarea en curso {CursoId}",
                profesorId, request.CursoId);

            // Verificar que el profesor es dueño del curso
            var curso = await context.Cursos
                .FirstOrDefaultAsync(c => c.Id == request.CursoId && c.ProfesorId == profesorId);

            if (curso == null)
            {
                logger.LogWarning("Intento de crear tarea sin permiso en curso {CursoId}", request.CursoId);
                return Forbid();
            }

            var tarea = new Tarea
            {
                CursoId = request.CursoId,
                Titulo = request.Titulo.Trim(),
                Descripcion = request.Descripcion.Trim(),
                FechaEntrega = request.FechaEntrega,
                PuntajeMaximo = request.PuntajeMaximo,
                CreadoEn = DateTime.UtcNow
            };

            try
            {
                await context.Tareas.AddAsync(tarea);
                await context.SaveChangesAsync();

                // Crear notificaciones para todos los alumnos del curso
                var alumnosDelCurso = await context.Inscripciones
                    .Where(i => i.CursoId == request.CursoId && i.Rol == "Alumno")
                    .Select(i => i.UsuarioId)
                    .ToListAsync();

                var notificaciones = alumnosDelCurso.Select(alumnoId => new Notificacion
                {
                    UsuarioId = alumnoId,
                    Titulo = "Nueva Tarea Publicada",
                    Mensaje = $"Nueva tarea: {request.Titulo}",
                    Leida = false,
                    CreadoEn = DateTime.UtcNow
                }).ToList();

                if (notificaciones.Any())
                {
                    await context.Notificaciones.AddRangeAsync(notificaciones);
                    await context.SaveChangesAsync();
                }

                securityLogger.LogSensitiveDataAccess(
                    profesorEmail, "CREAR_TAREA", $"CURSO_{request.CursoId}");

                logger.LogInformation("Tarea creada exitosamente: {TareaId}", tarea.Id);

                return Ok(new
                {
                    id = tarea.Id,
                    titulo = tarea.Titulo,
                    mensaje = "Tarea creada y notificaciones enviadas",
                    notificacionesEnviadas = notificaciones.Count
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creando tarea");
                return StatusCode(500, new { error = "Error al crear tarea" });
            }
        }

        /// <summary>
        /// Obtener tareas de un curso
        /// </summary>
        [HttpGet("curso/{cursoId}")]
        [Authorize]
        public async Task<IActionResult> ObtenerTareasPorCurso(int cursoId)
        {
            var tareas = await context.Tareas
                .AsNoTracking()
                .Where(t => t.CursoId == cursoId)
                .OrderByDescending(t => t.FechaEntrega)
                .Select(t => new
                {
                    id = t.Id,
                    titulo = t.Titulo,
                    descripcion = (t.Descripcion != null && t.Descripcion.Length > 100) 
                        ? t.Descripcion.Substring(0, 100) + "..." 
                        : (t.Descripcion ?? ""),
                    fechaEntrega = t.FechaEntrega,
                    puntajeMaximo = t.PuntajeMaximo,
                    creadoEn = t.CreadoEn
                })
                .ToListAsync();

            return Ok(tareas);
        }

        /// <summary>
        /// Obtener tarea por ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> ObtenerTarea(int id)
        {
            var tarea = await context.Tareas
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tarea == null)
                return NotFound(new { error = "Tarea no encontrada" });

            return Ok(new
            {
                id = tarea.Id,
                titulo = tarea.Titulo,
                descripcion = tarea.Descripcion,
                fechaEntrega = tarea.FechaEntrega,
                puntajeMaximo = tarea.PuntajeMaximo,
                creadoEn = tarea.CreadoEn
            });
        }

        /// <summary>
        /// Eliminar tarea (solo el profesor propietario del curso)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> EliminarTarea(int id)
        {
            var profesorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var tarea = await context.Tareas
                .Include(t => t.Curso)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tarea == null)
                return NotFound(new { error = "Tarea no encontrada" });

            if (tarea.Curso.ProfesorId != profesorId)
            {
                logger.LogWarning("Intento no autorizado de eliminar tarea {TareaId} por profesor {ProfesorId}",
                    id, profesorId);
                return Forbid();
            }

            try
            {
                context.Tareas.Remove(tarea);
                await context.SaveChangesAsync();

                logger.LogInformation("Tarea eliminada: {TareaId}", id);
                return Ok(new { mensaje = "Tarea eliminada exitosamente" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error eliminando tarea {TareaId}", id);
                return StatusCode(500, new { error = "Error al eliminar tarea" });
            }
        }
    }
}


