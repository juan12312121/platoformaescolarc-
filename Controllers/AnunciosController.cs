using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using PlataformaEscolar.API.Data;
using PlataformaEscolar.API.Models;

namespace PlataformaEscolar.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnunciosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AnunciosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CrearAnuncio([FromBody] AnuncioDTO request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var anuncio = new Anuncio
            {
                UsuarioId = userId,
                CursoId = request.CursoId,
                Contenido = request.Contenido,
                CreadoEn = DateTime.UtcNow
            };

            _context.Anuncios.Add(anuncio);
            await _context.SaveChangesAsync();

            return Ok(anuncio);
        }

        [HttpGet("curso/{cursoId}")]
        public async Task<IActionResult> GetAnuncios(int cursoId)
        {
            var anuncios = await _context.Anuncios
                .Where(a => a.CursoId == cursoId)
                .Include(a => a.Usuario)
                .OrderByDescending(a => a.CreadoEn)
                .Select(a => new {
                    id = a.Id,
                    usuario = a.Usuario.Nombre,
                    contenido = a.Contenido,
                    creadoEn = a.CreadoEn
                })
                .ToListAsync();

            return Ok(anuncios);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarAnuncio(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userRole = User.FindFirst(ClaimTypes.Role).Value;

            var anuncio = await _context.Anuncios.FindAsync(id);
            if (anuncio == null) return NotFound();

            if (anuncio.UsuarioId != userId && userRole != "Profesor") return Forbid();

            _context.Anuncios.Remove(anuncio);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }

    public class AnuncioDTO
    {
        public int CursoId { get; set; }
        public string Contenido { get; set; }
    }
}
