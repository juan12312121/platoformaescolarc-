using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlataformaEscolar.API.Data;
using PlataformaEscolar.API.DTOs;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace PlataformaEscolar.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificacionesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public NotificacionesController(AppDbContext context) { _context = context; }

        // GET /api/notificaciones
        [HttpGet]
        public async Task<IActionResult> GetNotificaciones()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var notifs = await _context.Notificaciones
                .Where(n => n.UsuarioId == userId)
                .OrderByDescending(n => n.CreadoEn)
                .Select(n => new NotificacionDTO
                {
                    Id = n.Id,
                    Mensaje = n.Mensaje,
                    Leida = n.Leida,
                    CreadoEn = n.CreadoEn
                })
                .ToListAsync();
            return Ok(notifs);
        }

        // GET /api/notificaciones/no-leidas
        [HttpGet("no-leidas")]
        public async Task<IActionResult> GetNoLeidas()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var count = await _context.Notificaciones
                .CountAsync(n => n.UsuarioId == userId && !n.Leida);
            return Ok(new { noLeidas = count });
        }

        // PUT /api/notificaciones/{id}/leer
        [HttpPut("{id}/leer")]
        public async Task<IActionResult> MarcarLeida(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var notif = await _context.Notificaciones
                .FirstOrDefaultAsync(n => n.Id == id && n.UsuarioId == userId);
            if (notif == null) return NotFound();
            notif.Leida = true;
            await _context.SaveChangesAsync();
            return Ok("Notificación marcada como leída");
        }

        // PUT /api/notificaciones/leer-todas
        [HttpPut("leer-todas")]
        public async Task<IActionResult> MarcarTodasLeidas()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var notifs = await _context.Notificaciones
                .Where(n => n.UsuarioId == userId && !n.Leida)
                .ToListAsync();
            notifs.ForEach(n => n.Leida = true);
            await _context.SaveChangesAsync();
            return Ok($"{notifs.Count} notificaciones marcadas como leídas");
        }

        // DELETE /api/notificaciones/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var notif = await _context.Notificaciones
                .FirstOrDefaultAsync(n => n.Id == id && n.UsuarioId == userId);
            if (notif == null) return NotFound();
            _context.Notificaciones.Remove(notif);
            await _context.SaveChangesAsync();
            return Ok("Notificación eliminada");
        }
    }
}
