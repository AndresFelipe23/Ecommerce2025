// pages/Products/ProductList.tsx
import React, { useState, useEffect, useCallback } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { ProductDto, ProductFilterDto } from '../../types/products';
import { usePermissions } from '../../hooks/usePermissions';
import PermissionGate from '../../components/auth/PermissionGate';
import { PERMISSIONS } from '../../types/permissions';
import { AxiosError } from 'axios';
import { FiEye, FiEdit, FiTrash2, FiToggleLeft, FiToggleRight, FiPackage, FiTrendingUp, FiAlertTriangle } from 'react-icons/fi';
import productsService, { ApiResponse, PagedResult } from '../../services/productsService';

const ProductList: React.FC = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const [products, setProducts] = useState<ProductDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState<number | null>(null);
  const [totalItems, setTotalItems] = useState(0);
  const [currentPage, setCurrentPage] = useState(1);
  const [selectedProducts, setSelectedProducts] = useState<number[]>([]);
  const [showFilters, setShowFilters] = useState(false);

  // Filter states - ✅ CORREGIDO: Cambiar filtro inicial para mostrar todos los productos
  const [filters, setFilters] = useState<ProductFilterDto>({
    Busqueda: searchParams.get('search') || '',
    // ✅ CAMBIO CRÍTICO: Cambiar la lógica del filtro Activo
    Activo: (() => {
      const activoParam = searchParams.get('activo') || searchParams.get('Activo');
      if (activoParam === 'true') return true;
      if (activoParam === 'false') return false;
      return undefined; // ✅ IMPORTANTE: undefined = mostrar todos
    })(),
    CategoriaId: searchParams.get('categoryId') ? parseInt(searchParams.get('categoryId')!) : undefined,
    MarcaId: searchParams.get('brandId') ? parseInt(searchParams.get('brandId')!) : undefined,
    EnOferta: searchParams.get('onSale') === 'true' ? true : searchParams.get('onSale') === 'false' ? false : undefined,
    Destacado: searchParams.get('featured') === 'true' ? true : searchParams.get('featured') === 'false' ? false : undefined,
    BajoStock: searchParams.get('lowStock') === 'true',
    SinStock: searchParams.get('outOfStock') === 'true',
    PrecioMin: searchParams.get('minPrice') ? parseFloat(searchParams.get('minPrice')!) : undefined,
    PrecioMax: searchParams.get('maxPrice') ? parseFloat(searchParams.get('maxPrice')!) : undefined,
    Page: parseInt(searchParams.get('page') || '1'),
    PageSize: parseInt(searchParams.get('pageSize') || '10'),
    SortBy: searchParams.get('sortBy') || 'nombre',
    SortDescending: searchParams.get('sortDirection') === 'desc'
  });

  usePermissions();

  // Load products - ✅ MEJORADO: Agregar debug y mejor manejo de errores
  const loadProducts = useCallback(async () => {
    try {
      setLoading(true);
      console.log('🚀 Cargando productos con filtros:', filters);
      
      const result: PagedResult<ProductDto> = await productsService.getProducts(filters);
      
      console.log('📦 Respuesta recibida:', {
        totalItems: result.totalItems,
        itemsCount: result.items?.length,
        currentPage: result.page,
        sampleProduct: result.items?.[0]
      });
      
      setProducts(result.items || []);
      setTotalItems(result.totalItems);
      setCurrentPage(result.page);
    } catch (error) {
      const axiosError = error as AxiosError<ApiResponse<PagedResult<ProductDto>>>;
      console.error('❌ Error loading products:', {
        status: axiosError.response?.status,
        message: axiosError.response?.data?.message,
        data: axiosError.response?.data
      });
      alert(axiosError.response?.data?.message || 'Error al cargar los productos');
    } finally {
      setLoading(false);
    }
  }, [filters]);

  // ✅ DEBUG: Log de productos cuando cambian
  useEffect(() => {
    console.log('🔧 === PRODUCT LIST DEBUG ===');
    console.log('📋 Current filters:', filters);
    console.log('📦 Current products count:', products.length);
    console.log('🔢 Total items:', totalItems);
    
    if (products.length > 0) {
      console.log('📋 First product sample:', {
        id: products[0].Id,
        nombre: products[0].Nombre,
        sku: products[0].SKU,
        precio: products[0].Precio,
        activo: products[0].Activo
      });
    }
  }, [filters, products, totalItems]);


  
  // Update URL params when filters change
  useEffect(() => {
    const params = new URLSearchParams();
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        params.set(key.toLowerCase(), value.toString());
      }
    });
    setSearchParams(params);
  }, [filters, setSearchParams]);

  useEffect(() => {
    loadProducts();
  }, [loadProducts]);

  // Handle filter changes
  const handleFilterChange = (key: keyof ProductFilterDto, value: string | number | boolean | undefined) => {
    setFilters(prev => {
      const newFilters = { ...prev };
      if (key === 'Page') {
        newFilters.Page = typeof value === 'number' ? value : prev.Page;
      } else {
        (newFilters as Record<string, unknown>)[key] = value;
        newFilters.Page = 1;
      }
      return newFilters;
    });
  };

  // Handle search
  const handleSearch = (searchTerm: string) => {
    handleFilterChange('Busqueda', searchTerm);
  };

  // Handle pagination
  const handlePageChange = (page: number) => {
    handleFilterChange('Page', page);
  };

  // Handle sort
  const handleSort = (sortBy: string) => {
    const newDirection = filters.SortBy === sortBy && !filters.SortDescending;
    setFilters(prev => ({ ...prev, SortBy: sortBy, SortDescending: newDirection }));
  };

  // Toggle product status
  const handleToggleStatus = async (productId: number) => {
    try {
      setActionLoading(productId);
      await productsService.toggleProductStatus(productId);
      await loadProducts();
    } catch (error) {
      const axiosError = error as AxiosError<ApiResponse<unknown>>;
      console.error('Error toggling status:', axiosError);
      alert(axiosError.response?.data?.message || 'Error al cambiar el estado del producto');
    } finally {
      setActionLoading(null);
    }
  };

  // Delete product
  const handleDelete = async (product: ProductDto) => {
    const confirmed = window.confirm(
      `¿Está seguro de que desea eliminar el producto "${product.Nombre}"?\n\n` +
      `${(product.StockActual ?? 0) > 0
        ? `Este producto tiene ${product.StockActual} unidades en stock y será desactivado en lugar de eliminado.`
        : 'Esta acción no se puede deshacer.'
      }`
    );
    
    if (!confirmed) return;

    try {
      setActionLoading(product.Id);
      await productsService.deleteProduct(product.Id);
      await loadProducts();
    } catch (error) {
      const axiosError = error as AxiosError<ApiResponse<unknown>>;
      console.error('Error deleting product:', axiosError);
      alert(axiosError.response?.data?.message || 'Error al eliminar el producto');
    } finally {
      setActionLoading(null);
    }
  };

  // Handle selection
  const handleSelectProduct = (productId: number) => {
    setSelectedProducts(prev => 
      prev.includes(productId) 
        ? prev.filter(id => id !== productId)
        : [...prev, productId]
    );
  };


  // Bulk actions
  const handleBulkToggleStatus = async (active: boolean) => {
    if (selectedProducts.length === 0) return;
    
    try {
      setActionLoading(-1);
      await productsService.bulkToggleStatus({ ProductIds: selectedProducts, Activo: active });
      setSelectedProducts([]);
      await loadProducts();
    } catch (error) {
      console.error('Error in bulk operation:', error);
      alert('Error en la operación masiva');
    } finally {
      setActionLoading(null);
    }
  };

  const handleBulkDelete = async () => {
    if (selectedProducts.length === 0) return;
    
    const confirmed = window.confirm(
      `¿Está seguro de que desea eliminar ${selectedProducts.length} producto(s) seleccionado(s)?\n\n` +
      'Los productos con stock serán desactivados en lugar de eliminados.'
    );
    
    if (!confirmed) return;

    try {
      setActionLoading(-1);
      await productsService.bulkDelete({ ProductIds: selectedProducts });
      setSelectedProducts([]);
      await loadProducts();
    } catch (error) {
      console.error('Error in bulk delete:', error);
      alert('Error en la eliminación masiva');
    } finally {
      setActionLoading(null);
    }
  };

  // Reset filters
  const resetFilters = () => {
    setFilters({
      Busqueda: '',
      Activo: undefined,
      CategoriaId: undefined,
      MarcaId: undefined,
      EnOferta: undefined,
      Destacado: undefined,
      BajoStock: false,
      SinStock: false,
      PrecioMin: undefined,
      PrecioMax: undefined,
      Page: 1,
      PageSize: 10,
      SortBy: 'nombre',
      SortDescending: false
    });
  };

  // Generate pagination
  const generatePagination = () => {
    const totalPages = Math.ceil(totalItems / (filters.PageSize || 10));
    const pages = [];
    const maxVisible = 5;
    
    let start = Math.max(1, currentPage - Math.floor(maxVisible / 2));
    const end = Math.min(totalPages, start + maxVisible - 1);
    
    if (end - start + 1 < maxVisible) {
      start = Math.max(1, end - maxVisible + 1);
    }
    
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    
    return { pages, totalPages };
  };

  // ✅ MEJORADO: Format price con manejo de valores null/undefined
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

  // ✅ MEJORADO: Get stock status con manejo de undefined
  const getStockStatus = (stock: number | undefined, stockReservado: number | undefined = 0) => {
    const currentStock = stock ?? 0;
    const reservedStock = stockReservado ?? 0;
    const availableStock = currentStock - reservedStock;
    
    if (availableStock <= 0) return { status: 'sin-stock', label: 'Sin Stock', color: 'text-red-600' };
    if (availableStock <= 5) return { status: 'bajo-stock', label: 'Bajo Stock', color: 'text-yellow-600' };
    return { status: 'en-stock', label: 'En Stock', color: 'text-green-600' };
  };

  const { pages, totalPages } = generatePagination();

  if (loading && products.length === 0) {
    return (
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900 p-4 sm:p-6">
        <div className="max-w-7xl mx-auto">
          <div className="flex items-center justify-center h-64">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
            <span className="ml-2 text-gray-600 dark:text-gray-400">Cargando productos...</span>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
      <div className="max-w-7xl mx-auto p-4 sm:p-6 lg:p-8">
        {/* Header */}
        <div className="mb-6">
          <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
            <div>
              <h1 className="text-2xl sm:text-3xl font-bold text-gray-900 dark:text-white">
                Productos
              </h1>
              <p className="text-sm text-gray-600 dark:text-gray-400 mt-1">
                Gestiona el catálogo completo de productos
              </p>
            </div>
            
            <div className="flex flex-col sm:flex-row gap-2">
              <PermissionGate permissions={[PERMISSIONS.PRODUCTS.CREATE]}>
                <Link
                  to="/products/create"
                  className="inline-flex items-center justify-center px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-medium transition-colors"
                >
                  <FiPackage className="w-4 h-4 mr-2" />
                  Nuevo Producto
                </Link>
              </PermissionGate>
              
              <button
                onClick={() => setShowFilters(!showFilters)}
                className={`inline-flex items-center justify-center px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
                  showFilters 
                    ? 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200'
                    : 'bg-white text-gray-700 border border-gray-300 hover:bg-gray-50 dark:bg-gray-800 dark:text-gray-300 dark:border-gray-600 dark:hover:bg-gray-700'
                }`}
              >
                <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 4a1 1 0 011-1h16a1 1 0 011 1v2.586a1 1 0 01-.293.707l-6.414 6.414a1 1 0 00-.293.707V17l-4 4v-6.586a1 1 0 00-.293-.707L3.293 7.414A1 1 0 013 6.707V4z" />
                </svg>
                Filtros
              </button>
            </div>
          </div>

          {/* Quick Stats */}
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 mt-6">
            <div className="bg-white dark:bg-gray-800 rounded-lg p-4 border border-gray-200 dark:border-gray-700">
              <div className="flex items-center gap-2 mb-2">
                <FiPackage className="w-6 h-6 text-blue-600 dark:text-blue-400" />
                <div className="text-2xl font-bold text-blue-600 dark:text-blue-400">
                  {totalItems}
                </div>
              </div>
              <div className="text-sm text-gray-600 dark:text-gray-400">
                Total Productos
              </div>
            </div>
            <div className="bg-white dark:bg-gray-800 rounded-lg p-4 border border-gray-200 dark:border-gray-700">
              <div className="flex items-center gap-2 mb-2">
                <svg className="w-6 h-6 text-green-600 dark:text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                <div className="text-2xl font-bold text-green-600 dark:text-green-400">
                  {products.filter(p => p.Activo).length}
                </div>
              </div>
              <div className="text-sm text-gray-600 dark:text-gray-400">
                Activos
              </div>
            </div>
            <div className="bg-white dark:bg-gray-800 rounded-lg p-4 border border-gray-200 dark:border-gray-700">
              <div className="flex items-center gap-2 mb-2">
                <FiTrendingUp className="w-6 h-6 text-purple-600 dark:text-purple-400" />
                <div className="text-2xl font-bold text-purple-600 dark:text-purple-400">
                  {products.filter(p => p.EnOferta).length}
                </div>
              </div>
              <div className="text-sm text-gray-600 dark:text-gray-400">
                En Oferta
              </div>
            </div>
            <div className="bg-white dark:bg-gray-800 rounded-lg p-4 border border-gray-200 dark:border-gray-700">
              <div className="flex items-center gap-2 mb-2">
                <FiAlertTriangle className="w-6 h-6 text-orange-600 dark:text-orange-400" />
                <div className="text-2xl font-bold text-orange-600 dark:text-orange-400">
                  {products.filter(p => (p.StockActual ?? 0) <= 5).length}
                </div>
              </div>
              <div className="text-sm text-gray-600 dark:text-gray-400">
                Bajo Stock
              </div>
            </div>
          </div>
        </div>

        {/* Filters */}
        {showFilters && (
          <div className="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 p-4 sm:p-6 mb-6">
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
              {/* Search */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Buscar
                </label>
                <input
                  type="text"
                  placeholder="Nombre, SKU o descripción..."
                  value={filters.Busqueda || ''}
                  onChange={(e) => handleSearch(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white text-sm"
                />
              </div>

              {/* Status Filter - ✅ CORREGIDO */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Estado
                </label>
                <select
                  value={filters.Activo === undefined ? 'all' : filters.Activo.toString()}
                  onChange={(e) => {
                    const value = e.target.value;
                    const activo = value === 'all' ? undefined : value === 'true';
                    handleFilterChange('Activo', activo);
                  }}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white text-sm"
                >
                  <option value="all">Todos</option>
                  <option value="true">Activos</option>
                  <option value="false">Inactivos</option>
                </select>
              </div>

              {/* Price Min */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Precio Mínimo
                </label>
                <input
                  type="number"
                  placeholder="0"
                  value={filters.PrecioMin ?? ''}
                  onChange={(e) => handleFilterChange('PrecioMin', e.target.value ? parseFloat(e.target.value) : undefined)}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white text-sm"
                />
              </div>

              {/* Price Max */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Precio Máximo
                </label>
                <input
                  type="number"
                  placeholder="999999"
                  value={filters.PrecioMax ?? ''}
                  onChange={(e) => handleFilterChange('PrecioMax', e.target.value ? parseFloat(e.target.value) : undefined)}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white text-sm"
                />
              </div>

              {/* Featured Filter */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Destacado
                </label>
                <select
                  value={filters.Destacado === undefined ? '' : filters.Destacado.toString()}
                  onChange={(e) => handleFilterChange('Destacado', e.target.value === '' ? undefined : e.target.value === 'true')}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white text-sm"
                >
                  <option value="">Todos</option>
                  <option value="true">Destacados</option>
                  <option value="false">No destacados</option>
                </select>
              </div>

              {/* On Sale Filter */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  En Oferta
                </label>
                <select
                  value={filters.EnOferta === undefined ? '' : filters.EnOferta.toString()}
                  onChange={(e) => handleFilterChange('EnOferta', e.target.value === '' ? undefined : e.target.value === 'true')}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white text-sm"
                >
                  <option value="">Todos</option>
                  <option value="true">En oferta</option>
                  <option value="false">Sin oferta</option>
                </select>
              </div>

              {/* Stock Filters */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Stock
                </label>
                <div className="flex gap-2">
                  <label className="flex items-center">
                    <input
                      type="checkbox"
                      checked={filters.BajoStock || false}
                      onChange={(e) => handleFilterChange('BajoStock', e.target.checked)}
                      className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                    />
                    <span className="ml-2 text-sm text-gray-700 dark:text-gray-300">Bajo</span>
                  </label>
                  <label className="flex items-center">
                    <input
                      type="checkbox"
                      checked={filters.SinStock || false}
                      onChange={(e) => handleFilterChange('SinStock', e.target.checked)}
                      className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                    />
                    <span className="ml-2 text-sm text-gray-700 dark:text-gray-300">Sin</span>
                  </label>
                </div>
              </div>

              {/* Page Size */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Elementos por página
                </label>
                <select
                  value={filters.PageSize || 10}
                  onChange={(e) => handleFilterChange('PageSize', parseInt(e.target.value))}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white text-sm"
                >
                  <option value={5}>5</option>
                  <option value={10}>10</option>
                  <option value={25}>25</option>
                  <option value={50}>50</option>
                </select>
              </div>
            </div>

            <div className="flex flex-col sm:flex-row gap-2 mt-4">
              <button
                onClick={resetFilters}
                className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-600 transition-colors"
              >
                Limpiar Filtros
              </button>
            </div>
          </div>
        )}

        {/* Bulk Actions */}
        {selectedProducts.length > 0 && (
          <div className="bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg p-4 mb-6">
            <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
              <div className="flex items-center gap-2">
                <span className="text-sm font-medium text-blue-800 dark:text-blue-200">
                  {selectedProducts.length} producto(s) seleccionado(s)
                </span>
              </div>
              
              <div className="flex flex-wrap gap-2">
                <PermissionGate permissions={[PERMISSIONS.PRODUCTS.EDIT]}>
                  <button
                    onClick={() => handleBulkToggleStatus(true)}
                    disabled={actionLoading === -1}
                    className="px-3 py-1 bg-green-600 hover:bg-green-700 text-white rounded text-sm font-medium transition-colors disabled:opacity-50"
                  >
                    Activar Seleccionados
                  </button>
                  <button
                    onClick={() => handleBulkToggleStatus(false)}
                    disabled={actionLoading === -1}
                    className="px-3 py-1 bg-yellow-600 hover:bg-yellow-700 text-white rounded text-sm font-medium transition-colors disabled:opacity-50"
                  >
                    Desactivar Seleccionados
                  </button>
                </PermissionGate>
                <PermissionGate permissions={[PERMISSIONS.PRODUCTS.DELETE]}>
                  <button
                    onClick={handleBulkDelete}
                    disabled={actionLoading === -1}
                    className="px-3 py-1 bg-red-600 hover:bg-red-700 text-white rounded text-sm font-medium transition-colors disabled:opacity-50"
                  >
                    Eliminar Seleccionados
                  </button>
                </PermissionGate>
                <button
                  onClick={() => setSelectedProducts([])}
                  className="px-3 py-1 bg-gray-600 hover:bg-gray-700 text-white rounded text-sm font-medium transition-colors"
                >
                  Cancelar Selección
                </button>
              </div>
            </div>
          </div>
        )}

        {/* Table */}
        <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden">
          {/* Desktop Table */}
          <div className="hidden lg:block overflow-x-auto">
            <table className="w-full">
              <thead className="bg-gray-50 dark:bg-gray-700">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Imagen
                  </th>
                  <th 
                    className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider cursor-pointer hover:bg-gray-100 dark:hover:bg-gray-600"
                    onClick={() => handleSort('nombre')}
                  >
                    <div className="flex items-center gap-1">
                      Producto
                      {filters.SortBy === 'nombre' && (
                        <svg className={`w-4 h-4 ${filters.SortDescending ? 'rotate-180' : ''}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 15l7-7 7 7" />
                        </svg>
                      )}
                    </div>
                  </th>
                  <th 
                    className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider cursor-pointer hover:bg-gray-100 dark:hover:bg-gray-600"
                    onClick={() => handleSort('precio')}
                  >
                    <div className="flex items-center gap-1">
                      Precio
                      {filters.SortBy === 'precio' && (
                        <svg className={`w-4 h-4 ${filters.SortDescending ? 'rotate-180' : ''}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 15l7-7 7 7" />
                        </svg>
                      )}
                    </div>
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Categoría
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Stock
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Estado
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Acciones
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                {products.map((product) => {
                  const stockStatus = getStockStatus(product.StockActual, product.StockReservado);
                  const hasImage = product.Imagenes && product.Imagenes.length > 0;
                  const mainImage = hasImage ? product.Imagenes.find(img => img.EsPrincipal) || product.Imagenes[0] : null;
                  
                  return (
                    <tr key={`product-${product.Id}`} className="hover:bg-gray-50 dark:hover:bg-gray-700">
                      <td className="px-6 py-4">
                        <input
                          type="checkbox"
                          checked={selectedProducts.includes(product.Id)}
                          onChange={() => handleSelectProduct(product.Id)}
                          className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                        />
                      </td>
                      <td className="px-6 py-4">
                        <div className="w-12 h-12 bg-gray-100 dark:bg-gray-600 rounded-lg overflow-hidden flex items-center justify-center">
                          {mainImage ? (
                            <img
                              src={mainImage.Url}
                              alt={mainImage.AltText || product.Nombre}
                              className="w-full h-full object-cover"
                            />
                          ) : (
                            <FiPackage className="w-6 h-6 text-gray-400" />
                          )}
                        </div>
                      </td>
                      <td className="px-6 py-4">
                        <div className="flex items-start gap-3">
                          <div className="flex-1 min-w-0">
                            <Link
                              to={`/products/${product.Id}`}
                              className="text-sm font-medium text-gray-900 dark:text-white hover:text-blue-600 dark:hover:text-blue-400 line-clamp-1"
                            >
                              {/* ✅ CORREGIDO: Manejar nombre null/undefined */}
                              {product.Nombre || 'Sin nombre'}
                            </Link>
                            <div className="flex items-center gap-2 mt-1">
                              <span className="text-xs text-gray-500 dark:text-gray-400">
                                {/* ✅ CORREGIDO: Manejar SKU null/undefined */}
                                SKU: {product.SKU || 'Sin SKU'}
                              </span>
                              {product.Destacado && (
                                <span className="inline-flex items-center px-1.5 py-0.5 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200">
                                  ⭐ Destacado
                                </span>
                              )}
                            </div>
                            {product.DescripcionCorta && (
                              <p className="text-xs text-gray-500 dark:text-gray-400 mt-1 line-clamp-1">
                                {product.DescripcionCorta}
                              </p>
                            )}
                          </div>
                        </div>
                      </td>
                      <td className="px-6 py-4">
                        <div className="text-sm font-medium text-gray-900 dark:text-white">
                          {/* ✅ CORREGIDO: Usar función mejorada formatPrice */}
                          {formatPrice(product.Precio)}
                        </div>
                        {product.EnOferta && product.PrecioComparacion && (
                          <div className="flex items-center gap-2 mt-1">
                            <span className="text-xs text-gray-500 dark:text-gray-400 line-through">
                              {formatPrice(product.PrecioComparacion)}
                            </span>
                            <span className="inline-flex items-center px-1.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200">
                              Oferta
                            </span>
                          </div>
                        )}
                      </td>
                      <td className="px-6 py-4">
                        <div className="text-sm text-gray-900 dark:text-white">
                          {/* ✅ CORREGIDO: Manejar categoría null/undefined */}
                          {product.CategoriaNombre || 'Sin categoría'}
                        </div>
                        <div className="text-xs text-gray-500 dark:text-gray-400">
                          {/* ✅ CORREGIDO: Manejar marca null/undefined */}
                          {product.MarcaNombre || 'Sin marca'}
                        </div>
                      </td>
                      <td className="px-6 py-4">
                        <div className="flex items-center gap-2">
                          <div className="text-sm font-medium text-gray-900 dark:text-white">
                            {/* ✅ CORREGIDO: Manejar stock null/undefined */}
                            {product.StockActual ?? 0}
                          </div>
                          <span className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${
                            stockStatus.status === 'sin-stock'
                              ? 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200'
                              : stockStatus.status === 'bajo-stock'
                              ? 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200'
                              : 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
                          }`}>
                            {stockStatus.label}
                          </span>
                        </div>
                        {(product.StockReservado ?? 0) > 0 && (
                          <div className="text-xs text-orange-600 dark:text-orange-400 mt-1">
                            {product.StockReservado} reservado
                          </div>
                        )}
                      </td>
                      <td className="px-6 py-4">
                        <div className="flex items-center gap-2">
                          {product.Activo ? (
                            <svg className="w-5 h-5 text-green-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                            </svg>
                          ) : (
                            <svg className="w-5 h-5 text-red-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
                            </svg>
                          )}
                          <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                            product.Activo
                              ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
                              : 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200'
                          }`}>
                            {product.Activo ? 'Activo' : 'Inactivo'}
                          </span>
                        </div>
                      </td>
                      <td className="px-6 py-4 text-right text-sm font-medium">
                        <div className="flex justify-end gap-2">
                          {/* Botón Ver */}
                          <Link
                            to={`/products/${product.Id}`}
                            className="flex items-center justify-center w-8 h-8 bg-blue-100 text-blue-600 border border-blue-600 rounded hover:bg-blue-200 transition relative group"
                            title="Ver producto"
                          >
                            <FiEye className="w-4 h-4" />
                            <span className="absolute bottom-full mb-2 left-1/2 transform -translate-x-1/2 bg-gray-800 text-white text-xs rounded py-1 px-2 opacity-0 group-hover:opacity-100 transition-opacity duration-300 whitespace-nowrap pointer-events-none">
                              Ver producto
                            </span>
                          </Link>

                          {/* Botón Editar */}
                          <PermissionGate permissions={[PERMISSIONS.PRODUCTS.EDIT]}>
                            <Link
                              to={`/products/${product.Id}/edit`}
                              className="flex items-center justify-center w-8 h-8 bg-yellow-100 text-yellow-600 border border-yellow-600 rounded hover:bg-yellow-200 transition relative group"
                              title="Editar producto"
                            >
                              <FiEdit className="w-4 h-4" />
                              <span className="absolute bottom-full mb-2 left-1/2 transform -translate-x-1/2 bg-gray-800 text-white text-xs rounded py-1 px-2 opacity-0 group-hover:opacity-100 transition-opacity duration-300 whitespace-nowrap pointer-events-none">
                                Editar producto
                              </span>
                            </Link>

                            {/* Botón Activar/Desactivar */}
                            <button
                              onClick={() => handleToggleStatus(product.Id)}
                              disabled={actionLoading === product.Id}
                              className={`flex items-center justify-center w-8 h-8 ${
                                product.Activo
                                  ? 'bg-orange-100 text-orange-600 border border-orange-600 hover:bg-orange-200'
                                  : 'bg-green-100 text-green-600 border border-green-600 hover:bg-green-200'
                              } rounded transition disabled:opacity-50 relative group`}
                              title={product.Activo ? "Desactivar producto" : "Activar producto"}
                            >
                              {actionLoading === product.Id ? (
                                '...'
                              ) : product.Activo ? (
                                <FiToggleLeft className="w-4 h-4" />
                              ) : (
                                <FiToggleRight className="w-4 h-4" />
                              )}
                              <span className="absolute bottom-full mb-2 left-1/2 transform -translate-x-1/2 bg-gray-800 text-white text-xs rounded py-1 px-2 opacity-0 group-hover:opacity-100 transition-opacity duration-300 whitespace-nowrap pointer-events-none">
                                {product.Activo ? "Desactivar producto" : "Activar producto"}
                              </span>
                            </button>
                          </PermissionGate>

                          {/* Botón Eliminar */}
                          <PermissionGate permissions={[PERMISSIONS.PRODUCTS.DELETE]}>
                            <button
                              onClick={() => handleDelete(product)}
                              disabled={actionLoading === product.Id}
                              className="flex items-center justify-center w-8 h-8 bg-red-100 text-red-600 border border-red-600 rounded hover:bg-red-200 transition disabled:opacity-50 relative group"
                              title="Eliminar producto"
                            >
                              {actionLoading === product.Id ? (
                                '...'
                              ) : (
                                <FiTrash2 className="w-4 h-4" />
                              )}
                              <span className="absolute bottom-full mb-2 left-1/2 transform -translate-x-1/2 bg-gray-800 text-white text-xs rounded py-1 px-2 opacity-0 group-hover:opacity-100 transition-opacity duration-300 whitespace-nowrap pointer-events-none">
                                Eliminar producto
                              </span>
                            </button>
                          </PermissionGate>
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>

          {/* Mobile Cards */}
          <div className="lg:hidden">
            {products.map((product) => {
              const stockStatus = getStockStatus(product.StockActual, product.StockReservado);
              const hasImage = product.Imagenes && product.Imagenes.length > 0;
              const mainImage = hasImage ? product.Imagenes.find(img => img.EsPrincipal) || product.Imagenes[0] : null;
              
              return (
                <div key={`mobile-product-${product.Id}`} className="border-b border-gray-200 dark:border-gray-700 p-4">
                  <div className="flex items-start gap-3">
                    <input
                      type="checkbox"
                      checked={selectedProducts.includes(product.Id)}
                      onChange={() => handleSelectProduct(product.Id)}
                      className="rounded border-gray-300 text-blue-600 focus:ring-blue-500 mt-1"
                    />
                    
                    {/* Product Image */}
                    <div className="w-16 h-16 bg-gray-100 dark:bg-gray-600 rounded-lg overflow-hidden flex items-center justify-center flex-shrink-0">
                      {mainImage ? (
                        <img
                          src={mainImage.Url}
                          alt={mainImage.AltText || product.Nombre}
                          className="w-full h-full object-cover"
                        />
                      ) : (
                        <FiPackage className="w-8 h-8 text-gray-400" />
                      )}
                    </div>
                    
                    <div className="flex-1 min-w-0">
                      <div className="flex items-start justify-between mb-2">
                        <div className="flex items-center gap-2 flex-wrap">
                          <Link
                            to={`/products/${product.Id}`}
                            className="text-base font-medium text-gray-900 dark:text-white hover:text-blue-600 dark:hover:text-blue-400"
                          >
                            {/* ✅ CORREGIDO: Manejar nombre null/undefined */}
                            {product.Nombre || 'Sin nombre'}
                          </Link>
                          {product.Destacado && (
                            <span className="inline-flex items-center px-1.5 py-0.5 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200">
                              ⭐
                            </span>
                          )}
                        </div>
                        
                        <span className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium flex-shrink-0 ${
                          product.Activo
                            ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
                            : 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200'
                        }`}>
                          {product.Activo ? 'Activo' : 'Inactivo'}
                        </span>
                      </div>

                      <div className="text-xs text-gray-500 dark:text-gray-400 mb-2">
                        {/* ✅ CORREGIDO: Manejar valores null/undefined */}
                        SKU: {product.SKU || 'Sin SKU'} • {product.CategoriaNombre || 'Sin categoría'} • {product.MarcaNombre || 'Sin marca'}
                      </div>

                      {product.DescripcionCorta && (
                        <p className="text-sm text-gray-600 dark:text-gray-400 mb-3 line-clamp-2">
                          {product.DescripcionCorta}
                        </p>
                      )}

                      <div className="grid grid-cols-2 gap-4 mb-3 text-sm">
                        <div>
                          <span className="text-gray-500 dark:text-gray-400">Precio:</span>
                          <div className="font-medium text-gray-900 dark:text-white">
                            {/* ✅ CORREGIDO: Usar función mejorada formatPrice */}
                            {formatPrice(product.Precio)}
                          </div>
                          {product.EnOferta && product.PrecioComparacion && (
                            <div className="flex items-center gap-2 mt-1">
                              <span className="text-xs text-gray-500 dark:text-gray-400 line-through">
                                {formatPrice(product.PrecioComparacion)}
                              </span>
                              <span className="inline-flex items-center px-1.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200">
                                Oferta
                              </span>
                            </div>
                          )}
                        </div>
                        
                        <div>
                          <span className="text-gray-500 dark:text-gray-400">Stock:</span>
                          <div className="flex items-center gap-2">
                            <span className="font-medium text-gray-900 dark:text-white">
                              {/* ✅ CORREGIDO: Manejar stock null/undefined */}
                              {product.StockActual ?? 0}
                            </span>
                            <span className={`inline-flex items-center px-1.5 py-0.5 rounded-full text-xs font-medium ${
                              stockStatus.status === 'sin-stock'
                                ? 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200'
                                : stockStatus.status === 'bajo-stock'
                                ? 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200'
                                : 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
                            }`}>
                              {stockStatus.label}
                            </span>
                          </div>
                          {(product.StockReservado ?? 0) > 0 && (
                            <div className="text-xs text-orange-600 dark:text-orange-400 mt-1">
                              {product.StockReservado} reservado
                            </div>
                          )}
                        </div>
                      </div>

                      <div className="text-xs text-gray-500 dark:text-gray-400 mb-3">
                        Creado: {new Date(product.FechaCreacion).toLocaleDateString('es-ES')}
                      </div>

                      {/* Mobile Actions */}
                      <div className="flex flex-wrap gap-2">
                        <Link
                          to={`/products/${product.Id}`}
                          className="px-3 py-1 bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200 rounded-full text-xs font-medium hover:bg-blue-200 dark:hover:bg-blue-800 transition-colors"
                        >
                          Ver Detalles
                        </Link>
                        
                        <PermissionGate permissions={[PERMISSIONS.PRODUCTS.EDIT]}>
                          <Link
                            to={`/products/${product.Id}/edit`}
                            className="px-3 py-1 bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200 rounded-full text-xs font-medium hover:bg-yellow-200 dark:hover:bg-yellow-800 transition-colors"
                          >
                            Editar
                          </Link>
                          
                          <button
                            onClick={() => handleToggleStatus(product.Id)}
                            disabled={actionLoading === product.Id}
                            className={`px-3 py-1 rounded-full text-xs font-medium transition-colors disabled:opacity-50 ${
                              product.Activo
                                ? 'bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-200 hover:bg-orange-200 dark:hover:bg-orange-800'
                                : 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200 hover:bg-green-200 dark:hover:bg-green-800'
                            }`}
                          >
                            {actionLoading === product.Id ? '...' : (product.Activo ? 'Desactivar' : 'Activar')}
                          </button>
                        </PermissionGate>
                        
                        <PermissionGate permissions={[PERMISSIONS.PRODUCTS.DELETE]}>
                          <button
                            onClick={() => handleDelete(product)}
                            disabled={actionLoading === product.Id}
                            className="px-3 py-1 bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200 rounded-full text-xs font-medium hover:bg-red-200 dark:hover:bg-red-800 transition-colors disabled:opacity-50"
                          >
                            {actionLoading === product.Id ? '...' : 'Eliminar'}
                          </button>
                        </PermissionGate>
                      </div>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>

          {/* Empty State */}
          {products.length === 0 && !loading && (
            <div className="text-center py-12">
              <div className="text-6xl mb-4">📦</div>
              <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-2">
                No se encontraron productos
              </h3>
              <p className="text-gray-600 dark:text-gray-400 mb-6 max-w-md mx-auto">
                {filters.Busqueda || filters.Activo !== undefined || filters.EnOferta !== undefined
                  ? 'No hay productos que coincidan con los filtros aplicados.'
                  : 'Aún no hay productos creados en el sistema.'
                }
              </p>
              <div className="flex flex-col sm:flex-row gap-2 justify-center">
                <PermissionGate permissions={[PERMISSIONS.PRODUCTS.CREATE]}>
                  <Link
                    to="/products/create"
                    className="inline-flex items-center justify-center px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-medium transition-colors"
                  >
                    <FiPackage className="w-4 h-4 mr-2" />
                    Crear Primer Producto
                  </Link>
                </PermissionGate>
                {(filters.Busqueda || filters.Activo !== undefined || filters.EnOferta !== undefined) && (
                  <button
                    onClick={resetFilters}
                    className="inline-flex items-center justify-center px-4 py-2 bg-gray-600 hover:bg-gray-700 text-white rounded-lg text-sm font-medium transition-colors"
                  >
                    Limpiar Filtros
                  </button>
                )}
              </div>
            </div>
          )}
        </div>

        {/* Pagination */}
        {totalPages > 1 && (
          <div className="mt-6 flex flex-col sm:flex-row items-center justify-between gap-4">
            <div className="text-sm text-gray-700 dark:text-gray-300">
              Mostrando {((currentPage - 1) * (filters.PageSize || 10)) + 1} - {Math.min(currentPage * (filters.PageSize || 10), totalItems)} de {totalItems} resultados
            </div>
            
            <div className="flex items-center gap-2">
              {/* Previous Page */}
              <button
                onClick={() => handlePageChange(currentPage - 1)}
                disabled={currentPage === 1}
                className="px-3 py-2 text-sm font-medium text-gray-500 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed dark:bg-gray-800 dark:border-gray-600 dark:text-gray-400 dark:hover:bg-gray-700"
              >
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                </svg>
              </button>

              {/* Page Numbers */}
              <div className="hidden sm:flex items-center gap-1">
                {currentPage > 3 && (
                  <>
                    <button
                      key="first-page"
                      onClick={() => handlePageChange(1)}
                      className="px-3 py-2 text-sm font-medium text-gray-500 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 dark:bg-gray-800 dark:border-gray-600 dark:text-gray-400 dark:hover:bg-gray-700"
                    >
                      1
                    </button>
                    {currentPage > 4 && (
                      <span key="first-ellipsis" className="px-2 text-gray-500 dark:text-gray-400">...</span>
                    )}
                  </>
                )}
                
                {pages.map((page) => (
                  <button
                    key={`page-${page}`}
                    onClick={() => handlePageChange(page)}
                    className={`px-3 py-2 text-sm font-medium rounded-lg border ${
                      page === currentPage
                        ? 'bg-blue-600 text-white border-blue-600'
                        : 'text-gray-500 bg-white border-gray-300 hover:bg-gray-50 dark:bg-gray-800 dark:border-gray-600 dark:text-gray-400 dark:hover:bg-gray-700'
                    }`}
                  >
                    {page}
                  </button>
                ))}
                
                {currentPage < totalPages - 2 && (
                  <>
                    {currentPage < totalPages - 3 && (
                      <span key="last-ellipsis" className="px-2 text-gray-500 dark:text-gray-400">...</span>
                    )}
                    <button
                      key="last-page"
                      onClick={() => handlePageChange(totalPages)}
                      className="px-3 py-2 text-sm font-medium text-gray-500 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 dark:bg-gray-800 dark:border-gray-600 dark:text-gray-400 dark:hover:bg-gray-700"
                    >
                      {totalPages}
                    </button>
                  </>
                )}
              </div>

              {/* Mobile Page Info */}
              <div className="sm:hidden px-3 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded-lg">
                {currentPage} / {totalPages}
              </div>

              {/* Next Page */}
              <button
                onClick={() => handlePageChange(currentPage + 1)}
                disabled={currentPage === totalPages}
                className="px-3 py-2 text-sm font-medium text-gray-500 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed dark:bg-gray-800 dark:border-gray-600 dark:text-gray-400 dark:hover:bg-gray-700"
              >
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                </svg>
              </button>
            </div>
          </div>
        )}

        {/* Loading Overlay */}
        {loading && products.length > 0 && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white dark:bg-gray-800 rounded-lg p-6 flex items-center gap-3">
              <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-blue-600"></div>
              <span className="text-gray-700 dark:text-gray-300">Actualizando...</span>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default ProductList;