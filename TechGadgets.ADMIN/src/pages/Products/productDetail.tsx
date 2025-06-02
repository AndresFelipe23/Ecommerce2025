import { useEffect, useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import toast from 'react-hot-toast';
import { ProductDto } from '../../types/products';
import productsService from '../../services/productsService';

export default function ProductDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [product, setProduct] = useState<ProductDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [selectedImageIndex, setSelectedImageIndex] = useState(0);

  useEffect(() => {
    if (!id) return;

    const fetchProduct = async () => {
      try {
        const data = await productsService.getProductById(parseInt(id));
        setProduct(data);
        console.log('📦 Producto cargado:', data);
      } catch (error) {
        toast.error('Error al cargar el producto');
        console.error(error);
        navigate('/admin/products');
      } finally {
        setLoading(false);
      }
    };

    fetchProduct();
  }, [id, navigate]);

  const handleToggleStatus = async () => {
    if (!product) return;
    
    try {
      await productsService.toggleProductStatus(product.Id);
      setProduct({ ...product, Activo: !product.Activo });
      toast.success(`Producto ${product.Activo ? 'desactivado' : 'activado'} correctamente`);
    } catch (error) {
      toast.error('Error al cambiar el estado del producto');
      console.error(error);
    }
  };

  const formatDate = (dateString: string) => {
    if (!dateString) return 'No disponible';
    try {
      return new Date(dateString).toLocaleDateString('es-ES', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      });
    } catch {
      return dateString;
    }
  };

  const getStockStatus = (stock: number) => {
    if (stock === 0) return { color: 'bg-red-100 text-red-800', text: 'Sin stock' };
    if (stock <= 5) return { color: 'bg-yellow-100 text-yellow-800', text: 'Stock bajo' };
    if (stock <= 20) return { color: 'bg-blue-100 text-blue-800', text: 'Stock normal' };
    return { color: 'bg-green-100 text-green-800', text: 'Stock alto' };
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="bg-white rounded-lg shadow-lg p-8 max-w-md w-full mx-4">
          <div className="flex flex-col items-center space-y-4">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
            <div className="text-center">
              <h3 className="text-lg font-semibold text-gray-900 mb-2">
                Cargando producto
              </h3>
              <p className="text-gray-600">
                Obteniendo información detallada...
              </p>
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (!product) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="bg-white rounded-lg shadow-lg p-8 max-w-md w-full mx-4">
          <div className="text-center">
            <div className="w-16 h-16 bg-red-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <svg className="w-8 h-8 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
            <h3 className="text-lg font-semibold text-gray-900 mb-2">Producto no encontrado</h3>
            <p className="text-gray-600 mb-6">El producto solicitado no existe o no tienes permisos para verlo.</p>
            <Link
              to="/admin/products"
              className="bg-blue-600 text-white px-6 py-3 rounded-lg hover:bg-blue-700 transition-colors duration-200"
            >
              Volver a productos
            </Link>
          </div>
        </div>
      </div>
    );
  }

  // Usar solo las propiedades que sabemos que existen y funcionan
  const stockActual = product.StockActual || 0;
  const stockReservado = product.StockReservado || 0;
  const stockStatus = getStockStatus(stockActual);
  const images = product.Imagenes || [];

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header con navegación */}
      <div className="bg-white border-b border-gray-200 shadow-sm">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between h-16">
            <div className="flex items-center space-x-4">
              <button
                onClick={() => navigate(-1)}
                className="flex items-center space-x-2 text-gray-600 hover:text-gray-900 transition-colors duration-200"
              >
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 19l-7-7m0 0l7-7m-7 7h18" />
                </svg>
                <span className="font-medium">Volver</span>
              </button>
              <div className="h-6 w-px bg-gray-300"></div>
              <div>
                <h1 className="text-xl font-semibold text-gray-900">Detalle del Producto</h1>
                <p className="text-sm text-gray-600">ID: {product.Id}</p>
              </div>
            </div>
            
            <div className="flex items-center space-x-3">
              <button
                onClick={handleToggleStatus}
                className={`px-4 py-2 rounded-lg font-medium transition-colors duration-200 ${
                  product.Activo
                    ? 'bg-red-100 text-red-700 hover:bg-red-200'
                    : 'bg-green-100 text-green-700 hover:bg-green-200'
                }`}
              >
                {product.Activo ? 'Desactivar' : 'Activar'}
              </button>
              <Link
                to={`/admin/productos/${product.Id}/editar`}
                className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition-colors duration-200"
              >
                Editar Producto
              </Link>
            </div>
          </div>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Columna de imágenes */}
          <div className="lg:col-span-1">
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
              <div className="p-4">
                <h3 className="text-lg font-semibold text-gray-900 mb-4">
                  Imágenes ({images.length})
                </h3>
                
                {images.length > 0 ? (
                  <>
                    {/* Imagen principal */}
                    <div className="aspect-square bg-gray-100 rounded-lg overflow-hidden mb-4">
                      <img
                        src={images[selectedImageIndex]?.Url}
                        alt={images[selectedImageIndex]?.AltText || product.Nombre}
                        className="w-full h-full object-cover"
                        onError={(e) => {
                          (e.target as HTMLImageElement).src = 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNDAwIiBoZWlnaHQ9IjQwMCIgdmlld0JveD0iMCAwIDQwMCA0MDAiIGZpbGw9Im5vbmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+PHJlY3Qgd2lkdGg9IjQwMCIgaGVpZ2h0PSI0MDAiIGZpbGw9IiNGM0Y0RjYiLz48cGF0aCBkPSJNMTYwIDE2MEMxNjguODM3IDE2MCAxNzYgMTUyLjgzNyAxNzYgMTQ0QzE3NiAxMzUuMTYzIDE2OC44MzcgMTI4IDE2MCAxMjhDMTUxLjE2MyAxMjggMTQ0IDEzNS4xNjMgMTQ0IDE0NEMxNDQgMTUyLjgzNyAxNTEuMTYzIDE2MCAxNjBaIiBmaWxsPSIjOUIwQkYxIi8+PC9zdmc+';
                        }}
                      />
                    </div>
                    
                    {/* Miniaturas */}
                    {images.length > 1 && (
                      <div className="grid grid-cols-4 gap-2">
                        {images.map((img, index) => (
                          <button
                            key={img.Id}
                            onClick={() => setSelectedImageIndex(index)}
                            className={`aspect-square rounded-lg overflow-hidden border-2 transition-all duration-200 relative ${
                              selectedImageIndex === index
                                ? 'border-blue-500 ring-2 ring-blue-200'
                                : 'border-gray-200 hover:border-gray-300'
                            }`}
                          >
                            <img
                              src={img.Url}
                              alt={img.AltText || `Imagen ${index + 1}`}
                              className="w-full h-full object-cover"
                            />
                            {img.EsPrincipal && (
                              <div className="absolute top-1 left-1">
                                <span className="bg-green-500 text-white text-xs px-1 py-0.5 rounded">
                                  Principal
                                </span>
                              </div>
                            )}
                          </button>
                        ))}
                      </div>
                    )}
                  </>
                ) : (
                  <div className="aspect-square bg-gray-100 rounded-lg flex items-center justify-center border border-gray-200">
                    <div className="text-center">
                      <svg className="w-16 h-16 text-gray-400 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                      </svg>
                      <p className="text-gray-500 text-sm">Sin imágenes</p>
                    </div>
                  </div>
                )}
                
                <div className="mt-4">
                  <Link
                    to={`/products/${product.Id}/imagenes`}
                    className="w-full bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition-colors duration-200 flex items-center justify-center space-x-2"
                  >
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                    </svg>
                    <span>Gestionar Imágenes</span>
                  </Link>
                </div>
              </div>
            </div>
          </div>

          {/* Información principal */}
          <div className="lg:col-span-2 space-y-6">
            {/* Header del producto */}
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
              <div className="flex items-start justify-between mb-4">
                <div>
                  <h2 className="text-3xl font-bold text-gray-900 mb-2">{product.Nombre}</h2>
                  <div className="flex items-center space-x-4 text-sm text-gray-600">
                    <span className="font-medium">SKU: {product.SKU}</span>
                    {product.Slug && (
                      <>
                        <span>•</span>
                        <span>Slug: {product.Slug}</span>
                      </>
                    )}
                  </div>
                </div>
                <div className="flex flex-col items-end space-y-2">
                  <span className={`inline-flex items-center px-3 py-1 rounded-full text-sm font-medium ${
                    product.Activo
                      ? 'bg-green-100 text-green-800'
                      : 'bg-red-100 text-red-800'
                  }`}>
                    {product.Activo ? 'Activo' : 'Inactivo'}
                  </span>
                  {product.Destacado && (
                    <span className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-blue-100 text-blue-800">
                      Destacado
                    </span>
                  )}
                  {product.Nuevo && (
                    <span className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-green-100 text-green-800">
                      Nuevo
                    </span>
                  )}
                  {product.EnOferta && (
                    <span className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-orange-100 text-orange-800">
                      En Oferta
                    </span>
                  )}
                </div>
              </div>
              
              {/* Descripción corta */}
              {product.DescripcionCorta && (
                <p className="text-gray-700 text-lg leading-relaxed">{product.DescripcionCorta}</p>
              )}
            </div>

            {/* Precios e inventario */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {/* Precios */}
              <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center">
                  <svg className="w-5 h-5 text-green-600 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1" />
                  </svg>
                  Precios
                </h3>
                <div className="space-y-3">
                  <div className="flex justify-between items-center">
                    <span className="text-gray-600">Precio de venta:</span>
                    <span className="text-2xl font-bold text-green-600">${product.Precio.toFixed(2)}</span>
                  </div>
                  {product.PrecioComparacion && (
                    <div className="flex justify-between items-center">
                      <span className="text-gray-600">Precio de comparación:</span>
                      <span className="text-lg text-gray-500 line-through">${product.PrecioComparacion.toFixed(2)}</span>
                    </div>
                  )}
                  {product.Costo && (
                    <div className="flex justify-between items-center">
                      <span className="text-gray-600">Costo:</span>
                      <span className="text-lg text-gray-700">${product.Costo.toFixed(2)}</span>
                    </div>
                  )}
                  {product.Costo && product.Precio && (
                    <div className="flex justify-between items-center pt-2 border-t border-gray-200">
                      <span className="text-gray-600 font-medium">Margen:</span>
                      <span className="text-lg font-semibold text-blue-600">
                        {(((product.Precio - product.Costo) / product.Precio) * 100).toFixed(1)}%
                      </span>
                    </div>
                  )}
                </div>
              </div>

              {/* Inventario */}
              <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center">
                  <svg className="w-5 h-5 text-blue-600 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 7l-8-4-8 4m16 0l-8 4-8-4m16 0v10l-8 4-8-4V7" />
                  </svg>
                  Inventario
                </h3>
                <div className="space-y-3">
                  <div className="flex justify-between items-center">
                    <span className="text-gray-600">Stock actual:</span>
                    <div className="flex items-center space-x-2">
                      <span className="text-2xl font-bold text-gray-900">{stockActual}</span>
                      <span className={`px-2 py-1 rounded-full text-xs font-medium ${stockStatus.color}`}>
                        {stockStatus.text}
                      </span>
                    </div>
                  </div>
                  {stockReservado > 0 && (
                    <div className="flex justify-between items-center">
                      <span className="text-gray-600">Stock reservado:</span>
                      <span className="text-lg text-orange-600">{stockReservado}</span>
                    </div>
                  )}
                  <div className="flex justify-between items-center">
                    <span className="text-gray-600">Stock disponible:</span>
                    <span className="text-lg font-semibold text-green-600">
                      {stockActual - stockReservado}
                    </span>
                  </div>
                </div>
              </div>
            </div>

            {/* Información básica */}
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center">
                <svg className="w-5 h-5 text-gray-600 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                Información General
              </h3>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="space-y-3">
                  <div>
                    <label className="text-sm font-medium text-gray-600">Categoría</label>
                    <p className="text-gray-900">{product.CategoriaNombre || `ID: ${product.CategoriaId}`}</p>
                  </div>
                  <div>
                    <label className="text-sm font-medium text-gray-600">Marca</label>
                    <p className="text-gray-900">{product.MarcaNombre || `ID: ${product.MarcaId}`}</p>
                  </div>
                  {product.Tipo && (
                    <div>
                      <label className="text-sm font-medium text-gray-600">Tipo</label>
                      <p className="text-gray-900">{product.Tipo}</p>
                    </div>
                  )}
                  {product.Estado && (
                    <div>
                      <label className="text-sm font-medium text-gray-600">Estado</label>
                      <p className="text-gray-900">{product.Estado}</p>
                    </div>
                  )}
                </div>
                <div className="space-y-3">
                  {product.Peso && (
                    <div>
                      <label className="text-sm font-medium text-gray-600">Peso</label>
                      <p className="text-gray-900">{product.Peso} kg</p>
                    </div>
                  )}
                  {product.Dimensiones && (
                    <div>
                      <label className="text-sm font-medium text-gray-600">Dimensiones</label>
                      <p className="text-gray-900">{product.Dimensiones}</p>
                    </div>
                  )}
                  <div>
                    <label className="text-sm font-medium text-gray-600">Requiere envío</label>
                    <p className="text-gray-900">{product.RequiereEnvio ? 'Sí' : 'No'}</p>
                  </div>
                  <div>
                    <label className="text-sm font-medium text-gray-600">Permite reseñas</label>
                    <p className="text-gray-900">{product.PermiteReseñas ? 'Sí' : 'No'}</p>
                  </div>
                </div>
              </div>
            </div>

            {/* Descripción larga */}
            {product.DescripcionLarga && (
              <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center">
                  <svg className="w-5 h-5 text-gray-600 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                  </svg>
                  Descripción Detallada
                </h3>
                <div className="prose prose-gray max-w-none">
                  <p className="text-gray-700 leading-relaxed whitespace-pre-wrap">{product.DescripcionLarga}</p>
                </div>
              </div>
            )}

            {/* SEO y Metadatos - Solo mostrar si existen */}
            {(product.MetaTitulo || product.MetaDescripcion || product.PalabrasClaves) && (
              <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center">
                  <svg className="w-5 h-5 text-gray-600 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                  </svg>
                  SEO y Metadatos
                </h3>
                <div className="space-y-4">
                  {product.MetaTitulo && (
                    <div>
                      <label className="text-sm font-medium text-gray-600">Meta Título</label>
                      <p className="text-gray-900 mt-1">{product.MetaTitulo}</p>
                    </div>
                  )}
                  {product.MetaDescripcion && (
                    <div>
                      <label className="text-sm font-medium text-gray-600">Meta Descripción</label>
                      <p className="text-gray-900 mt-1">{product.MetaDescripcion}</p>
                    </div>
                  )}
                  {product.PalabrasClaves && (
                    <div>
                      <label className="text-sm font-medium text-gray-600">Palabras Clave</label>
                      <div className="mt-2 flex flex-wrap gap-2">
                        {product.PalabrasClaves.split(',').map((keyword, index) => (
                          <span
                            key={index}
                            className="inline-flex items-center px-3 py-1 rounded-full text-sm bg-blue-100 text-blue-800"
                          >
                            {keyword.trim()}
                          </span>
                        ))}
                      </div>
                    </div>
                  )}
                </div>
              </div>
            )}

            {/* Información del sistema */}
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center">
                <svg className="w-5 h-5 text-gray-600 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                Información del Sistema
              </h3>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="space-y-3">
                  <div>
                    <label className="text-sm font-medium text-gray-600">Fecha de creación</label>
                    <p className="text-gray-900">{formatDate(product.FechaCreacion)}</p>
                  </div>
                  {product.FechaModificacion && (
                    <div>
                      <label className="text-sm font-medium text-gray-600">Última modificación</label>
                      <p className="text-gray-900">{formatDate(product.FechaModificacion)}</p>
                    </div>
                  )}
                  <div>
                    <label className="text-sm font-medium text-gray-600">Orden de visualización</label>
                    <p className="text-gray-900">{product.Orden || 0}</p>
                  </div>
                </div>
                <div className="space-y-3">
                  {product.Garantia && (
                    <div>
                      <label className="text-sm font-medium text-gray-600">Garantía</label>
                      <p className="text-gray-900">{product.Garantia}</p>
                    </div>
                  )}
                  <div>
                    <label className="text-sm font-medium text-gray-600">Total de imágenes</label>
                    <p className="text-gray-900">{images.length}</p>
                  </div>
                  {product.ImagenPrincipal && (
                    <div>
                      <label className="text-sm font-medium text-gray-600">URL imagen principal</label>
                      <p className="text-gray-900 text-sm truncate" title={product.ImagenPrincipal}>
                        {product.ImagenPrincipal}
                      </p>
                    </div>
                  )}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}