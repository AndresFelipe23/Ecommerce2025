// ====================================
// Middleware de Logging Personalizado
// ====================================

using System.Diagnostics;
using System.Text;
using System.Text.Json;
using TechGadgets.API.Services.Interfaces;

namespace TechGadgets.API.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;
        private readonly List<string> _excludedPaths;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _excludedPaths = new List<string>
            {
                "/health",
                "/metrics",
                "/favicon.ico",
                "/swagger"
            };
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Verificar si debemos excluir esta ruta
            if (ShouldExcludePath(context.Request.Path))
            {
                await _next(context);
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            var originalBodyStream = context.Response.Body;

            try
            {
                // Capturar el cuerpo de la respuesta
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                // Ejecutar el siguiente middleware
                await _next(context);

                stopwatch.Stop();

                // Log de la solicitud
                await LogRequest(context, stopwatch.ElapsedMilliseconds);

                // Copiar el cuerpo de respuesta de vuelta al stream original
                await responseBody.CopyToAsync(originalBodyStream);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                await LogError(context, ex, stopwatch.ElapsedMilliseconds);
                throw;
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }

        private bool ShouldExcludePath(PathString path)
        {
            return _excludedPaths.Any(excluded => path.StartsWithSegments(excluded));
        }

        private async Task LogRequest(HttpContext context, long elapsedMs)
        {
            try
            {
                var logService = context.RequestServices.GetService<ILogService>();
                if (logService == null) return;

                var method = context.Request.Method;
                var path = context.Request.Path + context.Request.QueryString;
                var statusCode = context.Response.StatusCode;
                var userAgent = context.Request.Headers.UserAgent.ToString();
                var ipAddress = GetClientIpAddress(context);
                var userId = GetUserId(context);

                var message = $"{method} {path} - {statusCode} ({elapsedMs}ms)";

                // Determinar el nivel de log basado en el código de estado
                if (statusCode >= 500)
                {
                    await logService.LogErrorAsync(message, null, userId, path, userAgent, ipAddress);
                }
                else if (statusCode >= 400)
                {
                    await logService.LogWarningAsync(message, userId, path, userAgent, ipAddress);
                }
                else if (elapsedMs > 5000) // Requests lentos
                {
                    await logService.LogWarningAsync($"Slow request: {message}", userId, path, userAgent, ipAddress);
                }
                else
                {
                    await logService.LogInformationAsync(message, userId, path, userAgent, ipAddress);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar solicitud en middleware de logging");
            }
        }

        private async Task LogError(HttpContext context, Exception exception, long elapsedMs)
        {
            try
            {
                var logService = context.RequestServices.GetService<ILogService>();
                if (logService == null) return;

                var method = context.Request.Method;
                var path = context.Request.Path + context.Request.QueryString;
                var userAgent = context.Request.Headers.UserAgent.ToString();
                var ipAddress = GetClientIpAddress(context);
                var userId = GetUserId(context);

                var message = $"Unhandled exception in {method} {path} ({elapsedMs}ms)";

                await logService.LogErrorAsync(message, exception, userId, path, userAgent, ipAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar excepción en middleware de logging");
            }
        }

        private static string? GetClientIpAddress(HttpContext context)
        {
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            return context.Connection.RemoteIpAddress?.ToString();
        }

        private static int? GetUserId(HttpContext context)
        {
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirst("UserId")?.Value ??
                                 context.User.FindFirst("sub")?.Value ??
                                 context.User.FindFirst("id")?.Value;

                if (int.TryParse(userIdClaim, out var userId))
                    return userId;
            }
            return null;
        }
    }

    // ====================================
    // Middleware de Manejo Global de Excepciones
    // ====================================

    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                success = false,
                message = "Ha ocurrido un error interno en el servidor",
                error = exception.Message,
                timestamp = DateTime.UtcNow
            };

            context.Response.StatusCode = exception switch
            {
                ArgumentException => StatusCodes.Status400BadRequest,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                InvalidOperationException => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };

            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);

            // Log la excepción usando el servicio de logs
            try
            {
                var logService = context.RequestServices.GetService<ILogService>();
                if (logService != null)
                {
                    await logService.LogWithContextAsync("Critical", 
                        $"Unhandled exception: {exception.Message}", exception);
                }
            }
            catch
            {
                // Evitar loops infinitos si el servicio de logs falla
            }
        }
    }
}

// ====================================
// Provider de Logging Personalizado para Base de Datos
// ====================================

namespace TechGadgets.API.Logging
{
    public class DatabaseLoggerProvider : ILoggerProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly LogLevel _minLevel;

        public DatabaseLoggerProvider(IServiceProvider serviceProvider, LogLevel minLevel = LogLevel.Warning)
        {
            _serviceProvider = serviceProvider;
            _minLevel = minLevel;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new DatabaseLogger(_serviceProvider, categoryName, _minLevel);
        }

        public void Dispose() { }
    }

    public class DatabaseLogger : ILogger
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _categoryName;
        private readonly LogLevel _minLevel;

        public DatabaseLogger(IServiceProvider serviceProvider, string categoryName, LogLevel minLevel)
        {
            _serviceProvider = serviceProvider;
            _categoryName = categoryName;
            _minLevel = minLevel;
        }

        public IDisposable BeginScope<TState>(TState state) => default!;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLevel;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            Task.Run(async () =>
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var logService = scope.ServiceProvider.GetService<ILogService>();
                    if (logService == null) return;

                    var message = formatter(state, exception);
                    var level = MapLogLevel(logLevel);

                    switch (logLevel)
                    {
                        case LogLevel.Trace:
                            await logService.LogTraceAsync($"[{_categoryName}] {message}");
                            break;
                        case LogLevel.Debug:
                            await logService.LogDebugAsync($"[{_categoryName}] {message}");
                            break;
                        case LogLevel.Information:
                            await logService.LogInformationAsync($"[{_categoryName}] {message}");
                            break;
                        case LogLevel.Warning:
                            await logService.LogWarningAsync($"[{_categoryName}] {message}");
                            break;
                        case LogLevel.Error:
                            await logService.LogErrorAsync($"[{_categoryName}] {message}", exception);
                            break;
                        case LogLevel.Critical:
                            await logService.LogCriticalAsync($"[{_categoryName}] {message}", exception);
                            break;
                    }
                }
                catch
                {
                    // Evitar loops infinitos si ocurre un error al loggear
                }
            });
        }

        private static string MapLogLevel(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => "Trace",
                LogLevel.Debug => "Debug",
                LogLevel.Information => "Information",
                LogLevel.Warning => "Warning",
                LogLevel.Error => "Error",
                LogLevel.Critical => "Critical",
                _ => "Information"
            };
        }
    }
}

// ====================================
// Servicio de Background para Limpieza Automática
// ====================================

namespace TechGadgets.API.Services.Background
{
    public class LogCleanupBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LogCleanupBackgroundService> _logger;
        private readonly TimeSpan _interval;

        public LogCleanupBackgroundService(
            IServiceProvider serviceProvider, 
            ILogger<LogCleanupBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _interval = TimeSpan.FromHours(24); // Ejecutar diariamente
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupLogs();
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en servicio de limpieza de logs");
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // Reintentar en 1 hora
                }
            }
        }

        private async Task CleanupLogs()
        {
            using var scope = _serviceProvider.CreateScope();
            var logService = scope.ServiceProvider.GetService<ILogService>();
            
            if (logService == null) return;

            try
            {
                // Eliminar logs de más de 30 días (excepto errores críticos)
                var normalLogsDeleted = await logService.CleanupLogsAsync(new Dtos.Log.LogCleanupDto
                {
                    FechaLimite = DateTime.UtcNow.AddDays(-30),
                    NivelesAEliminar = new List<string> { "Trace", "Debug", "Information" }
                });

                // Eliminar errores y warnings de más de 90 días
                var errorLogsDeleted = await logService.CleanupLogsAsync(new Dtos.Log.LogCleanupDto
                {
                    FechaLimite = DateTime.UtcNow.AddDays(-90),
                    NivelesAEliminar = new List<string> { "Warning", "Error" }
                });

                // Eliminar logs críticos de más de 365 días
                var criticalLogsDeleted = await logService.CleanupLogsAsync(new Dtos.Log.LogCleanupDto
                {
                    FechaLimite = DateTime.UtcNow.AddDays(-365),
                    NivelesAEliminar = new List<string> { "Critical" }
                });

                var totalDeleted = normalLogsDeleted + errorLogsDeleted + criticalLogsDeleted;
                
                if (totalDeleted > 0)
                {
                    _logger.LogInformation("Limpieza automática de logs completada. {TotalDeleted} logs eliminados", totalDeleted);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la limpieza automática de logs");
            }
        }
    }
}


// ====================================
// Configuración en Program.cs o Startup.cs
// ====================================

/*
// En Program.cs, agregar:

// Configurar logging personalizado
builder.Services.AddCustomLogging();

// Configurar logging framework
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// En el pipeline de la aplicación:
var app = builder.Build();

// Configurar middleware personalizado de logging
app.UseCustomLogging();

// Configurar provider de logging para base de datos (después de construir la app)
builder.Logging.AddDatabaseLogging(app.Services);

*/