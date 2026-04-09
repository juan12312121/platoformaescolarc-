using System.ComponentModel.DataAnnotations;

namespace PlataformaEscolar.API.DTOs
{
    /// <summary>
    /// DTO para entregar tareas
    /// Alumnos envían sus soluciones con validaciones
    /// </summary>
    public class CrearEntregaDTO
    {
        [Required(ErrorMessage = "La tarea es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "Tarea inválida")]
        public int TareaId { get; set; }

        [Required(ErrorMessage = "El contenido es requerido")]
        [StringLength(5000, MinimumLength = 1, 
            ErrorMessage = "El contenido debe tener entre 1 y 5000 caracteres")]
        [RegularExpression(
            @"^[a-záéíóúñA-ZÁÉÍÓÚÑ0-9\s.,!?¿¡\-()\/:\&'""àáäâèéëêìíïîòóöôùúüûñç\n\r@+=\[\]{};:<>|\\]*$",
            ErrorMessage = "El contenido contiene caracteres no permitidos")]
        public string Contenido { get; set; }

        [StringLength(255)]
        public string ArchivoUrl { get; set; } = "";
    }
}
