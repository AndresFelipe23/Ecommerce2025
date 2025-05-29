using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TechGadgets.API.Attributes;
using TechGadgets.API.Dtos.Brands;
using TechGadgets.API.Services.Interfaces;

namespace TechGadgets.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [SwaggerTag("Gestión de marcas de productos")]
    public class BrandsController : ControllerBase
    {
        private readonly IBrandService _brandService;

        public BrandsController(IBrandService brandService)
        {
            _brandService = brandService;
        }

        /// <summary>
        /// Obtiene todas las marcas con filtros y paginación
        /// </summary>
        [HttpGet]
        [SwaggerOperation(Summary = "Listar marcas", Description = "Obtiene todas las marcas con filtros y paginación")]
        [SwaggerResponse(200, "Lista de marcas obtenida exitosamente")]
        public async Task<ActionResult> GetBrands([FromQuery] BrandFilterDto filter)
        {
            var brands = await _brandService.GetBrandsAsync(filter);
            return Ok(new { success = true, data = brands });
        }

        /// <summary>
        /// Obtiene una marca específica por ID
        /// </summary>
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Obtener marca por ID", Description = "Obtiene los detalles de una marca específica")]
        [SwaggerResponse(200, "Marca encontrada", typeof(BrandDto))]
        [SwaggerResponse(404, "Marca no encontrada")]
        public async Task<ActionResult<BrandDto>> GetBrand(int id)
        {
            var brand = await _brandService.GetBrandByIdAsync(id);
            if (brand == null)
            {
                return NotFound(new { success = false, message = "Marca no encontrada" });
            }

            return Ok(new { success = true, data = brand });
        }

        /// <summary>
        /// Crea una nueva marca
        /// </summary>
        [HttpPost]
        [RequirePermission("marcas.crear")]
        [SwaggerOperation(Summary = "Crear nueva marca", Description = "Crea una nueva marca en el sistema")]
        [SwaggerResponse(201, "Marca creada exitosamente", typeof(BrandDto))]
        [SwaggerResponse(400, "Datos inválidos o marca ya existe")]
        [SwaggerResponse(403, "No tiene permisos para crear marcas")]
        public async Task<ActionResult<BrandDto>> CreateBrand([FromBody] CreateBrandDto dto)
        {
            // Verificar si la marca ya existe
            if (await _brandService.BrandExistsAsync(dto.Nombre))
            {
                return BadRequest(new { success = false, message = "Ya existe una marca con ese nombre" });
            }

            var brand = await _brandService.CreateBrandAsync(dto);
            return CreatedAtAction(nameof(GetBrand), new { id = brand.Id },
                new { success = true, message = "Marca creada exitosamente", data = brand });
        }

        /// <summary>
        /// Actualiza una marca existente
        /// </summary>
        [HttpPut("{id}")]
        [RequirePermission("marcas.editar")]
        [SwaggerOperation(Summary = "Actualizar marca", Description = "Actualiza una marca existente")]
        [SwaggerResponse(200, "Marca actualizada exitosamente", typeof(BrandDto))]
        [SwaggerResponse(404, "Marca no encontrada")]
        [SwaggerResponse(400, "Datos inválidos")]
        [SwaggerResponse(403, "No tiene permisos para editar marcas")]
        public async Task<ActionResult<BrandDto>> UpdateBrand(int id, [FromBody] UpdateBrandDto dto)
        {
            // Verificar si el nombre ya existe en otra marca
            if (await _brandService.BrandExistsAsync(dto.Nombre, id))
            {
                return BadRequest(new { success = false, message = "Ya existe otra marca con ese nombre" });
            }

            var brand = await _brandService.UpdateBrandAsync(id, dto);
            if (brand == null)
            {
                return NotFound(new { success = false, message = "Marca no encontrada" });
            }

            return Ok(new { success = true, message = "Marca actualizada exitosamente", data = brand });
        }

        /// <summary>
        /// Elimina una marca
        /// </summary>
        [HttpDelete("{id}")]
        [RequirePermission("marcas.eliminar")]
        [SwaggerOperation(Summary = "Eliminar marca", Description = "Elimina una marca del sistema")]
        [SwaggerResponse(200, "Marca eliminada exitosamente")]
        [SwaggerResponse(404, "Marca no encontrada")]
        [SwaggerResponse(400, "No se puede eliminar la marca porque tiene productos asociados")]
        [SwaggerResponse(403, "No tiene permisos para eliminar marcas")]
        public async Task<ActionResult> DeleteBrand(int id)
        {
            var hasProducts = await _brandService.BrandHasProductsAsync(id);
            if (hasProducts)
            {
                return BadRequest(new { 
                    success = false, 
                    message = "No se puede eliminar la marca porque tiene productos asociados. Se desactivará en su lugar." 
                });
            }

            var success = await _brandService.DeleteBrandAsync(id);
            if (!success)
            {
                return NotFound(new { success = false, message = "Marca no encontrada" });
            }

            return Ok(new { success = true, message = "Marca eliminada exitosamente" });
        }

        /// <summary>
        /// Obtiene marcas activas para selectores
        /// </summary>
        [HttpGet("active")]
        [SwaggerOperation(Summary = "Obtener marcas activas", Description = "Obtiene una lista simple de marcas activas para selectores")]
        [SwaggerResponse(200, "Lista de marcas activas obtenida exitosamente")]
        public async Task<ActionResult> GetActiveBrands()
        {
            var brands = await _brandService.GetActiveBrandsAsync();
            return Ok(new { success = true, data = brands });
        }

        /// <summary>
        /// Obtiene estadísticas de marcas
        /// </summary>
        [HttpGet("stats")]
        [RequirePermission("reportes.productos")]
        [SwaggerOperation(Summary = "Estadísticas de marcas", Description = "Obtiene estadísticas generales de las marcas")]
        [SwaggerResponse(200, "Estadísticas obtenidas exitosamente", typeof(BrandStatsDto))]
        [SwaggerResponse(403, "No tiene permisos para ver estadísticas")]
        public async Task<ActionResult> GetBrandStats()
        {
            var stats = await _brandService.GetBrandStatsAsync();
            return Ok(new { success = true, data = stats });
        }

        /// <summary>
        /// Activa o desactiva una marca
        /// </summary>
        [HttpPatch("{id}/toggle-status")]
        [RequirePermission("marcas.editar")]
        [SwaggerOperation(Summary = "Alternar estado de marca", Description = "Activa o desactiva una marca")]
        [SwaggerResponse(200, "Estado de marca actualizado exitosamente")]
        [SwaggerResponse(404, "Marca no encontrada")]
        [SwaggerResponse(403, "No tiene permisos para cambiar el estado")]
        public async Task<ActionResult> ToggleBrandStatus(int id)
        {
            var success = await _brandService.ToggleBrandStatusAsync(id);
            if (!success)
            {
                return NotFound(new { success = false, message = "Marca no encontrada" });
            }

            return Ok(new { success = true, message = "Estado de marca actualizado exitosamente" });
        }

        /// <summary>
        /// Cambio masivo de estado de marcas
        /// </summary>
        [HttpPatch("bulk-toggle-status")]
        [RequirePermission("productos.editar")]
        [SwaggerOperation(Summary = "Cambio masivo de estado", Description = "Activa o desactiva múltiples marcas")]
        [SwaggerResponse(200, "Estados actualizados exitosamente")]
        [SwaggerResponse(403, "No tiene permisos para cambiar estados")]
        public async Task<ActionResult> BulkToggleStatus([FromBody] BulkToggleBrandStatusDto dto)
        {
            var count = await _brandService.BulkToggleStatusAsync(dto.BrandIds, dto.Active);
            return Ok(new { 
                success = true, 
                message = $"Se actualizaron {count} marcas exitosamente",
                data = new { updatedCount = count }
            });
        }

        /// <summary>
        /// Obtiene marcas con conteo de productos
        /// </summary>
        [HttpGet("with-product-count")]
        [RequirePermission("marcas.ver")]
        [SwaggerOperation(Summary = "Marcas con conteo de productos", Description = "Obtiene marcas con el número de productos asociados")]
        [SwaggerResponse(200, "Lista obtenida exitosamente")]
        [SwaggerResponse(403, "No tiene permisos para ver esta información")]
        public async Task<ActionResult> GetBrandsWithProductCount()
        {
            var brands = await _brandService.GetBrandsWithProductCountAsync();
            return Ok(new { success = true, data = brands });
        }
    }
}