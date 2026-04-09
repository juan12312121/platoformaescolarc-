namespace PlataformaEscolar.API.DTOs
{
    public class NotificacionDTO
    {
        public int Id { get; set; }
        public string? Mensaje { get; set; }
        public bool Leida { get; set; }
        public DateTime CreadoEn { get; set; }
    }
}
