using System;

namespace PlataformaEscolar.API.Models
{
    /// <summary>
    /// Modelo Usuario con campos de seguridad
    /// Almacena intentos fallidos de login y bloqueos temporales
    /// </summary>
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string PasswordHash { get; set; }
        public string Rol { get; set; } // "Profesor" o "Alumno"
        public DateTime CreadoEn { get; set; }

        // ===== CAMPOS DE SEGURIDAD =====
        
        /// <summary>
        /// Contador de intentos fallidos de login
        /// Se resetea después de un login exitoso
        /// </summary>
        public int FailedLoginAttempts { get; set; } = 0;

        /// <summary>
        /// Fecha hasta la cual la cuenta está bloqueada
        /// Null significa que la cuenta está desbloqueada
        /// Bloqueada automáticamente después de 5 intentos fallidos
        /// </summary>
        public DateTime? BloqueadoHasta { get; set; } = null;

        /// <summary>
        /// Verifica si la cuenta está actualmente bloqueada
        /// </summary>
        public bool EstaBloqueado()
        {
            return BloqueadoHasta.HasValue && BloqueadoHasta > DateTime.UtcNow;
        }

        /// <summary>
        /// Desbloquea la cuenta si el tiempo de bloqueo ha pasado
        /// </summary>
        public void DesbloquearSiEsNecesario()
        {
            if (!EstaBloqueado())
            {
                BloqueadoHasta = null;
                FailedLoginAttempts = 0;
            }
        }
    }
}
