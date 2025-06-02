// 3. SlugService.cs - AGREGAR el método GenerateSlugAsync que falta
using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechGadgets.API.Data.Context;
using TechGadgets.API.Services.Interfaces;

namespace TechGadgets.API.Services.Implementation
{
    public class SlugService : ISlugService
    {
        private readonly TechGadgetsDbContext _context;

        public SlugService(TechGadgetsDbContext context)
        {
            _context = context;
        }

        // ✅ AGREGAR ESTE MÉTODO QUE FALTA
        public async Task<string> GenerateSlugAsync(string input, string tableName, int? excludeId = null)
        {
            var baseSlug = GenerateSlug(input);
            return await GenerateUniqueSlugAsync(baseSlug, async (slug) => 
                await SlugExistsAsync(slug, tableName, excludeId));
        }

        public string GenerateSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Convertir a minúsculas
            text = text.ToLowerInvariant();

            // Remover acentos y caracteres especiales
            text = RemoveAccents(text);

            // Reemplazar espacios y caracteres no alfanuméricos con guiones
            text = Regex.Replace(text, @"[^a-z0-9\s-]", "");
            text = Regex.Replace(text, @"\s+", "-");
            text = Regex.Replace(text, @"-+", "-");

            // Remover guiones al inicio y final
            text = text.Trim('-');

            // Limitar longitud
            if (text.Length > 50)
                text = text.Substring(0, 50).Trim('-');

            return text;
        }

        public async Task<string> GenerateUniqueSlugAsync(string baseSlug, Func<string, Task<bool>> existsFunc)
        {
            if (string.IsNullOrWhiteSpace(baseSlug))
                return Guid.NewGuid().ToString("N")[..8];

            var slug = baseSlug;
            var counter = 1;

            while (await existsFunc(slug))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;

                if (counter > 1000)
                {
                    slug = $"{baseSlug}-{Guid.NewGuid().ToString("N")[..8]}";
                    break;
                }
            }

            return slug;
        }

        private static string RemoveAccents(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        private async Task<bool> SlugExistsAsync(string slug, string tableName, int? excludeId = null)
        {
            return tableName.ToLower() switch
            {
                "productos" => await _context.Productos.AnyAsync(p => p.PrdSlug == slug && (excludeId == null || p.PrdId != excludeId)),
                "categorias" => await _context.Categorias.AnyAsync(c => c.CatSlug == slug && (excludeId == null || c.CatId != excludeId)),
                "marcas" => await _context.Marcas.AnyAsync(m => m.MarNombre == slug && (excludeId == null || m.MarId != excludeId)),
                _ => false
            };
        }
    }
}