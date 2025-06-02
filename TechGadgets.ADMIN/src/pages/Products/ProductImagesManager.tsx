import React, { useState, useEffect } from 'react';
import { ProductDto } from '../../types/products';

interface ProductImagesManagerProps {
  product: ProductDto;
  onImagesUpdated: () => void;
}

const ProductImagesManager: React.FC<ProductImagesManagerProps> = ({
  product,
  onImagesUpdated
}) => {
  const [imageAltText, setImageAltText] = useState('');
  const [imageOrder, setImageOrder] = useState<number>(1);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [uploadProgress, setUploadProgress] = useState(0);

  // Debug: Log del producto para verificar datos
  useEffect(() => {
    console.log('🔍 ProductImagesManager - Producto recibido:', {
      id: product?.Id,
      nombre: product?.Nombre,
      imagenes: product?.Imagenes,
      imagenesCount: product?.Imagenes?.length || 0
    });
  }, [product]);

  // Actualizar el orden sugerido cuando cambien las imágenes
  useEffect(() => {
    const nextOrder = (product?.Imagenes?.length || 0) + 1;
    setImageOrder(nextOrder);
  }, [product?.Imagenes]);

  const handleAddImage = async () => {
    if (!selectedFile) {
      setError('Debes seleccionar un archivo de imagen');
      return;
    }

    setLoading(true);
    setError(null);
    setUploadProgress(0);

    try {
      console.log('📸 Subiendo imagen para producto:', product.Id);

      const formData = new FormData();
      formData.append('imageFiles', selectedFile);
      formData.append('externalUrls', '');
      formData.append('AltText', imageAltText || `${product.Nombre} - Imagen ${imageOrder}`);
      formData.append('Orden', imageOrder.toString());

      // Simular progreso de carga
      const progressInterval = setInterval(() => {
        setUploadProgress(prev => Math.min(prev + 10, 90));
      }, 100);

      // Usar el endpoint correcto que maneja tanto la subida a Supabase como la BD
      const response = await fetch(`${import.meta.env.VITE_API_BASE_URL}/Products/${product.Id}/add-images`, {
        method: 'POST',
        headers: {
          Authorization: `Bearer ${localStorage.getItem('authToken')?.replace(/"/g, '')}`
        },
        body: formData
      });

      clearInterval(progressInterval);
      setUploadProgress(100);

      if (!response.ok) {
        let message = 'Error al subir la imagen';
        try {
          const errorData = await response.json();
          message = errorData.message || errorData.title || message;
          console.error('❌ Error del servidor:', errorData);
        } catch {
          console.error('❌ Error parseando respuesta del servidor');
        }
        throw new Error(message);
      }

      const result = await response.json();
      console.log('✅ Imagen subida exitosamente:', result);

      // Limpiar formulario
      setSelectedFile(null);
      setImageAltText('');
      setImageOrder((product.Imagenes?.length || 0) + 2);
      
      // Callback para recargar datos del producto
      onImagesUpdated();

      // Mostrar éxito brevemente
      setTimeout(() => setUploadProgress(0), 2000);
    } catch (err) {
      console.error('❌ Error subiendo imagen:', err);
      setError(err instanceof Error ? err.message : 'Error al subir imagen');
      setUploadProgress(0);
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteImage = async (imageId: number) => {
    if (!confirm('¿Estás seguro de eliminar esta imagen? Esta acción no se puede deshacer.')) return;

    setLoading(true);
    setError(null);

    try {
      console.log(`🗑️ Eliminando imagen ${imageId} del producto ${product.Id}`);

      const response = await fetch(`${import.meta.env.VITE_API_BASE_URL}/Products/${product.Id}/images/${imageId}`, {
        method: 'DELETE',
        headers: {
          Authorization: `Bearer ${localStorage.getItem('authToken')?.replace(/"/g, '')}`
        }
      });

      if (!response.ok) {
        let message = 'Error al eliminar la imagen';
        try {
          const errorData = await response.json();
          message = errorData.message || errorData.title || message;
          console.error('❌ Error del servidor:', errorData);
        } catch {
          console.error('❌ Error parseando respuesta del servidor');
        }
        throw new Error(message);
      }

      console.log('✅ Imagen eliminada exitosamente');
      onImagesUpdated();
    } catch (err) {
      console.error('❌ Error eliminando imagen:', err);
      setError(err instanceof Error ? err.message : 'No se pudo eliminar la imagen');
    } finally {
      setLoading(false);
    }
  };

  const handleSetMainImage = async (imageId: number) => {
    setLoading(true);
    setError(null);

    try {
      console.log(`🔄 Estableciendo imagen ${imageId} como principal`);

      // Usar el endpoint específico para establecer imagen principal
      const response = await fetch(`${import.meta.env.VITE_API_BASE_URL}/ProductosImagen/producto/${product.Id}/principal/${imageId}`, {
        method: 'PATCH',
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('authToken')?.replace(/"/g, '')}`,
          'Content-Type': 'application/json'
        }
      });

      if (!response.ok) {
        let message = 'Error al establecer imagen principal';
        try {
          const errorData = await response.json();
          message = errorData.message || errorData.title || message;
          console.error('❌ Error del servidor:', errorData);
        } catch {
          console.error('❌ Error parseando respuesta del servidor');
        }
        throw new Error(message);
      }

      console.log('✅ Imagen principal establecida exitosamente');
      onImagesUpdated();
    } catch (err) {
      console.error('❌ Error estableciendo imagen principal:', err);
      setError(err instanceof Error ? err.message : 'No se pudo establecer como principal');
    } finally {
      setLoading(false);
    }
  };

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      // Validar tipo de archivo
      if (!file.type.startsWith('image/')) {
        setError('Por favor selecciona un archivo de imagen válido');
        return;
      }

      // Validar tamaño (máximo 5MB)
      if (file.size > 5 * 1024 * 1024) {
        setError('El archivo debe ser menor a 5MB');
        return;
      }

      setSelectedFile(file);
      setError(null);
      
      // Sugerir alt text basado en el nombre del archivo si no hay uno
      if (!imageAltText) {
        const fileName = file.name.split('.')[0];
        const suggestedAlt = `${product.Nombre} - ${fileName}`;
        setImageAltText(suggestedAlt);
      }
    }
  };

  const formatFileSize = (bytes: number) => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  // Función para manejar errores de carga de imagen con más logging
  const handleImageError = (e: React.SyntheticEvent<HTMLImageElement, Event>) => {
    console.error('❌ Error cargando imagen:', e.currentTarget.src);
    const img = e.currentTarget;
    
    // Agregar información adicional sobre el error
    console.error('❌ Detalles del error de imagen:', {
      originalSrc: img.src,
      naturalWidth: img.naturalWidth,
      naturalHeight: img.naturalHeight,
      complete: img.complete
    });
    
    img.src = 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMjAwIiBoZWlnaHQ9IjIwMCIgdmlld0JveD0iMCAwIDIwMCAyMDAiIGZpbGw9Im5vbmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+CjxyZWN0IHdpZHRoPSIyMDAiIGhlaWdodD0iMjAwIiBmaWxsPSIjRjNGNEY2Ii8+CjxwYXRoIGQ9Ik04MCA4MEM4NC40MTgzIDgwIDg4IDc2LjQxODMgODggNzJDODggNjcuNTgxNyA4NC40MTgzIDY0IDgwIDY0Qzc1LjU4MTcgNjQgNzIgNjcuNTgxNyA3MiA3MkM3MiA3Ni40MTgzIDc1LjU4MTcgODAgODBaIiBmaWxsPSIjOUIwQkYxIi8+CjxwYXRoIGQ9Ik00MCA0MFY0MEg0MEw0OCAxMTJMNjQgMTI4TDgwIDExMkwxMTIgMTQ0SDE0NFYxNDRIMTQ0VjE2MEgxNDRWMTYwSDQwVjQwWiIgZmlsbD0iIzlCMEJGMSIvPgo8L3N2Zz4=';
  };

  // Función para manejar la carga exitosa de imagen
  const handleImageLoad = (imgSrc: string) => {
    console.log('✅ Imagen cargada exitosamente:', imgSrc);
  };

  // Validar que el producto y sus imágenes existan
  const images = product?.Imagenes || [];
  
  // Debug log para verificar las imágenes
  console.log('🖼️ Imágenes procesadas para renderizar:', images);

  return (
    <div className="bg-white rounded-lg shadow-lg overflow-hidden">
      {/* Header */}
      <div className="bg-gradient-to-r from-blue-600 to-blue-700 px-6 py-4">
        <div className="flex items-center justify-between">
          <div>
            <h2 className="text-xl font-bold text-white">Gestión de Imágenes</h2>
            <p className="text-blue-100 text-sm mt-1">{product?.Nombre || 'Producto sin nombre'}</p>
          </div>
          <div className="text-right">
            <div className="text-white text-sm font-medium">
              SKU: {product?.SKU || 'N/A'}
            </div>
            <div className="text-blue-100 text-xs">
              {images.length} imagen(es)
            </div>
          </div>
        </div>
      </div>

      <div className="p-6">
        {/* Error Alert */}
        {error && (
          <div className="mb-6 bg-red-50 border border-red-200 rounded-lg p-4 flex items-start space-x-3">
            <svg className="w-5 h-5 text-red-600 mt-0.5 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <div>
              <h3 className="text-red-800 font-medium">Error</h3>
              <p className="text-red-700 text-sm mt-1">{error}</p>
            </div>
          </div>
        )}

        {/* Add New Image Section */}
        <div className="bg-gray-50 rounded-lg p-6 mb-8">
          <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center">
            <svg className="w-5 h-5 text-blue-600 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
            </svg>
            Agregar nueva imagen
          </h3>

          <div className="space-y-4">
            {/* File Input */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Archivo de imagen
              </label>
              <div className="flex items-center space-x-4">
                <label className="flex-1 cursor-pointer">
                  <div className={`border-2 border-dashed rounded-lg p-4 text-center transition-colors ${
                    selectedFile 
                      ? 'border-green-300 bg-green-50' 
                      : 'border-gray-300 hover:border-gray-400 bg-white'
                  }`}>
                    <input
                      type="file"
                      accept="image/*"
                      onChange={handleFileSelect}
                      disabled={loading}
                      className="hidden"
                    />
                    {selectedFile ? (
                      <div className="text-green-700">
                        <svg className="w-8 h-8 mx-auto mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                        </svg>
                        <p className="text-sm font-medium">{selectedFile.name}</p>
                        <p className="text-xs text-green-600">{formatFileSize(selectedFile.size)}</p>
                      </div>
                    ) : (
                      <div className="text-gray-500">
                        <svg className="w-8 h-8 mx-auto mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
                        </svg>
                        <p className="text-sm">Haz clic para seleccionar una imagen</p>
                        <p className="text-xs">PNG, JPG, GIF hasta 5MB</p>
                      </div>
                    )}
                  </div>
                </label>
              </div>
            </div>

            {/* Alt Text and Order */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Texto alternativo
                </label>
                <input
                  type="text"
                  placeholder="Descripción de la imagen"
                  value={imageAltText}
                  onChange={(e) => setImageAltText(e.target.value)}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  disabled={loading}
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Orden
                </label>
                <input
                  type="number"
                  placeholder="Orden de visualización"
                  value={imageOrder}
                  onChange={(e) => setImageOrder(Number(e.target.value))}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  disabled={loading}
                  min={1}
                />
              </div>
            </div>

            {/* Progress Bar */}
            {uploadProgress > 0 && (
              <div className="w-full bg-gray-200 rounded-full h-2">
                <div 
                  className="bg-blue-600 h-2 rounded-full transition-all duration-300"
                  style={{ width: `${uploadProgress}%` }}
                ></div>
              </div>
            )}

            {/* Submit Button */}
            <button
              onClick={handleAddImage}
              disabled={loading || !selectedFile}
              className="w-full bg-blue-600 text-white px-4 py-3 rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors duration-200 font-medium"
            >
              {loading ? (
                <div className="flex items-center justify-center space-x-2">
                  <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                  <span>Subiendo imagen...</span>
                </div>
              ) : (
                'Agregar imagen'
              )}
            </button>
          </div>
        </div>

        {/* Existing Images */}
        <div>
          <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center">
            <svg className="w-5 h-5 text-gray-600 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 002 2z" />
            </svg>
            Imágenes del producto ({images.length})
          </h3>
          
         
          
          {/* Verificar si hay imágenes para mostrar */}
          {!images.length ? (
            <div className="text-center py-12 bg-gray-50 rounded-lg">
              <svg className="w-16 h-16 text-gray-400 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 002 2v12a2 2 0 002 2z" />
              </svg>
              <h3 className="text-lg font-medium text-gray-900 mb-2">Sin imágenes</h3>
              <p className="text-gray-600 mb-4">Este producto aún no tiene imágenes registradas</p>
              <p className="text-sm text-yellow-600 bg-yellow-50 inline-block px-3 py-1 rounded-full">
                ⚠️ Los productos necesitan al menos una imagen para estar completos
              </p>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {images
                .sort((a, b) => (a.Orden || 0) - (b.Orden || 0))
                .map((img) => {
                  // Log adicional para cada imagen que se va a renderizar
                  console.log('🎨 Renderizando imagen:', {
                    id: img.Id,
                    url: img.Url,
                    altText: img.AltText,
                    orden: img.Orden,
                    esPrincipal: img.EsPrincipal
                  });
                  
                  return (
                    <div key={img.Id} className="bg-white border border-gray-200 rounded-lg overflow-hidden shadow-sm hover:shadow-md transition-shadow duration-200">
                      {/* Image Container con más debugging */}
                      <div className="aspect-square bg-gray-100 relative">
                        <img
                          src={img.Url}
                          alt={img.AltText || 'Imagen del producto'}
                          className="w-full h-full object-cover"
                          onError={handleImageError}
                          onLoad={() => handleImageLoad(img.Url)}
                          crossOrigin="anonymous"
                        />
                        {img.EsPrincipal && (
                          <div className="absolute top-2 left-2">
                            <span className="bg-green-500 text-white text-xs font-bold px-2 py-1 rounded-full flex items-center space-x-1">
                              <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                              </svg>
                              <span>Principal</span>
                            </span>
                          </div>
                        )}
                        
                        {/* Overlay con información de debug (solo en desarrollo) */}
                        {process.env.NODE_ENV === 'development' && (
                          <div className="absolute bottom-2 left-2 bg-black bg-opacity-75 text-white text-xs p-1 rounded">
                            ID: {img.Id}
                          </div>
                        )}
                      </div>

                      {/* Content */}
                      <div className="p-4">
                        <div className="text-sm text-gray-700 font-medium mb-1 truncate">
                          {img.AltText || 'Sin descripción'}
                        </div>
                        <div className="text-xs text-gray-500 mb-3">
                          Orden: {img.Orden || 0}
                        </div>
                        <div className="text-xs text-gray-400 mb-3 truncate" title={img.Url}>
                          URL: {img.Url}
                        </div>

                        {/* Actions */}
                        <div className="flex flex-col space-y-2">
                          {!img.EsPrincipal && (
                            <button
                              onClick={() => handleSetMainImage(img.Id)}
                              disabled={loading}
                              className="w-full text-sm bg-blue-50 text-blue-700 px-3 py-2 rounded-md hover:bg-blue-100 disabled:opacity-50 transition-colors duration-200 font-medium"
                            >
                              Establecer como principal
                            </button>
                          )}
                          <button
                            onClick={() => handleDeleteImage(img.Id)}
                            disabled={loading}
                            className="w-full text-sm bg-red-50 text-red-700 px-3 py-2 rounded-md hover:bg-red-100 disabled:opacity-50 transition-colors duration-200 font-medium"
                          >
                            Eliminar imagen
                          </button>
                        </div>
                      </div>
                    </div>
                  );
                })}
            </div>
          )}
        </div>

        {/* Footer Info */}
        <div className="mt-8 bg-blue-50 rounded-lg p-4">
          <div className="flex items-start space-x-3">
            <svg className="w-5 h-5 text-blue-600 mt-0.5 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <div className="text-sm text-blue-800">
              <p className="font-medium mb-1">Tips para mejores resultados:</p>
              <ul className="text-blue-700 space-y-1">
                <li>• Usa imágenes de alta calidad con al menos 800x800 píxeles</li>
                <li>• La primera imagen se establece automáticamente como principal</li>
                <li>• Completa el texto alternativo para mejorar la accesibilidad</li>
                <li>• El orden determina cómo se muestran las imágenes en el catálogo</li>
                <li>• Las imágenes se almacenan en Supabase para mejor rendimiento</li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ProductImagesManager;