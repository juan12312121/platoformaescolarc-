using System.ComponentModel.DataAnnotations;

namespace PlataformaEscolar.API.DTOs
{
    /// <summary>
    /// DTO para crear comentarios en tareas
    /// Protección contra XSS con validación de caracteres
    /// </summary>
    public class CrearComentarioDTO
    {
        [Required(ErrorMessage = "La tarea es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "Tarea inválida")]
        public int TareaId { get; set; }

        [Required(ErrorMessage = "El contenido es requerido")]
        [StringLength(1000, MinimumLength = 1, 
            ErrorMessage = "El contenido debe tener entre 1 y 1000 caracteres")]
        [RegularExpression(
            @"^[a-záéíóúñA-ZÁÉÍÓÚÑ0-9\s.,!?¿¡\-()\/:\&'""àáäâèéëêìíïîòóöôùúüûñç\n\r]*$", 
            ErrorMessage = "El contenido contiene caracteres no permitidos")]
        public string Contenido { get; set; }
    }
}
