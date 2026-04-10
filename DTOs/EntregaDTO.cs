namespace PlataformaEscolar.API.DTOs
{
    public class EntregaDTO
    {
        public int Id { get; set; }
        public int TareaId { get; set; }
        public int AlumnoId { get; set; }
        public string Contenido { get; set; }
        public string ArchivoUrl { get; set; }
        public DateTime EntregadoEn { get; set; }
    }
    public class EntregaDetalleDTO
    {
        public int Id { get; set; }
        public int TareaId { get; set; }
        public string TareaTitulo { get; set; }
        public string AlumnoNombre { get; set; }
        public string Contenido { get; set; }
        public string ArchivoUrl { get; set; }
        public DateTime EntregadoEn { get; set; }
        public CalificacionDTO Calificacion { get; set; }
    }
}
