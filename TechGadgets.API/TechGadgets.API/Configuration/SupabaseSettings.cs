using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Configuration
{
    public class SupabaseSettings
    {
        public const string SectionName = "Supabase";
        
        public string Url { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string ServiceKey { get; set; } = string.Empty;
        public SupabaseStorageSettings Storage { get; set; } = new();
    }
}