namespace PlataformaEscolar.API.DTOs
{
    public class UsuarioDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Rol { get; set; }
        public DateTime CreadoEn { get; set; }
    }
}
