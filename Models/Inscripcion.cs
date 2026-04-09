using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlataformaEscolar.API.Models
{
    [Table("Inscripciones")]
    public class Inscripcion
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }
        public int CursoId { get; set; }
        [ForeignKey("CursoId")]
        public Curso Curso { get; set; }
        public string Rol { get; set; } = "Alumno";
        public DateTime CreadoEn { get; set; }
    }
}
