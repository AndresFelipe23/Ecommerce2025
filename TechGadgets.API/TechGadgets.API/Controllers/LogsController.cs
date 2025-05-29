// ====================================
// Controlador del Sistema de Logs
// ====================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TechGadgets.API.Attributes;
using TechGadgets.API.Dtos.Log;
using TechGadgets.API.Models.Common;
using TechGadgets.API.Services.Interfaces;

namespace TechGadgets.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    [SwaggerTag("Gestión de logs")]
    public class LogsController : ControllerBase
    {
        private readonly ILogService _logService;
        private readonly ILogger<LogsController> _logger;

        public LogsController(ILogService logService, ILogger<LogsController> logger)
        {
            _logService = logService;
            _logger = logger;
        }

        #region Consulta de Logs

        /// <summary>
        /// Obtiene una lista paginada de logs con filtros
        /// </summary>
        /// <param name="filter">Filtros de búsqueda</param>
        /// <returns>Lista paginada de logs</returns>
        [HttpGet]
        [RequirePermission("logs.listar")]
        [ProducesResponseType(typeof(PagedResult<LogDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PagedResult<LogDto>>> GetLogs([FromQuery] LogFilterDto filter)
        {
            try
            {
                var result = await _logService.GetLogsAsync(filter);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener logs");
                return BadRequest(new { success = false, message = "Error al obtener logs", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un log específico por su ID
        /// </summary>
        /// <param name="id">ID del log</param>
        /// <returns>Detalles del log</returns>
        [HttpGet("{id:long}")]
        [RequirePermission("logs.ver")]
        [ProducesResponseType(typeof(LogDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<LogDto>> GetLogById(long id)
        {
            try
            {
                var log = await _logService.GetLogByIdAsync(id);
                if (log == null)
                    return NotFound(new { success = false, message = "Log no encontrado" });

                return Ok(new { success = true, data = log });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener log con ID {LogId}", id);
                return BadRequest(new { success = false, message = "Error al obtener log", error = ex.Message });
            }
        }

        /// <summary>
        /// Busca logs con términos específicos
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda</param>
        /// <param name="from">Fecha desde</param>
        /// <param name="to">Fecha hasta</param>
        /// <param name="level">Nivel de log</param>
        /// <returns>Lista de logs encontrados</returns>
        [HttpGet("search")]
        [RequirePermission("logs.buscar")]
        [ProducesResponseType(typeof(IEnumerable<LogDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<LogDto>>> SearchLogs(
            [FromQuery] string searchTerm,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] string? level = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return BadRequest(new { success = false, message = "El término de búsqueda es requerido" });

                var logs = await _logService.SearchLogsAsync(searchTerm, from, to, level);
                return Ok(new { success = true, data = logs });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en búsqueda de logs");
                return BadRequest(new { success = false, message = "Error en búsqueda", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene logs por dirección IP
        /// </summary>
        /// <param name="ip">Dirección IP</param>
        /// <param name="count">Cantidad de logs a retornar</param>
        /// <returns>Lista de logs de la IP especificada</returns>
        [HttpGet("by-ip/{ip}")]
        [RequirePermission("logs.buscar")]
        [ProducesResponseType(typeof(IEnumerable<LogDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<LogDto>>> GetLogsByIp(string ip, [FromQuery] int count = 100)
        {
            try
            {
                var logs = await _logService.GetLogsByIpAsync(ip, count);
                return Ok(new { success = true, data = logs });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener logs por IP {IP}", ip);
                return BadRequest(new { success = false, message = "Error al obtener logs por IP", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene logs por URL
        /// </summary>
        /// <param name="url">URL a buscar</param>
        /// <param name="count">Cantidad de logs a retornar</param>
        /// <returns>Lista de logs de la URL especificada</returns>
        [HttpGet("by-url")]
        [RequirePermission("logs.buscar")]
        [ProducesResponseType(typeof(IEnumerable<LogDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<LogDto>>> GetLogsByUrl([FromQuery] string url, [FromQuery] int count = 100)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url))
                    return BadRequest(new { success = false, message = "La URL es requerida" });

                var logs = await _logService.GetLogsByUrlAsync(url, count);
                return Ok(new { success = true, data = logs });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener logs por URL {URL}", url);
                return BadRequest(new { success = false, message = "Error al obtener logs por URL", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene logs de un usuario específico
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="count">Cantidad de logs a retornar</param>
        /// <returns>Lista de logs del usuario</returns>
        [HttpGet("by-user/{userId:int}")]
        [RequirePermission("logs.buscar")]
        [ProducesResponseType(typeof(IEnumerable<LogSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<LogSummaryDto>>> GetLogsByUser(int userId, [FromQuery] int count = 50)
        {
            try
            {
                var logs = await _logService.GetLogsByUserAsync(userId, count);
                return Ok(new { success = true, data = logs });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener logs del usuario {UserId}", userId);
                return BadRequest(new { success = false, message = "Error al obtener logs del usuario", error = ex.Message });
            }
        }

        #endregion

        #region Estadísticas y Reportes

        /// <summary>
        /// Obtiene estadísticas generales de logs
        /// </summary>
        /// <returns>Estadísticas de logs</returns>
        [HttpGet("stats")]
        [RequirePermission("logs.estadisticas")]
        [ProducesResponseType(typeof(LogStatsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<LogStatsDto>> GetLogStats()
        {
            try
            {
                var stats = await _logService.GetLogStatsAsync();
                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de logs");
                return BadRequest(new { success = false, message = "Error al obtener estadísticas", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene errores recientes
        /// </summary>
        /// <param name="count">Cantidad de errores a retornar</param>
        /// <returns>Lista de errores recientes</returns>
        [HttpGet("recent-errors")]
        [RequirePermission("logs.errores")]
        [ProducesResponseType(typeof(IEnumerable<LogSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<LogSummaryDto>>> GetRecentErrors([FromQuery] int count = 10)
        {
            try
            {
                var errors = await _logService.GetRecentErrorsAsync(count);
                return Ok(new { success = true, data = errors });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener errores recientes");
                return BadRequest(new { success = false, message = "Error al obtener errores recientes", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene conteo de logs por nivel
        /// </summary>
        /// <param name="from">Fecha desde</param>
        /// <param name="to">Fecha hasta</param>
        /// <returns>Conteo por nivel de log</returns>
        [HttpGet("count-by-level")]
        [RequirePermission("logs.estadisticas")]
        [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Dictionary<string, int>>> GetLogCountByLevel(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            try
            {
                var counts = await _logService.GetLogCountByLevelAsync(from, to);
                return Ok(new { success = true, data = counts });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener conteo por nivel");
                return BadRequest(new { success = false, message = "Error al obtener conteo", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene conteo de logs por día
        /// </summary>
        /// <param name="days">Número de días hacia atrás</param>
        /// <returns>Conteo por día</returns>
        [HttpGet("count-by-day")]
        [RequirePermission("logs.estadisticas")]
        [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Dictionary<string, int>>> GetLogCountByDay([FromQuery] int days = 7)
        {
            try
            {
                var counts = await _logService.GetLogCountByDayAsync(days);
                return Ok(new { success = true, data = counts });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener conteo por día");
                return BadRequest(new { success = false, message = "Error al obtener conteo", error = ex.Message });
            }
        }

        #endregion

        #region Creación Manual de Logs

        /// <summary>
        /// Crea un log manualmente
        /// </summary>
        /// <param name="dto">Datos del log a crear</param>
        /// <returns>Log creado</returns>
        [HttpPost]
        [RequirePermission("logs.crear")]
        [ProducesResponseType(typeof(LogDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<LogDto>> CreateLog([FromBody] CreateLogDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Datos inválidos", errors = ModelState });

                // Validar nivel de log
                if (!LogLevels.All.Contains(dto.Nivel))
                    return BadRequest(new { success = false, message = "Nivel de log inválido" });

                var log = await _logService.CreateLogAsync(dto);
                return CreatedAtAction(nameof(GetLogById), new { id = log.Id },
                    new { success = true, message = "Log creado exitosamente", data = log });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear log manualmente");
                return BadRequest(new { success = false, message = "Error al crear log", error = ex.Message });
            }
        }

        /// <summary>
        /// Registra una acción de usuario
        /// </summary>
        /// <param name="request">Datos de la acción</param>
        /// <returns>Confirmación del registro</returns>
        [HttpPost("user-action")]
        [RequirePermission("logs.crear")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> LogUserAction([FromBody] LogUserActionRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Datos inválidos", errors = ModelState });

                await _logService.LogUserActionAsync(request.Accion, request.UsuarioId, request.Detalles);
                return Ok(new { success = true, message = "Acción registrada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar acción de usuario");
                return BadRequest(new { success = false, message = "Error al registrar acción", error = ex.Message });
            }
        }

        /// <summary>
        /// Registra un evento de seguridad
        /// </summary>
        /// <param name="request">Datos del evento de seguridad</param>
        /// <returns>Confirmación del registro</returns>
        [HttpPost("security-event")]
        [RequirePermission("logs.seguridad")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> LogSecurityEvent([FromBody] LogSecurityEventRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Datos inválidos", errors = ModelState });

                await _logService.LogSecurityEventAsync(request.TipoEvento, request.Descripcion, request.UsuarioId);
                return Ok(new { success = true, message = "Evento de seguridad registrado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar evento de seguridad");
                return BadRequest(new { success = false, message = "Error al registrar evento", error = ex.Message });
            }
        }

        #endregion

        #region Mantenimiento

        /// <summary>
        /// Elimina un log específico
        /// </summary>
        /// <param name="id">ID del log</param>
        /// <returns>Confirmación de eliminación</returns>
        [HttpDelete("{id:long}")]
        [RequirePermission("logs.eliminar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteLog(long id)
        {
            try
            {
                var result = await _logService.DeleteLogAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Log no encontrado" });

                return Ok(new { success = true, message = "Log eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar log {LogId}", id);
                return BadRequest(new { success = false, message = "Error al eliminar log", error = ex.Message });
            }
        }

        /// <summary>
        /// Limpia logs basado en criterios específicos
        /// </summary>
        /// <param name="dto">Criterios de limpieza</param>
        /// <returns>Cantidad de logs eliminados</returns>
        [HttpPost("cleanup")]
        [RequirePermission("logs.limpiar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CleanupLogs([FromBody] LogCleanupDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Datos inválidos", errors = ModelState });

                if (dto.FechaLimite > DateTime.UtcNow.AddDays(-1))
                    return BadRequest(new { success = false, message = "La fecha límite debe ser anterior a ayer" });

                var deletedCount = await _logService.CleanupLogsAsync(dto);
                return Ok(new { 
                    success = true, 
                    message = $"{deletedCount} logs eliminados exitosamente", 
                    data = new { count = deletedCount } 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en limpieza de logs");
                return BadRequest(new { success = false, message = "Error en limpieza", error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina logs antiguos (mantenimiento automático)
        /// </summary>
        /// <param name="daysToKeep">Días a conservar</param>
        /// <returns>Cantidad de logs eliminados</returns>
        [HttpPost("delete-old")]
        [RequirePermission("logs.limpiar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteOldLogs([FromBody] DeleteOldLogsRequest request)
        {
            try
            {
                if (request.DiasAConservar < 7)
                    return BadRequest(new { success = false, message = "Debe conservar al menos 7 días de logs" });

                var deletedCount = await _logService.DeleteOldLogsAsync(request.DiasAConservar);
                return Ok(new { 
                    success = true, 
                    message = $"{deletedCount} logs antiguos eliminados exitosamente", 
                    data = new { count = deletedCount } 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar logs antiguos");
                return BadRequest(new { success = false, message = "Error al eliminar logs antiguos", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene información del tamaño de la base de datos de logs
        /// </summary>
        /// <returns>Información de tamaño y conteo</returns>
        [HttpGet("database-info")]
        [RequirePermission("logs.estadisticas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetDatabaseInfo()
        {
            try
            {
                var count = await _logService.GetLogCountAsync();
                var sizeMB = await _logService.GetLogDatabaseSizeMBAsync();

                return Ok(new { 
                    success = true, 
                    data = new { 
                        totalLogs = count, 
                        sizeInMB = sizeMB,
                        sizeFormatted = $"{sizeMB:F2} MB"
                    } 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener información de la base de datos");
                return BadRequest(new { success = false, message = "Error al obtener información", error = ex.Message });
            }
        }

        #endregion

        #region Exportación

        /// <summary>
        /// Exporta logs a CSV
        /// </summary>
        /// <param name="filter">Filtros para la exportación</param>
        /// <returns>Archivo CSV</returns>
        [HttpPost("export/csv")]
        [RequirePermission("logs.exportar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ExportLogsToCSV([FromBody] LogFilterDto filter)
        {
            try
            {
                var csvData = await _logService.ExportLogsToCSVAsync(filter);
                var fileName = $"logs_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                return File(csvData, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al exportar logs a CSV");
                return BadRequest(new { success = false, message = "Error al exportar", error = ex.Message });
            }
        }

        /// <summary>
        /// Exporta logs a PDF
        /// </summary>
        /// <param name="filter">Filtros para la exportación</param>
        /// <returns>Archivo PDF</returns>
        [HttpPost("export/pdf")]
        [RequirePermission("logs.exportar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ExportLogsToPDF([FromBody] LogFilterDto filter)
        {
            try
            {
                var pdfData = await _logService.ExportLogsToPDFAsync(filter);
                var fileName = $"logs_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                return File(pdfData, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al exportar logs a PDF");
                return BadRequest(new { success = false, message = "Error al exportar", error = ex.Message });
            }
        }

        #endregion

        #region Utilidades

        /// <summary>
        /// Obtiene los niveles de log disponibles
        /// </summary>
        /// <returns>Lista de niveles de log</returns>
        [HttpGet("levels")]
        [RequirePermission("logs.ver")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult GetLogLevels()
        {
            return Ok(new { 
                success = true, 
                data = new { 
                    levels = LogLevels.All,
                    priorities = LogLevels.Priority
                } 
            });
        }

        #endregion
    }

    
}