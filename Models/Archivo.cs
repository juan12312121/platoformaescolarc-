namespace PlataformaEscolar.API.Models
{
using System.ComponentModel.DataAnnotations.Schema;

[Table("Archivos")]
public class Archivo
{
    public int Id { get; set; }

    public string Nombre { get; set; }
    public string Url { get; set; }

    public DateTime SubidoEn { get; set; }
}
}

