using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlataformaEscolar.API.Models
{
    [Table("Entregas")]
    public class Entrega
    {
        public int Id { get; set; }
        public int TareaId { get; set; }
        [ForeignKey("TareaId")]
        public Tarea Tarea { get; set; }
        public int AlumnoId { get; set; }
        [ForeignKey("AlumnoId")]
        public Usuario Alumno { get; set; }
        public string Contenido { get; set; }
        public string ArchivoUrl { get; set; }
        public DateTime EntregadoEn { get; set; }
        public Calificacion Calificacion { get; set; }
    }
}
