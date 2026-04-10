using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlataformaEscolar.API.Data;
using PlataformaEscolar.API.DTOs;

namespace PlataformaEscolar.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InscripcionesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InscripcionesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/inscripciones/curso/{cursoId} (listar alumnos inscritos)
        [HttpGet("curso/{cursoId}")]
        [Authorize(Roles = "Profesor,Alumno,Admin")]
        public async Task<ActionResult<IEnumerable<InscripcionDetalleDTO>>> GetInscripcionesPorCurso(int cursoId)
        {
            var inscripciones = await _context.Inscripciones
                .Where(i => i.CursoId == cursoId)
                .Select(i => new InscripcionDetalleDTO
                {
                    Id = i.Id, UsuarioId = i.UsuarioId, 
                    UsuarioNombre = i.Usuario.Nombre,
                    UsuarioCorreo = i.Usuario.Correo,
                    CursoNombre = i.Curso.Nombre,
                    Rol = i.Rol,
                    CreadoEn = i.CreadoEn
                })
                .ToListAsync();

            return Ok(inscripciones);
        }

        // GET: api/inscripciones (todas - solo Admin)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<InscripcionDetalleDTO>>> GetInscripciones()
        {
            var inscripciones = await _context.Inscripciones
                .Select(i => new InscripcionDetalleDTO
                {
                    Id = i.Id, UsuarioId = i.UsuarioId, 
                    UsuarioNombre = i.Usuario.Nombre,
                    UsuarioCorreo = i.Usuario.Correo,
                    CursoNombre = i.Curso.Nombre,
                    Rol = i.Rol,
                    CreadoEn = i.CreadoEn
                })
                .ToListAsync();

            return Ok(inscripciones);
        }

        // DELETE: api/inscripciones/{id} (desinscribir)
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteInscripcion(int id)
        {
            var inscripcion = await _context.Inscripciones.FindAsync(id);
            if (inscripcion == null)
                return NotFound("InscripciÃ³n no encontrada");

            _context.Inscripciones.Remove(inscripcion);
            await _context.SaveChangesAsync();

            return Ok("DesinscripciÃ³n realizada");
        }
    }
}



