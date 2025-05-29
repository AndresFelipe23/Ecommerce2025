using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechGadgets.API.Dtos.Products;

namespace TechGadgets.API.Services.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de gestión de imágenes de productos
    /// </summary>
    public interface IProductosImagenService
    {
        #region Consultas Básicas

        /// <summary>
        /// Obtiene todas las imágenes de un producto específico
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <returns>Lista de imágenes del producto</returns>
        Task<IEnumerable<ProductoImagenDto>> GetImagenesByProductoIdAsync(int productoId);

        /// <summary>
        /// Obtiene todas las imágenes de una variante específica
        /// </summary>
        /// <param name="varianteId">ID de la variante</param>
        /// <returns>Lista de imágenes de la variante</returns>
        Task<IEnumerable<ProductoImagenDto>> GetImagenesByVarianteIdAsync(int varianteId);

        /// <summary>
        /// Obtiene una imagen específica por su ID
        /// </summary>
        /// <param name="id">ID de la imagen</param>
        /// <returns>Datos de la imagen</returns>
        Task<ProductoImagenDto?> GetImagenByIdAsync(int id);

        /// <summary>
        /// Obtiene la imagen principal de un producto
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <returns>Imagen principal del producto</returns>
        Task<ProductoImagenDto?> GetImagenPrincipalByProductoIdAsync(int productoId);

        #endregion

        #region Operaciones CRUD

        /// <summary>
        /// Crea una nueva imagen de producto
        /// </summary>
        /// <param name="createDto">Datos para crear la imagen</param>
        /// <returns>Imagen creada</returns>
        Task<ProductoImagenDto> CreateImagenAsync(CreateProductoImagenDto createDto);

        /// <summary>
        /// Actualiza una imagen existente
        /// </summary>
        /// <param name="id">ID de la imagen</param>
        /// <param name="updateDto">Datos para actualizar</param>
        /// <returns>Imagen actualizada</returns>
        Task<ProductoImagenDto?> UpdateImagenAsync(int id, UpdateProductoImagenDto updateDto);

        /// <summary>
        /// Elimina una imagen
        /// </summary>
        /// <param name="id">ID de la imagen</param>
        /// <returns>True si se eliminó correctamente</returns>
        Task<bool> DeleteImagenAsync(int id);

        #endregion

        #region Operaciones Masivas y Especiales

        /// <summary>
        /// Crea múltiples imágenes para un producto
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="imagenes">Lista de imágenes a crear</param>
        /// <returns>Lista de imágenes creadas</returns>
        Task<IEnumerable<ProductoImagenDto>> CreateMultipleImagenesAsync(int productoId, IEnumerable<CreateProductoImagenDto> imagenes);

        /// <summary>
        /// Actualiza múltiples imágenes de un producto (crear, actualizar, eliminar)
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="imagenes">Lista de imágenes a procesar</param>
        /// <returns>Lista de imágenes resultantes</returns>
        Task<IEnumerable<ProductoImagenDto>> UpdateMultipleImagenesAsync(int productoId, IEnumerable<UpdateProductoImagenDto> imagenes);

        /// <summary>
        /// Actualiza el orden de las imágenes de un producto
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="ordenImagenes">Lista con ID y nuevo orden</param>
        /// <returns>True si se actualizó correctamente</returns>
        Task<bool> UpdateOrdenImagenesAsync(int productoId, IEnumerable<UpdateOrdenImagenDto> ordenImagenes);

        /// <summary>
        /// Establece una imagen como principal para un producto
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="imagenId">ID de la imagen a establecer como principal</param>
        /// <returns>True si se estableció correctamente</returns>
        Task<bool> SetImagenPrincipalAsync(int productoId, int imagenId);

        /// <summary>
        /// Cambia el estado activo/inactivo de una imagen
        /// </summary>
        /// <param name="id">ID de la imagen</param>
        /// <returns>True si se cambió correctamente</returns>
        Task<bool> ToggleImagenStatusAsync(int id);

        /// <summary>
        /// Elimina todas las imágenes de un producto
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <returns>Número de imágenes eliminadas</returns>
        Task<int> DeleteAllImagenesByProductoIdAsync(int productoId);

        #endregion

        #region Validaciones y Utilidades

        /// <summary>
        /// Verifica si existe una imagen con la URL especificada
        /// </summary>
        /// <param name="url">URL de la imagen</param>
        /// <param name="excludeId">ID a excluir de la búsqueda</param>
        /// <returns>True si existe</returns>
        Task<bool> ExistsImagenByUrlAsync(string url, int? excludeId = null);

        /// <summary>
        /// Verifica si un producto tiene imágenes
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <returns>True si tiene imágenes</returns>
        Task<bool> ProductoHasImagenesAsync(int productoId);

        /// <summary>
        /// Obtiene estadísticas de imágenes por producto
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <returns>Estadísticas de imágenes</returns>
        Task<ProductoImagenStatsDto> GetImagenStatsAsync(int productoId);

        #endregion
    }
}