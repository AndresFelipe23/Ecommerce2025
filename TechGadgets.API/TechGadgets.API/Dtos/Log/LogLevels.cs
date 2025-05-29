using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Log
{
    public class LogLevels
    {
        public const string TRACE = "Trace";
        public const string DEBUG = "Debug";
        public const string INFO = "Information";
        public const string WARNING = "Warning";
        public const string ERROR = "Error";
        public const string CRITICAL = "Critical";

        public static readonly List<string> All = new()
        {
            TRACE, DEBUG, INFO, WARNING, ERROR, CRITICAL
        };

        public static readonly Dictionary<string, int> Priority = new()
        {
            { TRACE, 0 },
            { DEBUG, 1 },
            { INFO, 2 },
            { WARNING, 3 },
            { ERROR, 4 },
            { CRITICAL, 5 }
        };
    }
}