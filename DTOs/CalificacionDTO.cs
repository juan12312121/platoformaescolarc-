namespace PlataformaEscolar.API.DTOs
{
    public class CalificacionDTO
    {
        public int Id { get; set; }
        public int EntregaId { get; set; }
        public decimal? Puntaje { get; set; }
        public string Retroalimentacion { get; set; }
        public DateTime CalificadoEn { get; set; }
    }
}
