using Microsoft.Extensions.Logging;
using System;

namespace PlataformaEscolar.API.Security
{
    /// <summary>
    /// Servicio para registrar eventos de seguridad
    /// </summary>
    public interface ISecurityLogger
    {
        void LogLoginAttempt(string email, bool success, string ipAddress);
        void LogSuspiciousActivity(string message, string details);
        void LogSensitiveDataAccess(string usuario, string accion, string recurso);
    }

    public class SecurityLogger : ISecurityLogger
    {
        private readonly ILogger logger;

        public SecurityLogger(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger("Security");
        }

        public void LogLoginAttempt(string email, bool success, string ipAddress)
        {
            if (success)
            {
                logger.LogInformation("LOGIN_EXITOSO - Email: {Email}, IP: {IP}, Timestamp: {Timestamp}",
                    email, ipAddress, DateTime.UtcNow);
            }
            else
            {
                logger.LogWarning("LOGIN_FALLIDO - Email: {Email}, IP: {IP}, Timestamp: {Timestamp}",
                    email, ipAddress, DateTime.UtcNow);
            }
        }

        public void LogSuspiciousActivity(string message, string details)
        {
            logger.LogError("ACTIVIDAD_SOSPECHOSA - {Message}: {Details}, Timestamp: {Timestamp}",
                message, details, DateTime.UtcNow);
        }

        public void LogSensitiveDataAccess(string usuario, string accion, string recurso)
        {
            logger.LogInformation(
                "ACCESO_DATOS_SENSIBLES - Usuario: {Usuario}, Acción: {Accion}, Recurso: {Recurso}, Timestamp: {Timestamp}",
                usuario, accion, recurso, DateTime.UtcNow);
        }
    }
}
