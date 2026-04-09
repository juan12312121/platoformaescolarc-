using System.ComponentModel.DataAnnotations;

namespace PlataformaEscolar.API.DTOs
{
    /// <summary>
    /// DTO para registro de nuevos usuarios
    /// Incluye validaciones de seguridad contra XSS e inyección
    /// </summary>
    public class RegisterRequestDTO
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, MinimumLength = 2, 
            ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        [RegularExpression(@"^[a-záéíóúñA-ZÁÉÍÓÚÑ\s'-]*$", 
            ErrorMessage = "El nombre solo puede contener letras y espacios")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        [StringLength(100)]
        public string Correo { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(50, MinimumLength = 8, 
            ErrorMessage = "La contraseña debe tener entre 8 y 50 caracteres")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "La contraseña debe contener mayúscula, minúscula, número y carácter especial (@$!%*?&)")]
        public string Password { get; set; }

        [Required(ErrorMessage = "El rol es requerido")]
        [RegularExpression("^(Profesor|Alumno)$", 
            ErrorMessage = "El rol debe ser 'Profesor' o 'Alumno'")]
        public string Rol { get; set; }
    }
}
