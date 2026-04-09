namespace PlataformaEscolar.API.Models
{
using System.ComponentModel.DataAnnotations.Schema;

[Table("TareaArchivos")]
public class TareaArchivo
{
    public int Id { get; set; }

    public int TareaId { get; set; }
    public int ArchivoId { get; set; }
}
}

