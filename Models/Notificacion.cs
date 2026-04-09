namespace PlataformaEscolar.API.Models
{
using System.ComponentModel.DataAnnotations.Schema;
[Table("Notificaciones")]
public class Notificacion
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string? Titulo { get; set; }
    public string? Mensaje { get; set; }
    public bool Leida { get; set; }
    public DateTime CreadoEn { get; set; }
}
}
