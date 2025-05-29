using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TechGadgets.API.Attributes;
using TechGadgets.API.Dtos.Categories;
using TechGadgets.API.Services.Interfaces;

namespace TechGadgets.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [SwaggerTag("Gestión de categorías de productos jerárquicas")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Obtiene todas las categorías con filtros y paginación
        /// </summary>
        [HttpGet]
        [SwaggerOperation(Summary = "Listar categorías", Description = "Obtiene todas las categorías con filtros y paginación")]
        [SwaggerResponse(200, "Lista de categorías obtenida exitosamente")]
        public async Task<ActionResult> GetCategories([FromQuery] CategoryFilterDto filter)
        {
            var categories = await _categoryService.GetCategoriesAsync(filter);
            return Ok(new { success = true, data = categories });
        }

        /// <summary>
        /// Obtiene una categoría específica por ID
        /// </summary>
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Obtener categoría por ID", Description = "Obtiene los detalles de una categoría específica")]
        [SwaggerResponse(200, "Categoría encontrada", typeof(CategoryDto))]
        [SwaggerResponse(404, "Categoría no encontrada")]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound(new { success = false, message = "Categoría no encontrada" });
            }

            return Ok(new { success = true, data = category });
        }

        /// <summary>
        /// Obtiene una categoría por su slug
        /// </summary>
        [HttpGet("slug/{slug}")]
        [SwaggerOperation(Summary = "Obtener categoría por slug", Description = "Obtiene una categoría usando su slug URL-friendly")]
        [SwaggerResponse(200, "Categoría encontrada", typeof(CategoryDto))]
        [SwaggerResponse(404, "Categoría no encontrada")]
        public async Task<ActionResult<CategoryDto>> GetCategoryBySlug(string slug)
        {
            var category = await _categoryService.GetCategoryBySlugAsync(slug);
            if (category == null)
            {
                return NotFound(new { success = false, message = "Categoría no encontrada" });
            }

            return Ok(new { success = true, data = category });
        }

        /// <summary>
        /// Crea una nueva categoría
        /// </summary>
        [HttpPost]
        [RequirePermission("categorias.crear")]
        [SwaggerOperation(Summary = "Crear nueva categoría", Description = "Crea una nueva categoría en el sistema")]
        [SwaggerResponse(201, "Categoría creada exitosamente", typeof(CategoryDto))]
        [SwaggerResponse(400, "Datos inválidos o categoría ya existe")]
        [SwaggerResponse(403, "No tiene permisos para crear categorías")]
        public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            // Verificar si la categoría ya existe
            if (await _categoryService.CategoryExistsAsync(dto.Nombre))
            {
                return BadRequest(new { success = false, message = "Ya existe una categoría con ese nombre" });
            }

            // Validar padre si se especifica
            if (dto.CategoriaPadreId.HasValue)
            {
                var parent = await _categoryService.GetCategoryByIdAsync(dto.CategoriaPadreId.Value);
                if (parent == null)
                {
                    return BadRequest(new { success = false, message = "La categoría padre especificada no existe" });
                }
            }

            var category = await _categoryService.CreateCategoryAsync(dto);
            return CreatedAtAction(nameof(GetCategory), new { id = category.Id },
                new { success = true, message = "Categoría creada exitosamente", data = category });
        }

        /// <summary>
        /// Actualiza una categoría existente
        /// </summary>
        [HttpPut("{id}")]
        [RequirePermission("categorias.editar")]
        [SwaggerOperation(Summary = "Actualizar categoría", Description = "Actualiza una categoría existente")]
        [SwaggerResponse(200, "Categoría actualizada exitosamente", typeof(CategoryDto))]
        [SwaggerResponse(404, "Categoría no encontrada")]
        [SwaggerResponse(400, "Datos inválidos")]
        [SwaggerResponse(403, "No tiene permisos para editar categorías")]
        public async Task<ActionResult<CategoryDto>> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
        {
            // Verificar si el nombre ya existe en otra categoría
            if (await _categoryService.CategoryExistsAsync(dto.Nombre, id))
            {
                return BadRequest(new { success = false, message = "Ya existe otra categoría con ese nombre" });
            }

            // Validar padre si se especifica
            if (dto.CategoriaPadreId.HasValue)
            {
                if (!await _categoryService.IsValidParentAsync(id, dto.CategoriaPadreId.Value))
                {
                    return BadRequest(new { success = false, message = "No se puede asignar como padre una categoría descendiente" });
                }
            }

            try
            {
                var category = await _categoryService.UpdateCategoryAsync(id, dto);
                if (category == null)
                {
                    return NotFound(new { success = false, message = "Categoría no encontrada" });
                }

                return Ok(new { success = true, message = "Categoría actualizada exitosamente", data = category });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Elimina una categoría
        /// </summary>
        [HttpDelete("{id}")]
        [RequirePermission("categorias.eliminar")]
        [SwaggerOperation(Summary = "Eliminar categoría", Description = "Elimina una categoría del sistema")]
        [SwaggerResponse(200, "Categoría eliminada exitosamente")]
        [SwaggerResponse(404, "Categoría no encontrada")]
        [SwaggerResponse(400, "No se puede eliminar la categoría porque tiene dependencias")]
        [SwaggerResponse(403, "No tiene permisos para eliminar categorías")]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            var hasChildren = await _categoryService.CategoryHasChildrenAsync(id);
            var hasProducts = await _categoryService.CategoryHasProductsAsync(id);

            if (hasChildren || hasProducts)
            {
                var dependencies = new List<string>();
                if (hasChildren) dependencies.Add("subcategorías");
                if (hasProducts) dependencies.Add("productos");

                return BadRequest(new { 
                    success = false, 
                    message = $"No se puede eliminar la categoría porque tiene {string.Join(" y ", dependencies)} asociados. Se desactivará en su lugar." 
                });
            }

            var success = await _categoryService.DeleteCategoryAsync(id);
            if (!success)
            {
                return NotFound(new { success = false, message = "Categoría no encontrada" });
            }

            return Ok(new { success = true, message = "Categoría eliminada exitosamente" });
        }

        /// <summary>
        /// Obtiene el árbol completo de categorías
        /// </summary>
        [HttpGet("tree")]
        [SwaggerOperation(Summary = "Obtener árbol de categorías", Description = "Obtiene la estructura jerárquica completa de categorías")]
        [SwaggerResponse(200, "Árbol de categorías obtenido exitosamente")]
        public async Task<ActionResult> GetCategoryTree([FromQuery] bool activeOnly = true)
        {
            var tree = await _categoryService.GetCategoryTreeAsync(activeOnly);
            return Ok(new { success = true, data = tree });
        }

        /// <summary>
        /// Obtiene categorías raíz (sin padre)
        /// </summary>
        [HttpGet("root")]
        [SwaggerOperation(Summary = "Obtener categorías raíz", Description = "Obtiene las categorías principales (sin padre)")]
        [SwaggerResponse(200, "Categorías raíz obtenidas exitosamente")]
        public async Task<ActionResult> GetRootCategories([FromQuery] bool activeOnly = true)
        {
            var categories = await _categoryService.GetRootCategoriesAsync(activeOnly);
            return Ok(new { success = true, data = categories });
        }

        /// <summary>
        /// Obtiene subcategorías de una categoría padre
        /// </summary>
        [HttpGet("{parentId}/subcategories")]
        [SwaggerOperation(Summary = "Obtener subcategorías", Description = "Obtiene las subcategorías de una categoría padre")]
        [SwaggerResponse(200, "Subcategorías obtenidas exitosamente")]
        [SwaggerResponse(404, "Categoría padre no encontrada")]
        public async Task<ActionResult> GetSubcategories(int parentId, [FromQuery] bool activeOnly = true)
        {
            var parent = await _categoryService.GetCategoryByIdAsync(parentId);
            if (parent == null)
            {
                return NotFound(new { success = false, message = "Categoría padre no encontrada" });
            }

            var subcategories = await _categoryService.GetSubcategoriesAsync(parentId, activeOnly);
            return Ok(new { success = true, data = subcategories });
        }

        /// <summary>
        /// Obtiene el breadcrumb/ruta de una categoría
        /// </summary>
        [HttpGet("{id}/breadcrumb")]
        [SwaggerOperation(Summary = "Obtener breadcrumb", Description = "Obtiene la ruta jerárquica de una categoría")]
        [SwaggerResponse(200, "Breadcrumb obtenido exitosamente")]
        [SwaggerResponse(404, "Categoría no encontrada")]
        public async Task<ActionResult> GetCategoryBreadcrumb(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound(new { success = false, message = "Categoría no encontrada" });
            }

            var breadcrumb = await _categoryService.GetCategoryBreadcrumbAsync(id);
            return Ok(new { success = true, data = breadcrumb });
        }

        /// <summary>
        /// Obtiene categorías activas para selectores
        /// </summary>
        [HttpGet("active")]
        [SwaggerOperation(Summary = "Obtener categorías activas", Description = "Obtiene una lista simple de categorías activas para selectores")]
        [SwaggerResponse(200, "Lista de categorías activas obtenida exitosamente")]
        public async Task<ActionResult> GetActiveCategories()
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();
            return Ok(new { success = true, data = categories });
        }

        /// <summary>
        /// Obtiene estadísticas de categorías
        /// </summary>
        [HttpGet("stats")]
        [RequirePermission("reportes.productos")]
        [SwaggerOperation(Summary = "Estadísticas de categorías", Description = "Obtiene estadísticas generales de las categorías")]
        [SwaggerResponse(200, "Estadísticas obtenidas exitosamente", typeof(CategoryStatsDto))]
        [SwaggerResponse(403, "No tiene permisos para ver estadísticas")]
        public async Task<ActionResult> GetCategoryStats()
        {
            var stats = await _categoryService.GetCategoryStatsAsync();
            return Ok(new { success = true, data = stats });
        }

        /// <summary>
        /// Mueve una categoría a otro padre
        /// </summary>
        [HttpPatch("{id}/move")]
        [RequirePermission("categorias.editar")]
        [SwaggerOperation(Summary = "Mover categoría", Description = "Mueve una categoría a otro padre y/o cambia su orden")]
        [SwaggerResponse(200, "Categoría movida exitosamente")]
        [SwaggerResponse(400, "Movimiento inválido")]
        [SwaggerResponse(404, "Categoría no encontrada")]
        [SwaggerResponse(403, "No tiene permisos para mover categorías")]
        public async Task<ActionResult> MoveCategory(int id, [FromBody] MoveCategoryDto dto)
        {
            if (dto.CategoryId != id)
            {
                return BadRequest(new { success = false, message = "El ID de la categoría no coincide" });
            }

            var success = await _categoryService.MoveCategoryAsync(id, dto.NuevoPadreId, dto.NuevoOrden);
            if (!success)
            {
                return BadRequest(new { success = false, message = "No se pudo mover la categoría. Verifique que no esté creando un ciclo." });
            }

            return Ok(new { success = true, message = "Categoría movida exitosamente" });
        }

        /// <summary>
        /// Activa o desactiva una categoría
        /// </summary>
        [HttpPatch("{id}/toggle-status")]
        [RequirePermission("categorias.editar")]
        [SwaggerOperation(Summary = "Alternar estado de categoría", Description = "Activa o desactiva una categoría")]
        [SwaggerResponse(200, "Estado de categoría actualizado exitosamente")]
        [SwaggerResponse(404, "Categoría no encontrada")]
        [SwaggerResponse(403, "No tiene permisos para cambiar el estado")]
        public async Task<ActionResult> ToggleCategoryStatus(int id)
        {
            var success = await _categoryService.ToggleCategoryStatusAsync(id);
            if (!success)
            {
                return NotFound(new { success = false, message = "Categoría no encontrada" });
            }

            return Ok(new { success = true, message = "Estado de categoría actualizado exitosamente" });
        }

        /// <summary>
        /// Cambio masivo de estado de categorías
        /// </summary>
        [HttpPatch("bulk-toggle-status")]
        [RequirePermission("categorias.editar")]
        [SwaggerOperation(Summary = "Cambio masivo de estado", Description = "Activa o desactiva múltiples categorías")]
        [SwaggerResponse(200, "Estados actualizados exitosamente")]
        [SwaggerResponse(403, "No tiene permisos para cambiar estados")]
        public async Task<ActionResult> BulkToggleStatus([FromBody] BulkToggleCategoryStatusDto dto)
        {
            var count = await _categoryService.BulkToggleStatusAsync(dto.CategoryIds, dto.Active);
            return Ok(new { 
                success = true, 
                message = $"Se actualizaron {count} categorías exitosamente",
                data = new { updatedCount = count }
            });
        }

        /// <summary>
        /// Eliminación masiva de categorías
        /// </summary>
        [HttpDelete("bulk")]
        [RequirePermission("categorias.eliminar")]
        [SwaggerOperation(Summary = "Eliminación masiva", Description = "Elimina múltiples categorías")]
        [SwaggerResponse(200, "Categorías procesadas exitosamente")]
        [SwaggerResponse(403, "No tiene permisos para eliminar categorías")]
        public async Task<ActionResult> BulkDelete([FromBody] BulkCategoryOperationDto dto)
        {
            var count = await _categoryService.BulkDeleteAsync(dto.CategoryIds);
            return Ok(new { 
                success = true, 
                message = $"Se procesaron {count} categorías exitosamente",
                data = new { processedCount = count }
            });
        }

        /// <summary>
        /// Reordenar categorías
        /// </summary>
        [HttpPatch("reorder")]
        [RequirePermission("categorias.editar")]
        [SwaggerOperation(Summary = "Reordenar categorías", Description = "Cambia el orden de múltiples categorías")]
        [SwaggerResponse(200, "Categorías reordenadas exitosamente")]
        [SwaggerResponse(403, "No tiene permisos para reordenar categorías")]
        public async Task<ActionResult> ReorderCategories([FromBody] List<int> categoryIds)
        {
            var count = await _categoryService.ReorderCategoriesAsync(categoryIds);
            return Ok(new { 
                success = true, 
                message = $"Se reordenaron {count} categorías exitosamente",
                data = new { reorderedCount = count }
            });
        }

        /// <summary>
        /// Obtiene categorías con conteo de productos
        /// </summary>
        [HttpGet("with-product-count")]
        [RequirePermission("categorias.ver")]
        [SwaggerOperation(Summary = "Categorías con conteo de productos", Description = "Obtiene categorías con el número de productos asociados")]
        [SwaggerResponse(200, "Lista obtenida exitosamente")]
        [SwaggerResponse(403, "No tiene permisos para ver esta información")]
        public async Task<ActionResult> GetCategoriesWithProductCount([FromQuery] bool includeSubcategories = true)
        {
            var categories = await _categoryService.GetCategoriesWithProductCountAsync(includeSubcategories);
            return Ok(new { success = true, data = categories });
        }
    }
}