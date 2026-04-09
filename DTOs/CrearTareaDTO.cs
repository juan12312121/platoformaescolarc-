using System.ComponentModel.DataAnnotations;

namespace PlataformaEscolar.API.DTOs
{
    /// <summary>
    /// DTO para crear tareas
    /// Valida títulos, descripciones y fechas
    /// </summary>
    public class CrearTareaDTO
    {
        [Required(ErrorMessage = "El curso es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "Curso inválido")]
        public int CursoId { get; set; }

        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(200, MinimumLength = 5, 
            ErrorMessage = "El título debe tener entre 5 y 200 caracteres")]
        [RegularExpression(@"^[a-záéíóúñA-ZÁÉÍÓÚÑ0-9\s.,\-()]*$",
            ErrorMessage = "Título contiene caracteres no permitidos")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "La descripción es requerida")]
        [StringLength(2000, MinimumLength = 10, 
            ErrorMessage = "La descripción debe tener entre 10 y 2000 caracteres")]
        [RegularExpression(@"^[a-záéíóúñA-ZÁÉÍÓÚÑ0-9\s.,\-():\n\r]*$",
            ErrorMessage = "Descripción contiene caracteres no permitidos")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "La fecha de entrega es requerida")]
        public DateTime FechaEntrega { get; set; }

        [Required(ErrorMessage = "El puntaje máximo es requerido")]
        [Range(1, 1000, ErrorMessage = "El puntaje debe estar entre 1 y 1000")]
        public decimal PuntajeMaximo { get; set; }
    }
}
