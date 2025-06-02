using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Services.Interfaces
{
    public interface ISlugService
    {
        Task<string> GenerateSlugAsync(string input, string tableName, int? excludeId = null);
        string GenerateSlug(string input);
        Task<string> GenerateUniqueSlugAsync(string baseSlug, Func<string, Task<bool>> existsFunc);
    }
}