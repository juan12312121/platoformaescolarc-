namespace PlataformaEscolar.API.DTOs
{
    public class RegisterRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Rol { get; set; } = "Alumno";
    }
}