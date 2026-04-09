using System.ComponentModel.DataAnnotations;

namespace PlataformaEscolar.API.DTOs
{
    /// <summary>
    /// DTO para calificar entregas
    /// Solo profesores autorizados pueden usar este endpoint
    /// </summary>
    public class CrearCalificacionDTO
    {
        [Required(ErrorMessage = "El puntaje es requerido")]
        [Range(0, 1000, ErrorMessage = "El puntaje debe estar entre 0 y 1000")]
        public decimal Puntaje { get; set; }

        [StringLength(500, ErrorMessage = "La retroalimentación máximo 500 caracteres")]
        [RegularExpression(
            @"^[a-záéíóúñA-ZÁÉÍÓÚÑ0-9\s.,!?¿¡\-()\/:\&'""àáäâèéëêìíïîòóöôùúüûñç\n\r]*$",
            ErrorMessage = "La retroalimentación contiene caracteres no permitidos")]
        public string Retroalimentacion { get; set; } = "";
    }
}
