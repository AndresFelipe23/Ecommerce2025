using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Log
{
    public class LogStatsDto
    {
        public int TotalLogs { get; set; }
        public int LogsHoy { get; set; }
        public int LogsEstasemana { get; set; }
        public int LogsEsteMes { get; set; }
        public int ErroresHoy { get; set; }
        public int WarningsHoy { get; set; }
        public int InfosHoy { get; set; }
        public Dictionary<string, int> LogsPorNivel { get; set; } = new();
        public Dictionary<string, int> LogsPorDia { get; set; } = new();
        public List<LogSummaryDto> UltimosErrores { get; set; } = new();
    }
}