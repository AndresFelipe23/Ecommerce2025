// ====================================
// Implementación del Servicio de Logs
// ====================================

using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TechGadgets.API.Data.Context;
using TechGadgets.API.Dtos.Log;
using TechGadgets.API.Models.Common;
using TechGadgets.API.Models.Entities;
using TechGadgets.API.Services.Interfaces;

namespace TechGadgets.API.Services.Implementation
{
    public class LogService : ILogService
    {
        private readonly TechGadgetsDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<LogService> _logger;

        public LogService(
            TechGadgetsDbContext context, 
            IHttpContextAccessor httpContextAccessor,
            ILogger<LogService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        #region CRUD Básico

        public async Task<PagedResult<LogDto>> GetLogsAsync(LogFilterDto filter)
        {
            var query = BuildLogQuery(filter);

            var totalItems = await query.CountAsync();

            var logs = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(l => MapToLogDto(l))
                .ToListAsync();

            return new PagedResult<LogDto>
            {
                Items = logs,
                TotalItems = totalItems,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<LogDto?> GetLogByIdAsync(long id)
        {
            var log = await _context.Logs
                .Include(l => l.LogUsuario)
                .FirstOrDefaultAsync(l => l.LogId == id);

            return log != null ? MapToLogDto(log) : null;
        }

        public async Task<LogDto> CreateLogAsync(CreateLogDto dto)
        {
            var log = new Log
            {
                LogNivel = dto.Nivel,
                LogMensaje = dto.Mensaje,
                LogExcepcion = dto.Excepcion,
                LogUsuarioId = dto.UsuarioId,
                LogDireccionIp = dto.DireccionIP,
                LogUserAgent = dto.UserAgent,
                LogUrl = dto.Url,
                LogFecha = dto.Fecha ?? DateTime.UtcNow
            };

            _context.Logs.Add(log);
            await _context.SaveChangesAsync();

            return await GetLogByIdAsync(log.LogId) ?? throw new InvalidOperationException("Error al crear el log");
        }

        public async Task<bool> DeleteLogAsync(long id)
        {
            var log = await _context.Logs.FindAsync(id);
            if (log == null) return false;

            _context.Logs.Remove(log);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Métodos de Logging Específicos

        public async Task LogTraceAsync(string message, int? userId = null, string? url = null, string? userAgent = null, string? ip = null)
        {
            await CreateLogEntryAsync(LogLevels.TRACE, message, null, userId, url, userAgent, ip);
        }

        public async Task LogDebugAsync(string message, int? userId = null, string? url = null, string? userAgent = null, string? ip = null)
        {
            await CreateLogEntryAsync(LogLevels.DEBUG, message, null, userId, url, userAgent, ip);
        }

        public async Task LogInformationAsync(string message, int? userId = null, string? url = null, string? userAgent = null, string? ip = null)
        {
            await CreateLogEntryAsync(LogLevels.INFO, message, null, userId, url, userAgent, ip);
        }

        public async Task LogWarningAsync(string message, int? userId = null, string? url = null, string? userAgent = null, string? ip = null)
        {
            await CreateLogEntryAsync(LogLevels.WARNING, message, null, userId, url, userAgent, ip);
        }

        public async Task LogErrorAsync(string message, Exception? exception = null, int? userId = null, string? url = null, string? userAgent = null, string? ip = null)
        {
            await CreateLogEntryAsync(LogLevels.ERROR, message, exception, userId, url, userAgent, ip);
        }

        public async Task LogCriticalAsync(string message, Exception? exception = null, int? userId = null, string? url = null, string? userAgent = null, string? ip = null)
        {
            await CreateLogEntryAsync(LogLevels.CRITICAL, message, exception, userId, url, userAgent, ip);
        }

        public async Task LogWithContextAsync(string level, string message, Exception? exception = null)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
            {
                await CreateLogEntryAsync(level, message, exception);
                return;
            }

            var userId = GetCurrentUserId();
            var url = $"{context.Request.Method} {context.Request.Path}{context.Request.QueryString}";
            var userAgent = context.Request.Headers.UserAgent.ToString();
            var ip = GetClientIpAddress();

            await CreateLogEntryAsync(level, message, exception, userId, url, userAgent, ip);
        }

        public async Task LogUserActionAsync(string action, int userId, string? details = null)
        {
            var message = $"Usuario realizó acción: {action}";
            if (!string.IsNullOrEmpty(details))
                message += $" - {details}";

            await LogInformationAsync(message, userId);
        }

        public async Task LogSecurityEventAsync(string eventType, string description, int? userId = null)
        {
            var message = $"[SEGURIDAD] {eventType}: {description}";
            await LogWarningAsync(message, userId);
        }

        #endregion

        #region Estadísticas y Reportes

        public async Task<LogStatsDto> GetLogStatsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var totalLogs = await _context.Logs.CountAsync();
            var logsHoy = await _context.Logs.CountAsync(l => l.LogFecha >= today);
            var logsSemana = await _context.Logs.CountAsync(l => l.LogFecha >= weekStart);
            var logsMes = await _context.Logs.CountAsync(l => l.LogFecha >= monthStart);

            var erroresHoy = await _context.Logs.CountAsync(l => l.LogFecha >= today && l.LogNivel == LogLevels.ERROR);
            var warningsHoy = await _context.Logs.CountAsync(l => l.LogFecha >= today && l.LogNivel == LogLevels.WARNING);
            var infosHoy = await _context.Logs.CountAsync(l => l.LogFecha >= today && l.LogNivel == LogLevels.INFO);

            var logsPorNivel = await _context.Logs
                .Where(l => l.LogFecha >= today.AddDays(-30))
                .GroupBy(l => l.LogNivel)
                .Select(g => new { Nivel = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Nivel, x => x.Count);

            var logsPorDia = await _context.Logs
                .Where(l => l.LogFecha >= today.AddDays(-7))
                .GroupBy(l => l.LogFecha!.Value.Date)
                .Select(g => new { Fecha = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Fecha.ToString("dd/MM"), x => x.Count);

            var ultimosErrores = await _context.Logs
                .Include(l => l.LogUsuario)
                .Where(l => l.LogNivel == LogLevels.ERROR || l.LogNivel == LogLevels.CRITICAL)
                .OrderByDescending(l => l.LogFecha)
                .Take(5)
                .Select(l => new LogSummaryDto
                {
                    Id = l.LogId,
                    Nivel = l.LogNivel,
                    Mensaje = l.LogMensaje.Length > 100 ? l.LogMensaje.Substring(0, 100) + "..." : l.LogMensaje,
                    UsuarioNombre = l.LogUsuario != null ? l.LogUsuario.UsuNombre : null,
                    Fecha = l.LogFecha ?? DateTime.MinValue
                })
                .ToListAsync();

            return new LogStatsDto
            {
                TotalLogs = totalLogs,
                LogsHoy = logsHoy,
                LogsEstasemana = logsSemana,
                LogsEsteMes = logsMes,
                ErroresHoy = erroresHoy,
                WarningsHoy = warningsHoy,
                InfosHoy = infosHoy,
                LogsPorNivel = logsPorNivel,
                LogsPorDia = logsPorDia,
                UltimosErrores = ultimosErrores
            };
        }

        public async Task<IEnumerable<LogSummaryDto>> GetRecentErrorsAsync(int count = 10)
        {
            return await _context.Logs
                .Include(l => l.LogUsuario)
                .Where(l => l.LogNivel == LogLevels.ERROR || l.LogNivel == LogLevels.CRITICAL)
                .OrderByDescending(l => l.LogFecha)
                .Take(count)
                .Select(l => new LogSummaryDto
                {
                    Id = l.LogId,
                    Nivel = l.LogNivel,
                    Mensaje = l.LogMensaje.Length > 200 ? l.LogMensaje.Substring(0, 200) + "..." : l.LogMensaje,
                    UsuarioNombre = l.LogUsuario != null ? l.LogUsuario.UsuNombre : null,
                    Fecha = l.LogFecha ?? DateTime.MinValue
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<LogSummaryDto>> GetLogsByUserAsync(int userId, int count = 50)
        {
            return await _context.Logs
                .Include(l => l.LogUsuario)
                .Where(l => l.LogUsuarioId == userId)
                .OrderByDescending(l => l.LogFecha)
                .Take(count)
                .Select(l => new LogSummaryDto
                {
                    Id = l.LogId,
                    Nivel = l.LogNivel,
                    Mensaje = l.LogMensaje.Length > 150 ? l.LogMensaje.Substring(0, 150) + "..." : l.LogMensaje,
                    UsuarioNombre = l.LogUsuario != null ? l.LogUsuario.UsuNombre : null,
                    Fecha = l.LogFecha ?? DateTime.MinValue
                })
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetLogCountByLevelAsync(DateTime? from = null, DateTime? to = null)
        {
            var query = _context.Logs.AsQueryable();

            if (from.HasValue)
                query = query.Where(l => l.LogFecha >= from.Value);

            if (to.HasValue)
                query = query.Where(l => l.LogFecha <= to.Value);

            return await query
                .GroupBy(l => l.LogNivel)
                .Select(g => new { Nivel = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Nivel, x => x.Count);
        }

        public async Task<Dictionary<string, int>> GetLogCountByDayAsync(int days = 7)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-days);

            return await _context.Logs
                .Where(l => l.LogFecha >= startDate)
                .GroupBy(l => l.LogFecha!.Value.Date)
                .Select(g => new { Fecha = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Fecha.ToString("dd/MM/yyyy"), x => x.Count);
        }

        #endregion

        #region Mantenimiento

        public async Task<int> CleanupLogsAsync(LogCleanupDto dto)
        {
            var query = _context.Logs.Where(l => l.LogFecha < dto.FechaLimite);

            if (!dto.EliminarTodos && dto.NivelesAEliminar?.Any() == true)
            {
                query = query.Where(l => dto.NivelesAEliminar.Contains(l.LogNivel));
            }

            var logsToDelete = await query.ToListAsync();
            var count = logsToDelete.Count;

            if (count > 0)
            {
                _context.Logs.RemoveRange(logsToDelete);
                await _context.SaveChangesAsync();
            }

            return count;
        }

        public async Task<int> DeleteOldLogsAsync(int daysToKeep = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            
            var logsToDelete = await _context.Logs
                .Where(l => l.LogFecha < cutoffDate)
                .ToListAsync();

            var count = logsToDelete.Count;

            if (count > 0)
            {
                _context.Logs.RemoveRange(logsToDelete);
                await _context.SaveChangesAsync();
            }

            return count;
        }

        public async Task<long> GetLogCountAsync()
        {
            return await _context.Logs.LongCountAsync();
        }

        public async Task<double> GetLogDatabaseSizeMBAsync()
        {
            // Aproximación del tamaño basado en el número de registros
            var count = await GetLogCountAsync();
            return Math.Round(count * 0.5 / 1024, 2); // Estimación de 0.5KB por log
        }

        #endregion

        #region Búsqueda Avanzada

        public async Task<IEnumerable<LogDto>> SearchLogsAsync(string searchTerm, DateTime? from = null, DateTime? to = null, string? level = null)
        {
            var query = _context.Logs
                .Include(l => l.LogUsuario)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(l => l.LogMensaje.Contains(searchTerm) || 
                                       (l.LogExcepcion != null && l.LogExcepcion.Contains(searchTerm)));
            }

            if (from.HasValue)
                query = query.Where(l => l.LogFecha >= from.Value);

            if (to.HasValue)
                query = query.Where(l => l.LogFecha <= to.Value);

            if (!string.IsNullOrEmpty(level))
                query = query.Where(l => l.LogNivel == level);

            var logs = await query
                .OrderByDescending(l => l.LogFecha)
                .Take(100)
                .ToListAsync();

            return logs.Select(MapToLogDto);
        }

        public async Task<IEnumerable<LogDto>> GetLogsByIpAsync(string ip, int count = 100)
        {
            var logs = await _context.Logs
                .Include(l => l.LogUsuario)
                .Where(l => l.LogDireccionIp == ip)
                .OrderByDescending(l => l.LogFecha)
                .Take(count)
                .ToListAsync();

            return logs.Select(MapToLogDto);
        }

        public async Task<IEnumerable<LogDto>> GetLogsByUrlAsync(string url, int count = 100)
        {
            var logs = await _context.Logs
                .Include(l => l.LogUsuario)
                .Where(l => l.LogUrl != null && l.LogUrl.Contains(url))
                .OrderByDescending(l => l.LogFecha)
                .Take(count)
                .ToListAsync();

            return logs.Select(MapToLogDto);
        }

        #endregion

        #region Exportación

        public async Task<byte[]> ExportLogsToCSVAsync(LogFilterDto filter)
        {
            var logs = await _context.Logs
                .Include(l => l.LogUsuario)
                .Where(l => BuildLogQueryFilter(l, filter))
                .OrderByDescending(l => l.LogFecha)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("ID,Nivel,Mensaje,Usuario,IP,URL,Fecha");

            foreach (var log in logs)
            {
                csv.AppendLine($"{log.LogId},{log.LogNivel},\"{log.LogMensaje.Replace("\"", "\"\"")}\",{log.LogUsuario?.UsuNombre ?? ""},{log.LogDireccionIp},{log.LogUrl},{log.LogFecha:yyyy-MM-dd HH:mm:ss}");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        public async Task<byte[]> ExportLogsToPDFAsync(LogFilterDto filter)
        {
            // Implementación básica - podrías usar iTextSharp o similar para un PDF más elaborado
            var logs = await _context.Logs
                .Include(l => l.LogUsuario)
                .Where(l => BuildLogQueryFilter(l, filter))
                .OrderByDescending(l => l.LogFecha)
                .Take(1000) // Limitar para PDFs
                .ToListAsync();

            var content = new StringBuilder();
            content.AppendLine("REPORTE DE LOGS");
            content.AppendLine($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            content.AppendLine($"Total de registros: {logs.Count}");
            content.AppendLine(new string('-', 80));

            foreach (var log in logs)
            {
                content.AppendLine($"[{log.LogFecha:dd/MM/yyyy HH:mm:ss}] {log.LogNivel}: {log.LogMensaje}");
                if (!string.IsNullOrEmpty(log.LogExcepcion))
                    content.AppendLine($"  Excepción: {log.LogExcepcion}");
                content.AppendLine();
            }

            return Encoding.UTF8.GetBytes(content.ToString());
        }

        #endregion

        #region Métodos Privados

        private async Task CreateLogEntryAsync(string level, string message, Exception? exception = null, 
            int? userId = null, string? url = null, string? userAgent = null, string? ip = null)
        {
            try
            {
                var log = new Log
                {
                    LogNivel = level,
                    LogMensaje = message,
                    LogExcepcion = exception?.ToString(),
                    LogUsuarioId = userId ?? GetCurrentUserId(),
                    LogUrl = url ?? GetCurrentUrl(),
                    LogUserAgent = userAgent ?? GetUserAgent(),
                    LogDireccionIp = ip ?? GetClientIpAddress(),
                    LogFecha = DateTime.UtcNow
                };

                _context.Logs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // No relanzar la excepción para evitar loops infinitos
                _logger.LogError(ex, "Error al crear entrada de log");
            }
        }

        private IQueryable<Log> BuildLogQuery(LogFilterDto filter)
        {
            var query = _context.Logs
                .Include(l => l.LogUsuario)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.Nivel))
                query = query.Where(l => l.LogNivel == filter.Nivel);

            if (!string.IsNullOrEmpty(filter.Mensaje))
                query = query.Where(l => l.LogMensaje.Contains(filter.Mensaje));

            if (filter.UsuarioId.HasValue)
                query = query.Where(l => l.LogUsuarioId == filter.UsuarioId.Value);

            if (!string.IsNullOrEmpty(filter.DireccionIP))
                query = query.Where(l => l.LogDireccionIp == filter.DireccionIP);

            if (filter.FechaDesde.HasValue)
                query = query.Where(l => l.LogFecha >= filter.FechaDesde.Value);

            if (filter.FechaHasta.HasValue)
                query = query.Where(l => l.LogFecha <= filter.FechaHasta.Value);

            if (!string.IsNullOrEmpty(filter.Url))
                query = query.Where(l => l.LogUrl != null && l.LogUrl.Contains(filter.Url));

            // Ordenamiento
            query = filter.SortBy?.ToLower() switch
            {
                "nivel" => filter.SortDescending ? query.OrderByDescending(l => l.LogNivel) : query.OrderBy(l => l.LogNivel),
                "mensaje" => filter.SortDescending ? query.OrderByDescending(l => l.LogMensaje) : query.OrderBy(l => l.LogMensaje),
                "usuario" => filter.SortDescending ? query.OrderByDescending(l => l.LogUsuario!.UsuNombre) : query.OrderBy(l => l.LogUsuario!.UsuNombre),
                _ => filter.SortDescending ? query.OrderByDescending(l => l.LogFecha) : query.OrderBy(l => l.LogFecha)
            };

            return query;
        }

        private static bool BuildLogQueryFilter(Log log, LogFilterDto filter)
        {
            if (!string.IsNullOrEmpty(filter.Nivel) && log.LogNivel != filter.Nivel)
                return false;

            if (!string.IsNullOrEmpty(filter.Mensaje) && !log.LogMensaje.Contains(filter.Mensaje))
                return false;

            if (filter.UsuarioId.HasValue && log.LogUsuarioId != filter.UsuarioId.Value)
                return false;

            if (!string.IsNullOrEmpty(filter.DireccionIP) && log.LogDireccionIp != filter.DireccionIP)
                return false;

            if (filter.FechaDesde.HasValue && log.LogFecha < filter.FechaDesde.Value)
                return false;

            if (filter.FechaHasta.HasValue && log.LogFecha > filter.FechaHasta.Value)
                return false;

            if (!string.IsNullOrEmpty(filter.Url) && (log.LogUrl == null || !log.LogUrl.Contains(filter.Url)))
                return false;

            return true;
        }

        private static LogDto MapToLogDto(Log log)
        {
            return new LogDto
            {
                Id = log.LogId,
                Nivel = log.LogNivel,
                Mensaje = log.LogMensaje,
                Excepcion = log.LogExcepcion,
                UsuarioId = log.LogUsuarioId,
                UsuarioNombre = log.LogUsuario?.UsuNombre,
                DireccionIP = log.LogDireccionIp,
                UserAgent = log.LogUserAgent,
                Url = log.LogUrl,
                Fecha = log.LogFecha ?? DateTime.MinValue
            };
        }

        private int? GetCurrentUserId()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.User?.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirst("UserId")?.Value ??
                                 context.User.FindFirst("sub")?.Value ??
                                 context.User.FindFirst("id")?.Value;
                
                if (int.TryParse(userIdClaim, out var userId))
                    return userId;
            }
            return null;
        }

        private string? GetCurrentUrl()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                return $"{context.Request.Method} {context.Request.Path}{context.Request.QueryString}";
            }
            return null;
        }

        private string? GetUserAgent()
        {
            var context = _httpContextAccessor.HttpContext;
            return context?.Request.Headers.UserAgent.ToString();
        }

        private string? GetClientIpAddress()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            // Verificar headers de proxy
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

            // IP remota directa
            return context.Connection.RemoteIpAddress?.ToString();
        }


        Task<LogDto?> ILogService.GetLogByIdAsync(long id)
        {
            throw new NotImplementedException();
        }

      

        Task<LogStatsDto> ILogService.GetLogStatsAsync()
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<LogSummaryDto>> ILogService.GetRecentErrorsAsync(int count)
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<LogSummaryDto>> ILogService.GetLogsByUserAsync(int userId, int count)
        {
            throw new NotImplementedException();
        }



        Task<IEnumerable<LogDto>> ILogService.SearchLogsAsync(string searchTerm, DateTime? from, DateTime? to, string? level)
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<LogDto>> ILogService.GetLogsByIpAsync(string ip, int count)
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<LogDto>> ILogService.GetLogsByUrlAsync(string url, int count)
        {
            throw new NotImplementedException();
        }



        #endregion
    }
}