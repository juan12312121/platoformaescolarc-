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
    public class CursosController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CursosController(AppDbContext context) { _context = context; }

        // GET /api/cursos
        [HttpGet]
        public async Task<IActionResult> GetCursos()
        {
            var cursos = await _context.Cursos
                .Include(c => c.Profesor)
                .Select(c => new CursoDTO
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    Codigo = c.Codigo,
                    ProfesorId = c.ProfesorId,
                    ProfesorNombre = c.Profesor.Nombre
                })
                .ToListAsync();
            return Ok(cursos);
        }

        // GET /api/cursos/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCurso(int id)
        {
            var curso = await _context.Cursos
                .Include(c => c.Profesor)
                .Include(c => c.Inscripciones)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (curso == null) return NotFound("Curso no encontrado");

            return Ok(new
            {
                curso.Id,
                curso.Nombre,
                curso.Descripcion,
                curso.Codigo,
                curso.ProfesorId,
                ProfesorNombre = curso.Profesor?.Nombre,
                TotalAlumnos = curso.Inscripciones?.Count ?? 0,
                curso.CreadoEn
            });
        }

        // POST /api/cursos
        [HttpPost]
        [Authorize(Roles = "Profesor,Admin")]
        public async Task<IActionResult> CrearCurso([FromBody] CursoDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var curso = new Curso
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Codigo = dto.Codigo,
                ProfesorId = dto.ProfesorId,
                CreadoEn = DateTime.Now
            };

            _context.Cursos.Add(curso);
            await _context.SaveChangesAsync();

            dto.Id = curso.Id;
            return Ok(dto);
        }

        // PUT /api/cursos/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Profesor,Admin")]
        public async Task<IActionResult> EditarCurso(int id, [FromBody] CursoDTO dto)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null) return NotFound("Curso no encontrado");

            curso.Nombre = dto.Nombre;
            curso.Descripcion = dto.Descripcion;
            curso.Codigo = dto.Codigo;

            await _context.SaveChangesAsync();
            return Ok("Curso actualizado");
        }

        // DELETE /api/cursos/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Profesor,Admin")]
        public async Task<IActionResult> EliminarCurso(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null) return NotFound("Curso no encontrado");

            _context.Cursos.Remove(curso);
            await _context.SaveChangesAsync();
            return Ok("Curso eliminado");
        }

        // POST /api/cursos/{cursoId}/inscribirse
        [HttpPost("{cursoId}/inscribirse")]
        [Authorize(Roles = "Alumno")]
        public async Task<IActionResult> Inscribirse(int cursoId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var existe = await _context.Inscripciones
                .AnyAsync(i => i.CursoId == cursoId && i.UsuarioId == userId);
            if (existe) return BadRequest("Ya estás inscrito en este curso");

            _context.Inscripciones.Add(new Inscripcion
            {
                CursoId = cursoId,
                UsuarioId = userId,
                Rol = "Alumno",
                CreadoEn = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return Ok("Inscripción realizada");
        }

        // GET /api/cursos/mis-cursos
        [HttpGet("mis-cursos")]
        [Authorize]
        public async Task<IActionResult> MisCursos()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var rol = User.FindFirst(ClaimTypes.Role)?.Value;

            if (rol == "Profesor")
            {
                var cursos = await _context.Cursos
                    .Where(c => c.ProfesorId == userId)
                    .Select(c => new CursoDTO
                    {
                        Id = c.Id,
                        Nombre = c.Nombre,
                        Descripcion = c.Descripcion,
                        Codigo = c.Codigo,
                        ProfesorId = c.ProfesorId
                    })
                    .ToListAsync();
                return Ok(cursos);
            }
            else
            {
                var cursos = await _context.Inscripciones
                    .Where(i => i.UsuarioId == userId)
                    .Include(i => i.Curso)
                    .Select(i => new CursoDTO
                    {
                        Id = i.Curso.Id,
                        Nombre = i.Curso.Nombre,
                        Descripcion = i.Curso.Descripcion,
                        Codigo = i.Curso.Codigo,
                        ProfesorId = i.Curso.ProfesorId
                    })
                    .ToListAsync();
                return Ok(cursos);
            }
        }
    }
}
