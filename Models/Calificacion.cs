namespace PlataformaEscolar.API.Models
{
using System.ComponentModel.DataAnnotations.Schema;

[Table("Calificaciones")]
public class Calificacion
{
    public int Id { get; set; }

    public int EntregaId { get; set; }

    public decimal? Puntaje { get; set; }
    public string Retroalimentacion { get; set; }

    public DateTime CalificadoEn { get; set; }

    public Entrega Entrega { get; set; }
}
}

