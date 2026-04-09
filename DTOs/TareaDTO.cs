namespace PlataformaEscolar.API.DTOs
{
    public class TareaDTO
    {
        public int Id { get; set; }
        public int CursoId { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public decimal PuntajeMaximo { get; set; }
        public DateTime CreadoEn { get; set; }
    }
}
