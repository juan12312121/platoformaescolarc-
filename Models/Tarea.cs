using System.ComponentModel.DataAnnotations.Schema;
using PlataformaEscolar.API.Models;

namespace PlataformaEscolar.API.Models
{
    [Table("Tareas")]
    public class Tarea
    {
        public int Id { get; set; }
        public int CursoId { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public decimal PuntajeMaximo { get; set; }
        public DateTime CreadoEn { get; set; }

        public Curso Curso { get; set; }
    }
}
