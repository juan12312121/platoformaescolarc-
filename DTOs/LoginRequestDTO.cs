using System.ComponentModel.DataAnnotations;

namespace PlataformaEscolar.API.DTOs
{
    /// <summary>
    /// DTO para autenticación de usuarios
    /// Validación contra fuerza bruta (rate limiting en controller)
    /// </summary>
    public class LoginRequestDTO
    {
        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        [StringLength(100)]
        public string Correo { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(50, MinimumLength = 8)]
        public string Password { get; set; }
    }
}
