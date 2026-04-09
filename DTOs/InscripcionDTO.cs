namespace PlataformaEscolar.API.DTOs
{
    public class InscripcionDTO
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int CursoId { get; set; }
        public string Rol { get; set; }
        public DateTime CreadoEn { get; set; }
    }

    public class InscripcionDetalleDTO
    {
        public int Id { get; set; }
        public string UsuarioNombre { get; set; }
        public string CursoNombre { get; set; }
        public string Rol { get; set; }
        public DateTime CreadoEn { get; set; }
    }
}
