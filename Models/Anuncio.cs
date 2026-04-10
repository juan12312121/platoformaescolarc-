using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlataformaEscolar.API.Models
{
    [Table("Anuncios")]
    public class Anuncio
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }
        public int CursoId { get; set; }
        [ForeignKey("CursoId")]
        public Curso Curso { get; set; }
        public string Contenido { get; set; }
        public DateTime CreadoEn { get; set; }
    }
}
