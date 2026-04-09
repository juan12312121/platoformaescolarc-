using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlataformaEscolar.API.Models
{
    [Table("Comentarios")]
    public class Comentario
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }
        public int TareaId { get; set; }
        [ForeignKey("TareaId")]
        public Tarea Tarea { get; set; }
        public string Contenido { get; set; }
        public DateTime CreadoEn { get; set; }
    }
}
