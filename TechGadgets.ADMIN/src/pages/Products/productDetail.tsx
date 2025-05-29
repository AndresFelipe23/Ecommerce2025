import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { ProductDto } from '../../types/products';
import { usePermissions } from '../../hooks/usePermissions';
import PermissionGate from '../../components/auth/PermissionGate';
import { PERMISSIONS } from '../../types/permissions';
import { AxiosError } from 'axios';
import { 
  FiArrowLeft, 
  FiEdit, 
  FiTrash2, 
  FiToggleLeft, 
  FiToggleRight,
  FiPackage,
  FiTag,
  FiShoppingCart,
  FiTruck,
  FiStar,
  FiHeart,
  FiShare2,
  FiEye,
  FiAlertTriangle,
  FiCheckCircle,
  FiXCircle,
  FiInfo,
  FiImage,
  FiZoomIn,
  FiChevronLeft,
  FiChevronRight
} from 'react-icons/fi';
import productsService, { ApiResponse } from '../../services/productsService';

const ProductDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [product, setProduct] = useState<ProductDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [selectedImageIndex, setSelectedImageIndex] = useState(0);
  const [showImageModal, setShowImageModal] = useState(false);
  const [quantity, setQuantity] = useState(1);

  usePermissions();

  // ✅ LOAD PRODUCT MEJORADO CON DEBUG COMPLETO
  useEffect(() => {
    const loadProduct = async () => {
      if (!id) {
        console.error('❌ ProductDetail - ID no válido:', id);
        setError('ID de producto no válido');
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        setError(null);
        
        // ✅ DEBUG COMPLETO
        console.log('🔍 === ProductDetail - INICIANDO CARGA ===');
        console.log('📋 ID recibido:', id);
        console.log('📋 ID parseado:', parseInt(id));
        
        const productData = await productsService.getProduct(parseInt(id));
        
        // ✅ VERIFICAR DATA RECIBIDA CON DETALLE
        console.log('📦 === ProductDetail - PRODUCTO RECIBIDO ===');
        console.log('✅ Producto completo:', productData);
        console.log('✅ Análisis del producto:', {
          id: productData?.Id,
          nombre: productData?.Nombre,
          sku: productData?.SKU,
          precio: productData?.Precio,
          activo: productData?.Activo,
          categoria: productData?.CategoriaNombre,
          marca: productData?.MarcaNombre,
          stock: productData?.StockActual,
          hasData: !!productData,
          isValidProduct: !!(productData && productData.Id > 0 && productData.Nombre && productData.Nombre !== 'Sin nombre')
        });
        
        if (!productData) {
          console.error('❌ ProductDetail - Producto no encontrado (null/undefined)');
          setError('Producto no encontrado');
          return;
        }
        
        if (productData.Id === 0 || !productData.Nombre || productData.Nombre === 'Sin nombre') {
          console.error('❌ ProductDetail - Producto con datos inválidos:', {
            id: productData.Id,
            nombre: productData.Nombre,
            hasValidId: productData.Id > 0,
            hasValidName: productData.Nombre && productData.Nombre !== 'Sin nombre'
          });
          setError('Los datos del producto no son válidos');
          return;
        }
        
        console.log('✅ ProductDetail - Producto válido, estableciendo estado...');
        setProduct(productData);
        
      } catch (error) {
        const axiosError = error as AxiosError<ApiResponse<ProductDto>>;
        console.error('❌ === ProductDetail - ERROR EN CARGA ===');
        console.error('📋 Error details:', {
          status: axiosError.response?.status,
          message: axiosError.response?.data?.message,
          data: axiosError.response?.data,
          errorMessage: axiosError.message
        });
        
        let errorMessage = 'Error al cargar el producto';
        
        if (axiosError.response?.status === 404) {
          errorMessage = 'Producto no encontrado';
        } else if (axiosError.response?.data?.message) {
          errorMessage = axiosError.response.data.message;
        } else if (axiosError.message) {
          errorMessage = axiosError.message;
        }
        
        setError(errorMessage);
      } finally {
        setLoading(false);
        console.log('🔚 ProductDetail - Carga finalizada');
      }
    };

    loadProduct();
  }, [id]);

  // ✅ DEBUG DEL ESTADO CUANDO CAMBIA
  useEffect(() => {
    console.log('🔧 === ProductDetail - ESTADO ACTUALIZADO ===');
    console.log('📋 Loading:', loading);
    console.log('📋 Error:', error);
    console.log('📋 Product:', product ? {
      id: product.Id,
      nombre: product.Nombre,
      isValid: product.Id > 0 && product.Nombre !== 'Sin nombre'
    } : 'null');
  }, [loading, error, product]);

  // Toggle product status
  const handleToggleStatus = async () => {
    if (!product) return;
  
    const confirmMessage = product.Activo 
      ? '¿Está seguro de que desea desactivar este producto?' 
      : '¿Está seguro de que desea activar este producto?';
    
    if (!window.confirm(confirmMessage)) return;
  
    try {
      setActionLoading(true);
      
      // ✅ DEBUG ANTES DE TOGGLE
      console.log('🔄 Iniciando toggle para producto:', {
        id: product.Id,
        estadoActual: product.Activo,
        token: localStorage.getItem("authToken") ? 'Presente' : 'Ausente'
      });
      
      await productsService.toggleProductStatus(product.Id);
      
      // ✅ ACTUALIZAR ESTADO LOCAL
      setProduct({ ...product, Activo: !product.Activo });
      
      // ✅ MOSTRAR MENSAJE DE ÉXITO
      alert(`Producto ${!product.Activo ? 'activado' : 'desactivado'} exitosamente`);
      
    } catch (error) {
      const axiosError = error as AxiosError<ApiResponse<unknown>>;
      console.error('❌ Error en handleToggleStatus:', {
        status: axiosError.response?.status,
        message: axiosError.response?.data?.message,
        error: axiosError.message
      });
      
      let errorMessage = 'Error al cambiar el estado del producto';
      
      if (axiosError.response?.status === 403) {
        errorMessage = 'No tienes permisos para realizar esta acción. Contacta al administrador.';
      } else if (axiosError.response?.status === 401) {
        errorMessage = 'Tu sesión ha expirado. Por favor, inicia sesión nuevamente.';
        // Opcional: redirigir al login
        // navigate('/login');
      } else if (axiosError.response?.data?.message) {
        errorMessage = axiosError.response.data.message;
      }
      
      alert(errorMessage);
    } finally {
      setActionLoading(false);
    }
  };

  // Delete product
  const handleDelete = async () => {
    if (!product) return;

    const confirmMessage = `¿Está seguro de que desea eliminar el producto "${product.Nombre}"?\n\n` +
      `${(product.StockActual ?? 0) > 0
        ? `Este producto tiene ${product.StockActual} unidades en stock y será desactivado en lugar de eliminado.`
        : 'Esta acción no se puede deshacer.'
      }`;
    
    if (!window.confirm(confirmMessage)) return;

    try {
      setActionLoading(true);
      await productsService.deleteProduct(product.Id);
      navigate('/products', { 
        state: { message: 'Producto eliminado exitosamente' }
      });
    } catch (error) {
      const axiosError = error as AxiosError<ApiResponse<unknown>>;
      console.error('Error deleting product:', axiosError);
      alert(axiosError.response?.data?.message || 'Error al eliminar el producto');
    } finally {
      setActionLoading(false);
    }
  };

  // Format price
  const formatPrice = (price: number | null | undefined) => {
    if (price === null || price === undefined || isNaN(price)) {
      return 'Sin precio';
    }
    
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP',
      minimumFractionDigits: 0
    }).format(price);
  };

  // Format date
  const formatDate = (dateString: string) => {
    if (!dateString) return 'No disponible';
    return new Date(dateString).toLocaleDateString('es-ES', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  // Get stock status
  const getStockStatus = (stock: number | undefined, stockReservado: number | undefined = 0) => {
    const currentStock = stock ?? 0;
    const reservedStock = stockReservado ?? 0;
    const availableStock = currentStock - reservedStock;
    
    if (availableStock <= 0) return { 
      status: 'sin-stock', 
      label: 'Sin Stock', 
      color: 'text-red-600 bg-red-100',
      icon: <FiXCircle className="w-4 h-4" />
    };
    if (availableStock <= 5) return { 
      status: 'bajo-stock', 
      label: 'Bajo Stock', 
      color: 'text-yellow-600 bg-yellow-100',
      icon: <FiAlertTriangle className="w-4 h-4" />
    };
    return { 
      status: 'en-stock', 
      label: 'En Stock', 
      color: 'text-green-600 bg-green-100',
      icon: <FiCheckCircle className="w-4 h-4" />
    };
  };

  // Calculate discount percentage
  const getDiscountPercentage = (price: number, comparePrice: number) => {
    if (!comparePrice || comparePrice <= price) return 0;
    return Math.round(((comparePrice - price) / comparePrice) * 100);
  };

  // Handle quantity change
  const handleQuantityChange = (increment: boolean) => {
    if (increment) {
      const maxStock = (product?.StockActual ?? 0) - (product?.StockReservado ?? 0);
      if (quantity < maxStock) {
        setQuantity(quantity + 1);
      }
    } else {
      if (quantity > 1) {
        setQuantity(quantity - 1);
      }
    }
  };

  // Handle image navigation
  const handleImageNavigation = (direction: 'prev' | 'next') => {
    if (!product?.Imagenes || product.Imagenes.length <= 1) return;
    
    if (direction === 'prev') {
      setSelectedImageIndex(selectedImageIndex === 0 ? product.Imagenes.length - 1 : selectedImageIndex - 1);
    } else {
      setSelectedImageIndex(selectedImageIndex === product.Imagenes.length - 1 ? 0 : selectedImageIndex + 1);
    }
  };

  // ✅ LOADING STATE MEJORADO
  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900 p-4 sm:p-6">
        <div className="max-w-7xl mx-auto">
          <div className="flex items-center justify-center h-64">
            <div className="text-center">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto mb-4"></div>
              <span className="text-gray-600 dark:text-gray-400">Cargando producto...</span>
              <div className="text-xs text-gray-500 dark:text-gray-500 mt-2">
                ID: {id}
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  // ✅ ERROR STATE MEJORADO
  if (error || !product) {
    return (
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900 p-4 sm:p-6">
        <div className="max-w-7xl mx-auto">
          <div className="text-center py-12">
            <div className="text-6xl mb-4">❌</div>
            <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-2">
              Error al cargar el producto
            </h3>
            <p className="text-gray-600 dark:text-gray-400 mb-2">
              {error || 'El producto no fue encontrado'}
            </p>
            <div className="text-xs text-gray-500 dark:text-gray-500 mb-6">
              ID solicitado: {id}
            </div>
            <div className="flex justify-center gap-4">
              <button
                onClick={() => navigate('/products')}
                className="inline-flex items-center px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-medium transition-colors"
              >
                <FiArrowLeft className="w-4 h-4 mr-2" />
                Volver a Productos
              </button>
              <button
                onClick={() => window.location.reload()}
                className="inline-flex items-center px-4 py-2 bg-gray-600 hover:bg-gray-700 text-white rounded-lg text-sm font-medium transition-colors"
              >
                Reintentar
              </button>
              <button
                onClick={() => {
                  console.log('🔧 === DEBUG MANUAL ===');
                  (window as any).debugProductDetail?.(parseInt(id || '1'));
                }}
                className="inline-flex items-center px-4 py-2 bg-yellow-600 hover:bg-yellow-700 text-white rounded-lg text-sm font-medium transition-colors"
              >
                Debug
              </button>
            </div>
          </div>
        </div>
      </div>
    );
  }

  // ✅ VALIDACIÓN ADICIONAL DEL PRODUCTO
  if (product.Id === 0 || !product.Nombre || product.Nombre === 'Sin nombre') {
    return (
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900 p-4 sm:p-6">
        <div className="max-w-7xl mx-auto">
          <div className="text-center py-12">
            <div className="text-6xl mb-4">⚠️</div>
            <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-2">
              Datos del producto inválidos
            </h3>
            <p className="text-gray-600 dark:text-gray-400 mb-2">
              El producto existe pero tiene datos incompletos
            </p>
            <div className="text-xs text-gray-500 dark:text-gray-500 mb-6 bg-gray-100 dark:bg-gray-800 p-4 rounded-lg">
              <div>ID: {product.Id}</div>
              <div>Nombre: "{product.Nombre}"</div>
              <div>SKU: "{product.SKU}"</div>
            </div>
            <div className="flex justify-center gap-4">
              <button
                onClick={() => navigate('/products')}
                className="inline-flex items-center px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-medium transition-colors"
              >
                <FiArrowLeft className="w-4 h-4 mr-2" />
                Volver a Productos
              </button>
              <button
                onClick={() => {
                  console.log('🔧 === DEBUG PRODUCTO INVÁLIDO ===');
                  console.log('Producto recibido:', product);
                  (window as any).debugProductDetail?.(parseInt(id || '1'));
                }}
                className="inline-flex items-center px-4 py-2 bg-yellow-600 hover:bg-yellow-700 text-white rounded-lg text-sm font-medium transition-colors"
              >
                Debug Mapeo
              </button>
            </div>
          </div>
        </div>
      </div>
    );
  }

  const stockStatus = getStockStatus(product.StockActual, product.StockReservado);
  const hasImages = product.Imagenes && product.Imagenes.length > 0;
  const currentImage = hasImages ? product.Imagenes[selectedImageIndex] : null;
  const discountPercentage = product.PrecioComparacion ? getDiscountPercentage(product.Precio, product.PrecioComparacion) : 0;
  const availableStock = (product.StockActual ?? 0) - (product.StockReservado ?? 0);

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
      <div className="max-w-7xl mx-auto p-4 sm:p-6 lg:p-8">
        {/* ✅ DEBUG INFO - Solo en desarrollo */}
        {process.env.NODE_ENV === 'development' && (
          <div className="mb-4 p-2 bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded text-xs">
            <div className="font-medium text-blue-800 dark:text-blue-200 mb-1">Debug Info:</div>
            <div className="text-blue-600 dark:text-blue-300">
              ID: {product.Id} | Nombre: "{product.Nombre}" | SKU: "{product.SKU}" | Precio: {product.Precio}
            </div>
          </div>
        )}

        {/* Header Navigation */}
        <div className="mb-6">
          <div className="flex items-center gap-4 mb-4">
            <button
              onClick={() => navigate('/products')}
              className="inline-flex items-center text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white transition-colors"
            >
              <FiArrowLeft className="w-4 h-4 mr-2" />
              Volver a Productos
            </button>
            
            <div className="flex items-center gap-2 text-sm text-gray-500 dark:text-gray-400">
              <Link to="/" className="hover:text-gray-700 dark:hover:text-gray-300">Inicio</Link>
              <span>/</span>
              <Link to="/products" className="hover:text-gray-700 dark:hover:text-gray-300">Productos</Link>
              <span>/</span>
              <span className="font-medium text-gray-900 dark:text-white">{product.Nombre}</span>
            </div>
          </div>

          {/* Action Buttons */}
          <div className="flex flex-wrap gap-2">
            <PermissionGate permissions={[PERMISSIONS.PRODUCTS.EDIT]}>
              <Link
                to={`/products/${product.Id}/edit`}
                className="inline-flex items-center px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-medium transition-colors"
              >
                <FiEdit className="w-4 h-4 mr-2" />
                Editar Producto
              </Link>
              
              <button
                onClick={handleToggleStatus}
                disabled={actionLoading}
                className={`inline-flex items-center px-4 py-2 rounded-lg text-sm font-medium transition-colors disabled:opacity-50 ${
                  product.Activo
                    ? 'bg-orange-600 hover:bg-orange-700 text-white'
                    : 'bg-green-600 hover:bg-green-700 text-white'
                }`}
              >
                {actionLoading ? (
                  <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
                ) : product.Activo ? (
                  <FiToggleLeft className="w-4 h-4 mr-2" />
                ) : (
                  <FiToggleRight className="w-4 h-4 mr-2" />
                )}
                {product.Activo ? 'Desactivar' : 'Activar'}
              </button>
            </PermissionGate>

            <PermissionGate permissions={[PERMISSIONS.PRODUCTS.DELETE]}>
              <button
                onClick={handleDelete}
                disabled={actionLoading}
                className="inline-flex items-center px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg text-sm font-medium transition-colors disabled:opacity-50"
              >
                <FiTrash2 className="w-4 h-4 mr-2" />
                Eliminar
              </button>
            </PermissionGate>

            <button className="inline-flex items-center px-4 py-2 bg-gray-600 hover:bg-gray-700 text-white rounded-lg text-sm font-medium transition-colors">
              <FiShare2 className="w-4 h-4 mr-2" />
              Compartir
            </button>
          </div>
        </div>

        {/* Main Content */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8 mb-8">
          {/* Image Gallery */}
          <div className="space-y-4">
            {/* Main Image */}
            <div className="relative aspect-square bg-white dark:bg-gray-800 rounded-lg overflow-hidden border border-gray-200 dark:border-gray-700">
              {currentImage ? (
                <>
                  <img
                    src={currentImage.Url}
                    alt={currentImage.AltText || product.Nombre}
                    className="w-full h-full object-cover cursor-zoom-in"
                    onClick={() => setShowImageModal(true)}
                  />
                  <button
                    onClick={() => setShowImageModal(true)}
                    className="absolute top-4 right-4 p-2 bg-white dark:bg-gray-800 rounded-full shadow-lg hover:shadow-xl transition-all"
                  >
                    <FiZoomIn className="w-4 h-4 text-gray-600 dark:text-gray-400" />
                  </button>
                </>
              ) : (
                <div className="w-full h-full flex items-center justify-center">
                  <div className="text-center">
                    <FiImage className="w-16 h-16 text-gray-400 mx-auto mb-4" />
                    <p className="text-gray-500 dark:text-gray-400">Sin imagen disponible</p>
                  </div>
                </div>
              )}

              {/* Image Navigation */}
              {hasImages && product.Imagenes.length > 1 && (
                <>
                  <button
                    onClick={() => handleImageNavigation('prev')}
                    className="absolute left-4 top-1/2 transform -translate-y-1/2 p-2 bg-white dark:bg-gray-800 rounded-full shadow-lg hover:shadow-xl transition-all"
                  >
                    <FiChevronLeft className="w-4 h-4 text-gray-600 dark:text-gray-400" />
                  </button>
                  <button
                    onClick={() => handleImageNavigation('next')}
                    className="absolute right-4 top-1/2 transform -translate-y-1/2 p-2 bg-white dark:bg-gray-800 rounded-full shadow-lg hover:shadow-xl transition-all"
                  >
                    <FiChevronRight className="w-4 h-4 text-gray-600 dark:text-gray-400" />
                  </button>
                </>
              )}
            </div>

            {/* Thumbnail Gallery */}
            {hasImages && product.Imagenes.length > 1 && (
              <div className="grid grid-cols-4 sm:grid-cols-6 gap-2">
                {product.Imagenes.map((image, index) => (
                  <button
                    key={`thumbnail-${image.Id}-${index}`}
                    onClick={() => setSelectedImageIndex(index)}
                    className={`aspect-square rounded-lg overflow-hidden border-2 transition-all ${
                      selectedImageIndex === index
                        ? 'border-blue-500 ring-2 ring-blue-200 dark:ring-blue-800'
                        : 'border-gray-200 dark:border-gray-700 hover:border-gray-300 dark:hover:border-gray-600'
                    }`}
                  >
                    <img
                      src={image.Url}
                      alt={image.AltText || `${product.Nombre} - Imagen ${index + 1}`}
                      className="w-full h-full object-cover"
                    />
                  </button>
                ))}
              </div>
            )}
          </div>

          {/* Product Information */}
          <div className="space-y-6">
            {/* Status Badge */}
            <div className="flex items-center gap-3">
              <span className={`inline-flex items-center gap-2 px-3 py-1 rounded-full text-sm font-medium ${
                product.Activo
                  ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
                  : 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200'
              }`}>
                {product.Activo ? <FiCheckCircle className="w-4 h-4" /> : <FiXCircle className="w-4 h-4" />}
                {product.Activo ? 'Activo' : 'Inactivo'}
              </span>

              {product.Destacado && (
                <span className="inline-flex items-center gap-2 px-3 py-1 rounded-full text-sm font-medium bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200">
                  <FiStar className="w-4 h-4" />
                  Destacado
                </span>
              )}

              {product.Nuevo && (
                <span className="inline-flex items-center gap-2 px-3 py-1 rounded-full text-sm font-medium bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200">
                  Nuevo
                </span>
              )}

              {product.EnOferta && (
                <span className="inline-flex items-center gap-2 px-3 py-1 rounded-full text-sm font-medium bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200">
                  En Oferta
                </span>
              )}
            </div>

            {/* Title and SKU */}
            <div>
              <h1 className="text-3xl sm:text-4xl font-bold text-gray-900 dark:text-white mb-2">
                {product.Nombre}
              </h1>
              <div className="flex items-center gap-4 text-sm text-gray-600 dark:text-gray-400">
                <span className="flex items-center gap-1">
                  <FiTag className="w-4 h-4" />
                  SKU: {product.SKU}
                </span>
                <span className="flex items-center gap-1">
                  <FiEye className="w-4 h-4" />
                  ID: {product.Id}
                </span>
              </div>
            </div>

            {/* Price */}
            <div className="space-y-2">
              <div className="flex items-center gap-4">
                <span className="text-3xl font-bold text-gray-900 dark:text-white">
                  {formatPrice(product.Precio)}
                </span>
                {product.PrecioComparacion && product.PrecioComparacion > product.Precio && (
                  <>
                    <span className="text-xl text-gray-500 dark:text-gray-400 line-through">
                      {formatPrice(product.PrecioComparacion)}
                    </span>
                    <span className="inline-flex items-center px-2 py-1 bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200 rounded-full text-sm font-medium">
                      -{discountPercentage}%
                    </span>
                  </>
                )}
              </div>
              {product.Costo && (
                <p className="text-sm text-gray-600 dark:text-gray-400">
                  Costo: {formatPrice(product.Costo)}
                </p>
              )}
            </div>

            {/* Stock Information */}
            <div className="bg-gray-50 dark:bg-gray-800 rounded-lg p-4">
              <div className="flex items-center justify-between mb-3">
                <span className="font-medium text-gray-900 dark:text-white">Disponibilidad</span>
                <span className={`inline-flex items-center gap-2 px-3 py-1 rounded-full text-sm font-medium ${stockStatus.color}`}>
                  {stockStatus.icon}
                  {stockStatus.label}
                </span>
              </div>
              
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div>
                  <span className="text-gray-600 dark:text-gray-400">Stock Total:</span>
                  <p className="font-medium text-gray-900 dark:text-white">{product.StockActual ?? 0}</p>
                </div>
                <div>
                  <span className="text-gray-600 dark:text-gray-400">Disponible:</span>
                  <p className="font-medium text-gray-900 dark:text-white">{availableStock}</p>
                </div>
                {(product.StockReservado ?? 0) > 0 && (
                  <div className="col-span-2">
                    <span className="text-gray-600 dark:text-gray-400">Reservado:</span>
                    <p className="font-medium text-orange-600 dark:text-orange-400">{product.StockReservado}</p>
                  </div>
                )}
              </div>
            </div>

            {/* Quantity Selector */}
            {availableStock > 0 && (
              <div className="space-y-4">
                <div className="flex items-center gap-4">
                  <span className="font-medium text-gray-900 dark:text-white">Cantidad:</span>
                  <div className="flex items-center border border-gray-300 dark:border-gray-600 rounded-lg">
                    <button
                      onClick={() => handleQuantityChange(false)}
                      disabled={quantity <= 1}
                      className="px-3 py-2 text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      -
                    </button>
                    <span className="px-4 py-2 font-medium text-gray-900 dark:text-white border-x border-gray-300 dark:border-gray-600">
                      {quantity}
                    </span>
                    <button
                      onClick={() => handleQuantityChange(true)}
                      disabled={quantity >= availableStock}
                      className="px-3 py-2 text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      +
                    </button>
                  </div>
                  <span className="text-sm text-gray-600 dark:text-gray-400">
                    ({availableStock} disponibles)
                  </span>
                </div>

                {/* Action Buttons */}
                <div className="flex gap-3">
                  <button className="flex-1 inline-flex items-center justify-center px-6 py-3 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium transition-colors">
                    <FiShoppingCart className="w-5 h-5 mr-2" />
                    Agregar al Carrito
                  </button>
                  <button className="px-4 py-3 border border-gray-300 dark:border-gray-600 rounded-lg text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors">
                    <FiHeart className="w-5 h-5" />
                  </button>
                </div>
              </div>
            )}

            {/* Category and Brand */}
            <div className="grid grid-cols-2 gap-4 text-sm">
              <div>
                <span className="text-gray-600 dark:text-gray-400">Categoría:</span>
                <p className="font-medium text-gray-900 dark:text-white">
                  {product.CategoriaNombre || 'Sin categoría'}
                </p>
              </div>
              <div>
                <span className="text-gray-600 dark:text-gray-400">Marca:</span>
                <p className="font-medium text-gray-900 dark:text-white">
                  {product.MarcaNombre || 'Sin marca'}
                </p>
              </div>
            </div>

            {/* Shipping and Features */}
            <div className="space-y-3 text-sm">
              {product.RequiereEnvio && (
                <div className="flex items-center gap-2 text-gray-600 dark:text-gray-400">
                  <FiTruck className="w-4 h-4" />
                  <span>Envío disponible</span>
                </div>
              )}
              {product.PermiteReseñas && (
                <div className="flex items-center gap-2 text-gray-600 dark:text-gray-400">
                  <FiStar className="w-4 h-4" />
                  <span>Permite reseñas</span>
                </div>
              )}
              {product.Garantia && (
                <div className="flex items-center gap-2 text-gray-600 dark:text-gray-400">
                  <FiInfo className="w-4 h-4" />
                  <span>Garantía: {product.Garantia}</span>
                </div>
              )}
            </div>
          </div>
        </div>

        {/* Product Details Tabs */}
        <div className="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700">
          <div className="border-b border-gray-200 dark:border-gray-700">
            <div className="flex flex-wrap gap-0">
              <button className="px-6 py-4 text-sm font-medium text-blue-600 dark:text-blue-400 border-b-2 border-blue-600 dark:border-blue-400">
                Descripción
              </button>
              <button className="px-6 py-4 text-sm font-medium text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white">
                Especificaciones
              </button>
              <button className="px-6 py-4 text-sm font-medium text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white">
                Información Adicional
              </button>
            </div>
          </div>

          <div className="p-6">
            {/* Description Tab Content */}
            <div className="space-y-4">
              {product.DescripcionCorta && (
                <div>
                  <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-2">
                    Descripción Corta
                  </h3>
                  <p className="text-gray-600 dark:text-gray-400">
                    {product.DescripcionCorta}
                  </p>
                </div>
              )}

              {product.DescripcionLarga && (
                <div>
                  <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-2">
                    Descripción Detallada
                  </h3>
                  <div className="prose dark:prose-invert max-w-none">
                    <p className="text-gray-600 dark:text-gray-400 whitespace-pre-wrap">
                      {product.DescripcionLarga}
                    </p>
                  </div>
                </div>
              )}

              {!product.DescripcionCorta && !product.DescripcionLarga && (
                <p className="text-gray-500 dark:text-gray-400 italic">
                  No hay descripción disponible para este producto.
                </p>
              )}
            </div>
          </div>
        </div>

        {/* Technical Information */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mt-8">
          {/* Product Specifications */}
          <div className="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 p-6">
            <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-4 flex items-center gap-2">
              <FiPackage className="w-5 h-5" />
              Especificaciones
            </h3>
            
            <div className="space-y-3">
              <div className="flex justify-between py-2 border-b border-gray-100 dark:border-gray-700">
                <span className="text-gray-600 dark:text-gray-400">Tipo:</span>
                <span className="font-medium text-gray-900 dark:text-white capitalize">
                  {product.Tipo || 'Simple'}
                </span>
              </div>
              
              <div className="flex justify-between py-2 border-b border-gray-100 dark:border-gray-700">
                <span className="text-gray-600 dark:text-gray-400">Estado:</span>
                <span className="font-medium text-gray-900 dark:text-white capitalize">
                  {product.Estado || 'Disponible'}
                </span>
              </div>

              {product.Peso && (
                <div className="flex justify-between py-2 border-b border-gray-100 dark:border-gray-700">
                  <span className="text-gray-600 dark:text-gray-400">Peso:</span>
                  <span className="font-medium text-gray-900 dark:text-white">
                    {product.Peso}g
                  </span>
                </div>
              )}

              {product.Dimensiones && (
                <div className="flex justify-between py-2 border-b border-gray-100 dark:border-gray-700">
                  <span className="text-gray-600 dark:text-gray-400">Dimensiones:</span>
                  <span className="font-medium text-gray-900 dark:text-white">
                    {product.Dimensiones} cm
                  </span>
                </div>
              )}

              <div className="flex justify-between py-2 border-b border-gray-100 dark:border-gray-700">
                <span className="text-gray-600 dark:text-gray-400">Requiere Envío:</span>
                <span className="font-medium text-gray-900 dark:text-white">
                  {product.RequiereEnvio ? 'Sí' : 'No'}
                </span>
              </div>

              <div className="flex justify-between py-2">
                <span className="text-gray-600 dark:text-gray-400">Permite Reseñas:</span>
                <span className="font-medium text-gray-900 dark:text-white">
                  {product.PermiteReseñas ? 'Sí' : 'No'}
                </span>
              </div>
            </div>
          </div>

          {/* SEO and Metadata */}
          <div className="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 p-6">
            <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-4 flex items-center gap-2">
              <FiInfo className="w-5 h-5" />
              Información del Sistema
            </h3>
            
            <div className="space-y-3">
              <div className="flex justify-between py-2 border-b border-gray-100 dark:border-gray-700">
                <span className="text-gray-600 dark:text-gray-400">Slug:</span>
                <span className="font-medium text-gray-900 dark:text-white text-sm">
                  {product.Slug || 'No definido'}
                </span>
              </div>

              <div className="flex justify-between py-2 border-b border-gray-100 dark:border-gray-700">
                <span className="text-gray-600 dark:text-gray-400">Orden:</span>
                <span className="font-medium text-gray-900 dark:text-white">
                  {product.Orden}
                </span>
              </div>

              <div className="flex justify-between py-2 border-b border-gray-100 dark:border-gray-700">
                <span className="text-gray-600 dark:text-gray-400">Fecha Creación:</span>
                <span className="font-medium text-gray-900 dark:text-white text-sm">
                  {formatDate(product.FechaCreacion)}
                </span>
              </div>

              {product.FechaModificacion && (
                <div className="flex justify-between py-2 border-b border-gray-100 dark:border-gray-700">
                  <span className="text-gray-600 dark:text-gray-400">Última Modificación:</span>
                  <span className="font-medium text-gray-900 dark:text-white text-sm">
                    {formatDate(product.FechaModificacion)}
                  </span>
                </div>
              )}

              {product.MetaTitulo && (
                <div className="py-2 border-b border-gray-100 dark:border-gray-700">
                  <span className="text-gray-600 dark:text-gray-400 block mb-1">Meta Título:</span>
                  <span className="font-medium text-gray-900 dark:text-white text-sm">
                    {product.MetaTitulo}
                  </span>
                </div>
              )}

              {product.MetaDescripcion && (
                <div className="py-2 border-b border-gray-100 dark:border-gray-700">
                  <span className="text-gray-600 dark:text-gray-400 block mb-1">Meta Descripción:</span>
                  <span className="font-medium text-gray-900 dark:text-white text-sm">
                    {product.MetaDescripcion}
                  </span>
                </div>
              )}

              {product.PalabrasClaves && (
                <div className="py-2">
                  <span className="text-gray-600 dark:text-gray-400 block mb-1">Palabras Clave:</span>
                  <span className="font-medium text-gray-900 dark:text-white text-sm">
                    {product.PalabrasClaves}
                  </span>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Image Modal */}
      {showImageModal && currentImage && (
        <div className="fixed inset-0 bg-black bg-opacity-75 flex items-center justify-center z-50 p-4">
          <div className="relative max-w-4xl max-h-full">
            <button
              onClick={() => setShowImageModal(false)}
              className="absolute top-4 right-4 p-2 bg-white rounded-full text-gray-600 hover:text-gray-900 z-10"
            >
              <FiXCircle className="w-6 h-6" />
            </button>
            
            <img
              src={currentImage.Url}
              alt={currentImage.AltText || product.Nombre}
              className="max-w-full max-h-full object-contain rounded-lg"
            />
            
            {hasImages && product.Imagenes.length > 1 && (
              <>
                <button
                  onClick={() => handleImageNavigation('prev')}
                  className="absolute left-4 top-1/2 transform -translate-y-1/2 p-3 bg-white rounded-full text-gray-600 hover:text-gray-900"
                >
                  <FiChevronLeft className="w-6 h-6" />
                </button>
                <button
                  onClick={() => handleImageNavigation('next')}
                  className="absolute right-4 top-1/2 transform -translate-y-1/2 p-3 bg-white rounded-full text-gray-600 hover:text-gray-900"
                >
                  <FiChevronRight className="w-6 h-6" />
                </button>
              </>
            )}
            
            <div className="absolute bottom-4 left-1/2 transform -translate-x-1/2 bg-black bg-opacity-50 text-white px-3 py-1 rounded-full text-sm">
              {selectedImageIndex + 1} / {product.Imagenes.length}
            </div>
          </div>
        </div>
      )}

      {/* Loading Overlay */}
      {actionLoading && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 flex items-center gap-3">
            <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-blue-600"></div>
            <span className="text-gray-700 dark:text-gray-300">Procesando...</span>
          </div>
        </div>
      )}
    </div>
  );
};

export default ProductDetail;