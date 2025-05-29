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

        public async Task<string> GenerateSlugAsync(string input, string tableName, int? excludeId = null)
        {
            var baseSlug = GenerateSlug(input);
            var slug = baseSlug;
            var counter = 1;

            while (await SlugExistsAsync(slug, tableName, excludeId))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            return slug;
        }

        public string GenerateSlug(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Convertir a minúsculas
            var slug = input.ToLowerInvariant();

            // Reemplazar caracteres especiales del español
            slug = slug
                .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
                .Replace("ñ", "n").Replace("ü", "u")
                .Replace("ç", "c");

            // Remover diacríticos
            slug = RemoveDiacritics(slug);

            // Reemplazar espacios y caracteres no válidos con guiones
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"\s+", "-").Trim('-');
            slug = Regex.Replace(slug, @"-+", "-");

            // Limitar longitud
            if (slug.Length > 100)
                slug = slug.Substring(0, 100).TrimEnd('-');

            return slug;
        }

        private async Task<bool> SlugExistsAsync(string slug, string tableName, int? excludeId = null)
        {
            return tableName.ToLower() switch
            {
                "productos" => await _context.Productos.AnyAsync(p => p.PrdSlug == slug && (excludeId == null || p.PrdId != excludeId)),
                "categorias" => await _context.Categorias.AnyAsync(c => c.CatSlug == slug && (excludeId == null || c.CatId != excludeId)),
                "marcas" => await _context.Marcas.AnyAsync(m => m.MarNombre == slug && (excludeId == null || m.MarId != excludeId)), // Las marcas no tienen slug en tu BD
                _ => false
            };
        }

        private static string RemoveDiacritics(string text)
        {
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
    }
}