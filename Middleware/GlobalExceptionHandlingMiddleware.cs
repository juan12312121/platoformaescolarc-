using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PlataformaEscolar.API.Middleware
{
    /// <summary>
    /// Middleware global para manejar excepciones no capturadas
    /// Evita exponer detalles internos de la aplicación
    /// </summary>
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;

        public GlobalExceptionHandlingMiddleware(
            RequestDelegate next, 
            ILoggerFactory loggerFactory)
        {
            this.next = next;
            this.logger = loggerFactory.CreateLogger<GlobalExceptionHandlingMiddleware>();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                // Registrar error en logs (solo el servidor lo ve)
                logger.LogError(ex, "Excepción no manejada en {Path}", context.Request.Path);

                // Respuesta genérica al cliente (sin detalles internos)
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    error = "Ocurrió un error interno. Contacta con soporte.",
                    requestId = context.TraceIdentifier
                };

                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}
