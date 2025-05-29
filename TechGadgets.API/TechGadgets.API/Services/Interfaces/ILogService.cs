using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechGadgets.API.Dtos.Log;
using TechGadgets.API.Models.Common;

namespace TechGadgets.API.Services.Interfaces
{
    public interface ILogService
    {
        // CRUD Básico
        Task<PagedResult<LogDto>> GetLogsAsync(LogFilterDto filter);
        Task<LogDto?> GetLogByIdAsync(long id);
        Task<LogDto> CreateLogAsync(CreateLogDto dto);
        Task<bool> DeleteLogAsync(long id);

        // Métodos de logging específicos
        Task LogTraceAsync(string message, int? userId = null, string? url = null, string? userAgent = null, string? ip = null);
        Task LogDebugAsync(string message, int? userId = null, string? url = null, string? userAgent = null, string? ip = null);
        Task LogInformationAsync(string message, int? userId = null, string? url = null, string? userAgent = null, string? ip = null);
        Task LogWarningAsync(string message, int? userId = null, string? url = null, string? userAgent = null, string? ip = null);
        Task LogErrorAsync(string message, Exception? exception = null, int? userId = null, string? url = null, string? userAgent = null, string? ip = null);
        Task LogCriticalAsync(string message, Exception? exception = null, int? userId = null, string? url = null, string? userAgent = null, string? ip = null);

        // Métodos de conveniencia para logging con contexto HTTP
        Task LogWithContextAsync(string level, string message, Exception? exception = null);
        Task LogUserActionAsync(string action, int userId, string? details = null);
        Task LogSecurityEventAsync(string eventType, string description, int? userId = null);

        // Estadísticas y reportes
        Task<LogStatsDto> GetLogStatsAsync();
        Task<IEnumerable<LogSummaryDto>> GetRecentErrorsAsync(int count = 10);
        Task<IEnumerable<LogSummaryDto>> GetLogsByUserAsync(int userId, int count = 50);
        Task<Dictionary<string, int>> GetLogCountByLevelAsync(DateTime? from = null, DateTime? to = null);
        Task<Dictionary<string, int>> GetLogCountByDayAsync(int days = 7);

        // Mantenimiento
        Task<int> CleanupLogsAsync(LogCleanupDto dto);
        Task<int> DeleteOldLogsAsync(int daysToKeep = 30);
        Task<long> GetLogCountAsync();
        Task<double> GetLogDatabaseSizeMBAsync();

        // Búsqueda avanzada
        Task<IEnumerable<LogDto>> SearchLogsAsync(string searchTerm, DateTime? from = null, DateTime? to = null, string? level = null);
        Task<IEnumerable<LogDto>> GetLogsByIpAsync(string ip, int count = 100);
        Task<IEnumerable<LogDto>> GetLogsByUrlAsync(string url, int count = 100);

        // Exportación
        Task<byte[]> ExportLogsToCSVAsync(LogFilterDto filter);
        Task<byte[]> ExportLogsToPDFAsync(LogFilterDto filter);
    }
}