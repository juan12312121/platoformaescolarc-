namespace PlataformaEscolar.API.DTOs
{
    public class CursoDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public int ProfesorId { get; set; }
        public string? ProfesorNombre { get; set; }
    }
}
