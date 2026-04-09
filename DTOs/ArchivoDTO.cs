namespace PlataformaEscolar.API.DTOs
{
    public class ArchivoDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Url { get; set; }
        public DateTime SubidoEn { get; set; }
    }

    public class CrearArchivoDTO
    {
        public string Nombre { get; set; }
        public string Url { get; set; }
    }
}
