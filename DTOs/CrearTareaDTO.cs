using System;
using System.ComponentModel.DataAnnotations;

namespace PlataformaEscolar.API.DTOs
{
    public class CrearTareaDTO
    {
        [Required(ErrorMessage = "El curso es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "Curso inválido")]
        public int CursoId { get; set; }

        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "El título debe tener entre 5 y 200 caracteres")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "La descripción es requerida")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "La descripción debe tener entre 10 y 2000 caracteres")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "La fecha de entrega es requerida")]
        public DateTime FechaEntrega { get; set; }

        [Required(ErrorMessage = "El puntaje máximo es requerido")]
        [Range(1, 1000, ErrorMessage = "El puntaje debe estar entre 1 y 1000")]
        public decimal PuntajeMaximo { get; set; }
    }
}
