namespace PlataformaEscolar.API.Models
{
using System.ComponentModel.DataAnnotations.Schema;

[Table("EntregaArchivos")]
public class EntregaArchivo
{
    public int Id { get; set; }

    public int EntregaId { get; set; }
    public int ArchivoId { get; set; }
}
}

