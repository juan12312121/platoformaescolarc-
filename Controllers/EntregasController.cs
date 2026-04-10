using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlataformaEscolar.API.Data;
using PlataformaEscolar.API.DTOs;
using PlataformaEscolar.API.Models;
using System.Security.Claims;

namespace PlataformaEscolar.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EntregasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EntregasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/entregas/mias (entregas del alumno logueado)
        [HttpGet("mias")]
        [Authorize(Roles = "Alumno")]
        public async Task<ActionResult<IEnumerable<EntregaDetalleDTO>>> GetMisEntregas()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var entregas = await _context.Entregas
                .Where(e => e.AlumnoId == userId)
                .Select(e => new EntregaDetalleDTO
                {
                    Id = e.Id,
                    TareaId = e.TareaId, AlumnoId = e.AlumnoId,
                    TareaTitulo = e.Tarea.Titulo,
                    AlumnoNombre = e.Alumno.Nombre,
                    Contenido = e.Contenido,
                    ArchivoUrl = e.ArchivoUrl,
                    EntregadoEn = e.EntregadoEn,
                    Calificacion = e.Calificacion != null ? new CalificacionDTO
                    {
                        Id = e.Calificacion.Id,
                        EntregaId = e.Calificacion.EntregaId,
                        Puntaje = e.Calificacion.Puntaje,
                        Retroalimentacion = e.Calificacion.Retroalimentacion,
                        CalificadoEn = e.Calificacion.CalificadoEn
                    } : null
                })
                .ToListAsync();

            return Ok(entregas);
        }

        // GET: api/entregas/tarea/{tareaId} (entregas de una tarea - solo Profesor)
        [HttpGet("tarea/{tareaId}")]
        [Authorize(Roles = "Profesor,Admin")]
        public async Task<ActionResult<IEnumerable<EntregaDetalleDTO>>> GetEntregasPorTarea(int tareaId)
        {
            var entregas = await _context.Entregas
                .Where(e => e.TareaId == tareaId)
                .Select(e => new EntregaDetalleDTO
                {
                    Id = e.Id,
                    TareaId = e.TareaId, AlumnoId = e.AlumnoId,
                    TareaTitulo = e.Tarea.Titulo,
                    AlumnoNombre = e.Alumno.Nombre,
                    Contenido = e.Contenido,
                    ArchivoUrl = e.ArchivoUrl,
                    EntregadoEn = e.EntregadoEn,
                    Calificacion = e.Calificacion != null ? new CalificacionDTO
                    {
                        Id = e.Calificacion.Id,
                        EntregaId = e.Calificacion.EntregaId,
                        Puntaje = e.Calificacion.Puntaje,
                        Retroalimentacion = e.Calificacion.Retroalimentacion,
                        CalificadoEn = e.Calificacion.CalificadoEn
                    } : null
                })
                .ToListAsync();

            return Ok(entregas);
        }

        // POST: api/entregas (crear entrega - solo Alumno)
        [HttpPost]
        [Authorize(Roles = "Alumno")]
        public async Task<ActionResult<EntregaDTO>> CrearEntrega([FromBody] CrearEntregaDTO dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var tarea = await _context.Tareas.FindAsync(dto.TareaId);
            if (tarea == null)
                return NotFound("Tarea no encontrada");

            var yaEntregado = await _context.Entregas
                .AnyAsync(e => e.TareaId == dto.TareaId && e.AlumnoId == userId);
            if (yaEntregado)
                return BadRequest("Ya has entregado esta tarea");

            var entrega = new Entrega
            {
                TareaId = dto.TareaId,
                AlumnoId = userId,
                Contenido = dto.Contenido,
                ArchivoUrl = dto.ArchivoUrl,
                EntregadoEn = DateTime.Now
            };

            _context.Entregas.Add(entrega);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMisEntregas), new { id = entrega.Id }, entrega);
        }

        // DELETE: api/entregas/{id} (anular entrega - solo Alumno)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Alumno")]
        public async Task<IActionResult> AnularEntrega(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var entrega = await _context.Entregas.FindAsync(id);

            if (entrega == null) return NotFound();
            if (entrega.AlumnoId != userId) return Forbid();

            // Si ya tiene calificaciÃ³n, no se puede anular
            var tieneCalificacion = await _context.Calificaciones.AnyAsync(c => c.EntregaId == id);
            if (tieneCalificacion) return BadRequest("No puedes anular una tarea ya calificada");

            _context.Entregas.Remove(entrega);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

