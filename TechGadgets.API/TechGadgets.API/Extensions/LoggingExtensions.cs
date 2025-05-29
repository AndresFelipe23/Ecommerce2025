// ====================================
// Extensiones para Configuración
// ====================================

using TechGadgets.API.Logging;
using TechGadgets.API.Middleware;
using TechGadgets.API.Services.Background;
using TechGadgets.API.Services.Implementation;
using TechGadgets.API.Services.Interfaces;

namespace TechGadgets.API.Extensions
{
    public static class LoggingExtensions
    {
        /// <summary>
        /// Configura el sistema de logging personalizado
        /// </summary>
        public static IServiceCollection AddCustomLogging(this IServiceCollection services)
        {
            // Registrar servicios de logging
            services.AddScoped<ILogService, LogService>();
            
            // Registrar servicio de background para limpieza
            services.AddHostedService<LogCleanupBackgroundService>();
            
            return services;
        }

        /// <summary>
        /// Configura el middleware de logging
        /// </summary>
        public static IApplicationBuilder UseCustomLogging(this IApplicationBuilder app)
        {
            // Middleware de manejo global de excepciones
            app.UseMiddleware<GlobalExceptionMiddleware>();
            
            // Middleware de logging de requests
            app.UseMiddleware<RequestLoggingMiddleware>();
            
            return app;
        }

        /// <summary>
        /// Configura el provider de logging para base de datos
        /// </summary>
        public static ILoggingBuilder AddDatabaseLogging(this ILoggingBuilder builder, IServiceProvider serviceProvider)
        {
            builder.AddProvider(new DatabaseLoggerProvider(serviceProvider, LogLevel.Warning));
            return builder;
        }
    }
}
