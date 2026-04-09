using Microsoft.IdentityModel.Tokens;
using PlataformaEscolar.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PlataformaEscolar.API.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerarToken(Usuario usuario)
        {
            // Obtener configuración JWT
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(jwtKey))
                throw new InvalidOperationException("JWT Key no está configurada en appsettings.json");

            // ✅ Crear claims con información del usuario
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),      // Para User.FindFirst(ClaimTypes.NameIdentifier)
                new Claim(ClaimTypes.Email, usuario.Correo),                       // Para User.FindFirst(ClaimTypes.Email)
                new Claim(ClaimTypes.Role, usuario.Rol),                           // Para User.FindFirst(ClaimTypes.Role)
                new Claim(ClaimTypes.Name, usuario.Nombre),                        // Bonus: nombre del usuario
            };

            // Crear key de seguridad
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Crear token
            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),  // Token válido por 24 horas
                signingCredentials: credentials
            );

            // Retornar token como string
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }
    }
}