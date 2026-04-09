namespace PlataformaEscolar.API.DTOs
{
    public class ComentarioDTO
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int TareaId { get; set; }
        public string Contenido { get; set; }
        public DateTime CreadoEn { get; set; }
    }
    public class ComentarioDetalleDTO
    {
        public int Id { get; set; }
        public string UsuarioNombre { get; set; }
        public string TareaTitulo { get; set; }
        public string Contenido { get; set; }
        public DateTime CreadoEn { get; set; }
    }
}
