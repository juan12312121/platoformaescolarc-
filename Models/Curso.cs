namespace PlataformaEscolar.API.Models
{
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Cursos")]
public class Curso
{
    public int Id { get; set; }

    [Required]
    public string Nombre { get; set; }

    public string Descripcion { get; set; }

    public string Codigo { get; set; }

    public int ProfesorId { get; set; }

    [ForeignKey("ProfesorId")]
    public Usuario Profesor { get; set; }

    public DateTime CreadoEn { get; set; }

    public ICollection<Inscripcion> Inscripciones { get; set; }
}
}
