using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlataformaEscolar.API.Data;
using PlataformaEscolar.API.DTOs;
using System.Security.Claims;

namespace PlataformaEscolar.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/usuarios (solo Admin)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UsuarioDTO>>> GetUsuarios()
        {
            var usuarios = await _context.Usuarios
                .Select(u => new UsuarioDTO
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Correo = u.Correo,
                    Rol = u.Rol,
                    CreadoEn = u.CreadoEn,
                    FotoUrl = u.FotoUrl
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        // GET: api/usuarios/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<UsuarioDTO>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .Where(u => u.Id == id)
                .Select(u => new UsuarioDTO
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Correo = u.Correo,
                    Rol = u.Rol,
                    CreadoEn = u.CreadoEn,
                    FotoUrl = u.FotoUrl
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
                return NotFound("Usuario no encontrado");

            return Ok(usuario);
        }

        // GET: api/usuarios/perfil/mio (endpoint del usuario logueado)
        [HttpGet("perfil/mio")]
        [Authorize]
        public async Task<ActionResult<UsuarioDTO>> GetMiPerfil()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var usuario = await _context.Usuarios
                .Where(u => u.Id == userId)
                .Select(u => new UsuarioDTO
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Correo = u.Correo,
                    Rol = u.Rol,
                    CreadoEn = u.CreadoEn,
                    FotoUrl = u.FotoUrl
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
                return NotFound("Usuario no encontrado");

            return Ok(usuario);
        }

        // PUT: api/usuarios/perfil/foto
        [HttpPut("perfil/foto")]
        [Authorize]
        public async Task<IActionResult> ActualizarFoto([FromBody] FotoUpdateDTO dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var usuario = await _context.Usuarios.FindAsync(userId);

            if (usuario == null) return NotFound();

            usuario.FotoUrl = dto.FotoUrl;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Foto actualizada", fotoUrl = usuario.FotoUrl });
        }
    }

    public class FotoUpdateDTO
    {
        public string FotoUrl { get; set; }
    }
}
