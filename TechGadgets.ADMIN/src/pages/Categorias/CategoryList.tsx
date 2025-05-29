// pages/Categories/CategoryList.tsx
import React, { useState, useEffect, useCallback } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { CategoryDto, CategoryFilterDto } from '../../types/categories';
import { usePermissions } from '../../hooks/usePermissions';
import PermissionGate from '../../components/auth/PermissionGate';
import { PERMISSIONS } from '../../types/permissions';
import { AxiosError } from 'axios';
import categoriesService, { ApiResponse, PagedResult } from '../../services/categoryService';
import { FiEye, FiEdit, FiTrash2, FiToggleLeft, FiToggleRight } from 'react-icons/fi';

const CategoryList: React.FC = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const [categories, setCategories] = useState<CategoryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState<number | null>(null);
  const [totalItems, setTotalItems] = useState(0);
  const [currentPage, setCurrentPage] = useState(1);
  const [selectedCategories, setSelectedCategories] = useState<number[]>([]);
  const [showFilters, setShowFilters] = useState(false);
  const [, setShowBulkActions] = useState(false);

  // Filter states
  const [filters, setFilters] = useState<CategoryFilterDto>({
    search: searchParams.get('search') || '',
    isActive: searchParams.get('isActive') === 'true' ? true : searchParams.get('isActive') === 'false' ? false : undefined,
    categoriaPadreId: searchParams.get('parentId') ? parseInt(searchParams.get('parentId')!) : undefined,
    hasProducts: searchParams.get('hasProducts') === 'true' ? true : searchParams.get('hasProducts') === 'false' ? false : undefined,
    page: parseInt(searchParams.get('page') || '1'),
    pageSize: parseInt(searchParams.get('pageSize') || '10'),
    sortBy: searchParams.get('sortBy') || 'nombre',
    sortDirection: (searchParams.get('sortDirection') as 'asc' | 'desc') || 'asc'
  });

  usePermissions();

  // Load categories
  const loadCategories = useCallback(async () => {
    try {
      setLoading(true);
      const result: PagedResult<CategoryDto> = await categoriesService.getCategories(filters);
      setCategories(result.items);
      setTotalItems(result.totalItems);
      setCurrentPage(result.page);
    } catch (error) {
      const axiosError = error as AxiosError<ApiResponse<PagedResult<CategoryDto>>>;
      console.error('Error loading categories:', axiosError);
      alert(axiosError.response?.data?.message || 'Error al cargar las categorías');
    } finally {
      setLoading(false);
    }
  }, [filters]);

  // Update URL params when filters change
  useEffect(() => {
    const params = new URLSearchParams();
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        params.set(key, value.toString());
      }
    });
    setSearchParams(params);
  }, [filters, setSearchParams]);

  useEffect(() => {
    loadCategories();
  }, [loadCategories]);

  // Handle filter changes
  const handleFilterChange = (key: keyof CategoryFilterDto, value: string | number | boolean | undefined) => {
    setFilters(prev => {
      const newFilters = { ...prev };
      if (key === 'page') {
        newFilters.page = typeof value === 'number' ? value : prev.page;
      } else {
        // Type assertion is safe here because we know the key is valid
        (newFilters as Record<string, unknown>)[key] = value;
        newFilters.page = 1; // Reset to page 1 when other filters change
      }
      return newFilters;
    });
  };

  // Handle search
  const handleSearch = (searchTerm: string) => {
    handleFilterChange('search', searchTerm);
  };

  // Handle pagination
  const handlePageChange = (page: number) => {
    handleFilterChange('page', page);
  };

  // Handle sort
  const handleSort = (sortBy: string) => {
    const newDirection = filters.sortBy === sortBy && filters.sortDirection === 'asc' ? 'desc' : 'asc';
    setFilters(prev => ({ ...prev, sortBy, sortDirection: newDirection }));
  };

  // Toggle category status
  const handleToggleStatus = async (categoryId: number) => {
    try {
      setActionLoading(categoryId);
      await categoriesService.toggleCategoryStatus(categoryId);
      await loadCategories();
    } catch (error) {
      const axiosError = error as AxiosError<ApiResponse<unknown>>;
      console.error('Error toggling status:', axiosError);
      alert(axiosError.response?.data?.message || 'Error al cambiar el estado de la categoría');
    } finally {
      setActionLoading(null);
    }
  };

  // Delete category
  const handleDelete = async (category: CategoryDto) => {
    const confirmed = window.confirm(
      `¿Está seguro de que desea eliminar la categoría "${category.nombre}"?\n\n` +
      `${category.totalProductos > 0 || category.totalSubcategorias > 0
        ? `Esta categoría tiene ${category.totalProductos} producto(s) y ${category.totalSubcategorias} subcategoría(s) asociados y será desactivada en lugar de eliminada.`
        : 'Esta acción no se puede deshacer.'
      }`
    );
    
    if (!confirmed) return;

    try {
      setActionLoading(category.id);
      await categoriesService.deleteCategory(category.id);
      await loadCategories();
    } catch (error) {
      const axiosError = error as AxiosError<ApiResponse<unknown>>;
      console.error('Error deleting category:', axiosError);
      alert(axiosError.response?.data?.message || 'Error al eliminar la categoría');
    } finally {
      setActionLoading(null);
    }
  };

  // Handle selection
  const handleSelectCategory = (categoryId: number) => {
    setSelectedCategories(prev => 
      prev.includes(categoryId) 
        ? prev.filter(id => id !== categoryId)
        : [...prev, categoryId]
    );
  };

  const handleSelectAll = () => {
    setSelectedCategories(
      selectedCategories.length === categories.length 
        ? [] 
        : categories.map(cat => cat.id)
    );
  };

  // Bulk actions
  const handleBulkToggleStatus = async (active: boolean) => {
    if (selectedCategories.length === 0) return;
    
    try {
      setActionLoading(-1);
      // TODO: Implement bulk toggle in service
      await Promise.all(selectedCategories.map(id => 
        categoriesService.updateCategory(id, { 
          nombre: categories.find(c => c.id === id)?.nombre || '',
          activo: active 
        })
      ));
      setSelectedCategories([]);
      await loadCategories();
      setShowBulkActions(false);
    } catch (error) {
      console.error('Error in bulk operation:', error);
      alert('Error en la operación masiva');
    } finally {
      setActionLoading(null);
    }
  };

  // Reset filters
  const resetFilters = () => {
    setFilters({
      search: '',
      isActive: undefined,
      categoriaPadreId: undefined,
      hasProducts: undefined,
      page: 1,
      pageSize: 10,
      sortBy: 'nombre',
      sortDirection: 'asc'
    });
  };

  // Generate pagination
  const generatePagination = () => {
    const totalPages = Math.ceil(totalItems / (filters.pageSize || 10));
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

  const { pages, totalPages } = generatePagination();

  if (loading && categories.length === 0) {
    return (
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900 p-4 sm:p-6">
        <div className="max-w-7xl mx-auto">
          <div className="flex items-center justify-center h-64">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
            <span className="ml-2 text-gray-600 dark:text-gray-400">Cargando categorías...</span>
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
                Categorías
              </h1>
              <p className="text-sm text-gray-600 dark:text-gray-400 mt-1">
                Gestiona las categorías de productos de forma jerárquica
              </p>
            </div>
            
            <div className="flex flex-col sm:flex-row gap-2">
              <PermissionGate permissions={[PERMISSIONS.CATEGORIES.CREATE]}>
                <Link
                  to="/categories/create"
                  className="inline-flex items-center justify-center px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-medium transition-colors"
                >
                  <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                  </svg>
                  Nueva Categoría
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
                <svg className="w-6 h-6 text-blue-600 dark:text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
                </svg>
                <div className="text-2xl font-bold text-blue-600 dark:text-blue-400">
                  {totalItems}
                </div>
              </div>
              <div className="text-sm text-gray-600 dark:text-gray-400">
                Total Categorías
              </div>
            </div>
            <div className="bg-white dark:bg-gray-800 rounded-lg p-4 border border-gray-200 dark:border-gray-700">
              <div className="flex items-center gap-2 mb-2">
                <svg className="w-6 h-6 text-green-600 dark:text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                <div className="text-2xl font-bold text-green-600 dark:text-green-400">
                  {categories.filter(c => c.activo).length}
                </div>
              </div>
              <div className="text-sm text-gray-600 dark:text-gray-400">
                Activas
              </div>
            </div>
            <div className="bg-white dark:bg-gray-800 rounded-lg p-4 border border-gray-200 dark:border-gray-700">
              <div className="flex items-center gap-2 mb-2">
                <svg className="w-6 h-6 text-purple-600 dark:text-purple-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-6l-2-2H5a2 2 0 00-2 2z" />
                </svg>
                <div className="text-2xl font-bold text-purple-600 dark:text-purple-400">
                  {categories.filter(c => !c.categoriaPadreId).length}
                </div>
              </div>
              <div className="text-sm text-gray-600 dark:text-gray-400">
                Principales
              </div>
            </div>
            <div className="bg-white dark:bg-gray-800 rounded-lg p-4 border border-gray-200 dark:border-gray-700">
              <div className="flex items-center gap-2 mb-2">
                <svg className="w-6 h-6 text-orange-600 dark:text-orange-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
                </svg>
                <div className="text-2xl font-bold text-orange-600 dark:text-orange-400">
                  {categories.reduce((sum, c) => sum + (c.totalProductos || 0), 0)}
                </div>
              </div>
              <div className="text-sm text-gray-600 dark:text-gray-400">
                Productos Total
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
                  placeholder="Nombre de categoría..."
                  value={filters.search || ''}
                  onChange={(e) => handleSearch(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white text-sm"
                />
              </div>

              {/* Status Filter */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Estado
                </label>
                <select
                  value={filters.isActive === undefined ? '' : filters.isActive.toString()}
                  onChange={(e) => handleFilterChange('isActive', e.target.value === '' ? undefined : e.target.value === 'true')}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white text-sm"
                >
                  <option value="">Todos</option>
                  <option value="true">Activas</option>
                  <option value="false">Inactivas</option>
                </select>
              </div>

              {/* Has Products Filter */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Con Productos
                </label>
                <select
                  value={filters.hasProducts === undefined ? '' : filters.hasProducts.toString()}
                  onChange={(e) => handleFilterChange('hasProducts', e.target.value === '' ? undefined : e.target.value === 'true')}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white text-sm"
                >
                  <option value="">Todos</option>
                  <option value="true">Con productos</option>
                  <option value="false">Sin productos</option>
                </select>
              </div>

              {/* Page Size */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Elementos por página
                </label>
                <select
                  value={filters.pageSize || 10}
                  onChange={(e) => handleFilterChange('pageSize', parseInt(e.target.value))}
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
        {selectedCategories.length > 0 && (
          <div className="bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg p-4 mb-6">
            <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
              <div className="flex items-center gap-2">
                <span className="text-sm font-medium text-blue-800 dark:text-blue-200">
                  {selectedCategories.length} categoría(s) seleccionada(s)
                </span>
              </div>
              
              <div className="flex flex-wrap gap-2">
                <PermissionGate permissions={[PERMISSIONS.CATEGORIES.EDIT]}>
                  <button
                    onClick={() => handleBulkToggleStatus(true)}
                    disabled={actionLoading === -1}
                    className="px-3 py-1 bg-green-600 hover:bg-green-700 text-white rounded text-sm font-medium transition-colors disabled:opacity-50"
                  >
                    Activar Seleccionadas
                  </button>
                  <button
                    onClick={() => handleBulkToggleStatus(false)}
                    disabled={actionLoading === -1}
                    className="px-3 py-1 bg-yellow-600 hover:bg-yellow-700 text-white rounded text-sm font-medium transition-colors disabled:opacity-50"
                  >
                    Desactivar Seleccionadas
                  </button>
                </PermissionGate>
                <button
                  onClick={() => setSelectedCategories([])}
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
                  <th className="px-6 py-3 text-left">
                    <input
                      type="checkbox"
                      checked={selectedCategories.length === categories.length && categories.length > 0}
                      onChange={handleSelectAll}
                      className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                    />
                  </th>
                  <th 
                    className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider cursor-pointer hover:bg-gray-100 dark:hover:bg-gray-600"
                    onClick={() => handleSort('nombre')}
                  >
                    <div className="flex items-center gap-1">
                      Nombre
                      {filters.sortBy === 'nombre' && (
                        <svg className={`w-4 h-4 ${filters.sortDirection === 'asc' ? '' : 'rotate-180'}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 15l7-7 7 7" />
                        </svg>
                      )}
                    </div>
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Jerarquía
                  </th>
                  <th 
                    className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider cursor-pointer hover:bg-gray-100 dark:hover:bg-gray-600"
                    onClick={() => handleSort('totalProductos')}
                  >
                    <div className="flex items-center gap-1">
                      Productos
                      {filters.sortBy === 'totalProductos' && (
                        <svg className={`w-4 h-4 ${filters.sortDirection === 'asc' ? '' : 'rotate-180'}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 15l7-7 7 7" />
                        </svg>
                      )}
                    </div>
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
                {categories.map((category) => (
                  <tr key={category.id} className="hover:bg-gray-50 dark:hover:bg-gray-700">
                    <td className="px-6 py-4">
                      <input
                        type="checkbox"
                        checked={selectedCategories.includes(category.id)}
                        onChange={() => handleSelectCategory(category.id)}
                        className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                      />
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex items-center">
                        {category.icono ? (
                          <span className="text-2xl mr-3">{category.icono}</span>
                        ) : (
                          <svg className="w-6 h-6 text-gray-400 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                          </svg>
                        )}
                        <div>
                          <Link
                            to={`/categories/${category.id}`}
                            className="text-sm font-medium text-gray-900 dark:text-white hover:text-blue-600 dark:hover:text-blue-400"
                          >
                            {category.nombre}
                          </Link>
                          {category.descripcion && (
                            <p className="text-sm text-gray-500 dark:text-gray-400 truncate max-w-xs">
                              {category.descripcion}
                            </p>
                          )}
                        </div>
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="text-sm text-gray-900 dark:text-white">
                        {category.categoriaPadreNombre ? (
                          <span className="text-gray-500 dark:text-gray-400">
                            {category.categoriaPadreNombre} → 
                          </span>
                        ) : (
                          <span className="font-medium text-blue-600 dark:text-blue-400">
                            Raíz
                          </span>
                        )}
                      </div>
                      {category.totalSubcategorias > 0 && (
                        <div className="text-xs text-gray-500 dark:text-gray-400">
                          {category.totalSubcategorias} subcategoría(s)
                        </div>
                      )}
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex items-center gap-2">
                        <svg className="w-5 h-5 text-blue-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
                        </svg>
                        <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200">
                          {category.totalProductos || 0}
                        </span>
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex items-center gap-2">
                        {category.activo ? (
                          <svg className="w-5 h-5 text-green-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                          </svg>
                        ) : (
                          <svg className="w-5 h-5 text-red-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
                          </svg>
                        )}
                        <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                          category.activo
                            ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
                            : 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200'
                        }`}>
                          {category.activo ? 'Activa' : 'Inactiva'}
                        </span>
                      </div>
                    </td>
                    
                    <td className="px-6 py-4 text-right text-sm font-medium">
  <div className="flex justify-end gap-2">
    {/* Botón Ver */}
    <Link
      to={`/categories/${category.id}`}
      className="flex items-center justify-center w-8 h-8 bg-blue-100 text-blue-600 border border-blue-600 rounded hover:bg-blue-200 transition relative group"
      title="Ver categoría"
    >
      <FiEye className="w-4 h-4" />
      <span className="absolute bottom-full mb-2 left-1/2 transform -translate-x-1/2 bg-gray-800 text-white text-xs rounded py-1 px-2 opacity-0 group-hover:opacity-100 transition-opacity duration-300 whitespace-nowrap pointer-events-none">
        Ver categoría
      </span>
    </Link>

    {/* Botón Editar */}
    <PermissionGate permissions={[PERMISSIONS.CATEGORIES.EDIT]}>
      <Link
        to={`/categories/${category.id}/edit`}
        className="flex items-center justify-center w-8 h-8 bg-yellow-100 text-yellow-600 border border-yellow-600 rounded hover:bg-yellow-200 transition relative group"
        title="Editar categoría"
      >
        <FiEdit className="w-4 h-4" />
        <span className="absolute bottom-full mb-2 left-1/2 transform -translate-x-1/2 bg-gray-800 text-white text-xs rounded py-1 px-2 opacity-0 group-hover:opacity-100 transition-opacity duration-300 whitespace-nowrap pointer-events-none">
          Editar categoría
        </span>
      </Link>

      {/* Botón Activar/Desactivar */}
      <button
        onClick={() => handleToggleStatus(category.id)}
        disabled={actionLoading === category.id}
        className={`flex items-center justify-center w-8 h-8 ${
          category.activo
            ? 'bg-orange-100 text-orange-600 border border-orange-600 hover:bg-orange-200'
            : 'bg-green-100 text-green-600 border border-green-600 hover:bg-green-200'
        } rounded transition disabled:opacity-50 relative group`}
        title={category.activo ? "Desactivar categoría" : "Activar categoría"}
      >
        {actionLoading === category.id ? (
          '...'
        ) : category.activo ? (
          <FiToggleLeft className="w-4 h-4" />
        ) : (
          <FiToggleRight className="w-4 h-4" />
        )}
        <span className="absolute bottom-full mb-2 left-1/2 transform -translate-x-1/2 bg-gray-800 text-white text-xs rounded py-1 px-2 opacity-0 group-hover:opacity-100 transition-opacity duration-300 whitespace-nowrap pointer-events-none">
          {category.activo ? "Desactivar categoría" : "Activar categoría"}
        </span>
      </button>
    </PermissionGate>

    {/* Botón Eliminar */}
    <PermissionGate permissions={[PERMISSIONS.CATEGORIES.DELETE]}>
      <button
        onClick={() => handleDelete(category)}
        disabled={actionLoading === category.id}
        className="flex items-center justify-center w-8 h-8 bg-red-100 text-red-600 border border-red-600 rounded hover:bg-red-200 transition disabled:opacity-50 relative group"
        title="Eliminar categoría"
      >
        {actionLoading === category.id ? (
          '...'
        ) : (
          <FiTrash2 className="w-4 h-4" />
        )}
        <span className="absolute bottom-full mb-2 left-1/2 transform -translate-x-1/2 bg-gray-800 text-white text-xs rounded py-1 px-2 opacity-0 group-hover:opacity-100 transition-opacity duration-300 whitespace-nowrap pointer-events-none">
          Eliminar categoría
        </span>
      </button>
    </PermissionGate>
  </div>
</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Mobile Cards */}
          <div className="lg:hidden">
            {categories.map((category) => (
              <div key={category.id} className="border-b border-gray-200 dark:border-gray-700 p-4">
                <div className="flex items-start gap-3">
                  <input
                    type="checkbox"
                    checked={selectedCategories.includes(category.id)}
                    onChange={() => handleSelectCategory(category.id)}
                    className="rounded border-gray-300 text-blue-600 focus:ring-blue-500 mt-1"
                  />
                  
                  <div className="flex-1 min-w-0">
                    <div className="flex items-start justify-between">
                      <div className="flex items-center gap-2 mb-2">
                        {category.icono ? (
                          <span className="text-lg">{category.icono}</span>
                        ) : (
                          <svg className="w-6 h-6 text-gray-400 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                          </svg>
                        )}
                        <Link
                          to={`/categories/${category.id}`}
                          className="text-base font-medium text-gray-900 dark:text-white hover:text-blue-600 dark:hover:text-blue-400"
                        >
                          {category.nombre}
                        </Link>
                      </div>
                      
                      <span className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${
                        category.activo
                          ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
                          : 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200'
                      }`}>
                        {category.activo ? 'Activa' : 'Inactiva'}
                      </span>
                    </div>

                    {category.descripcion && (
                      <p className="text-sm text-gray-600 dark:text-gray-400 mb-3 line-clamp-2">
                        {category.descripcion}
                      </p>
                    )}

                    <div className="grid grid-cols-2 gap-4 mb-3 text-sm">
                      <div>
                        <span className="text-gray-500 dark:text-gray-400">Jerarquía:</span>
                        <div className="font-medium text-gray-900 dark:text-white">
                          {category.categoriaPadreNombre ? (
                            <span>
                              <span className="text-gray-500 dark:text-gray-400">
                                {category.categoriaPadreNombre} →
                              </span>
                            </span>
                          ) : (
                            <span className="text-blue-600 dark:text-blue-400 font-medium">
                              Raíz
                            </span>
                          )}
                        </div>
                        {category.totalSubcategorias > 0 && (
                          <div className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                            {category.totalSubcategorias} subcategoría(s)
                          </div>
                        )}
                      </div>
                      
                      <div>
                        <span className="text-gray-500 dark:text-gray-400">Productos:</span>
                        <div className="font-medium text-gray-900 dark:text-white">
                          <div className="flex items-center gap-2">
                            <svg className="w-5 h-5 text-blue-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
                            </svg>
                            <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200">
                              {category.totalProductos || 0}
                            </span>
                          </div>
                        </div>
                      </div>
                    </div>

                    <div className="text-xs text-gray-500 dark:text-gray-400 mb-3">
                      Creada: {new Date(category.fechaCreacion).toLocaleDateString('es-ES')}
                    </div>

                    {/* Mobile Actions */}
                    <div className="flex flex-wrap gap-2">
                      <Link
                        to={`/categories/${category.id}`}
                        className="px-3 py-1 bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200 rounded-full text-xs font-medium hover:bg-blue-200 dark:hover:bg-blue-800 transition-colors"
                      >
                        Ver Detalles
                      </Link>
                      
                      <PermissionGate permissions={[PERMISSIONS.CATEGORIES.EDIT]}>
                        <Link
                          to={`/categories/${category.id}/edit`}
                          className="px-3 py-1 bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200 rounded-full text-xs font-medium hover:bg-yellow-200 dark:hover:bg-yellow-800 transition-colors"
                        >
                          Editar
                        </Link>
                        
                        <button
                          onClick={() => handleToggleStatus(category.id)}
                          disabled={actionLoading === category.id}
                          className={`px-3 py-1 rounded-full text-xs font-medium transition-colors disabled:opacity-50 ${
                            category.activo
                              ? 'bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-200 hover:bg-orange-200 dark:hover:bg-orange-800'
                              : 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200 hover:bg-green-200 dark:hover:bg-green-800'
                          }`}
                        >
                          {actionLoading === category.id ? '...' : (category.activo ? 'Desactivar' : 'Activar')}
                        </button>
                      </PermissionGate>
                      
                      <PermissionGate permissions={[PERMISSIONS.CATEGORIES.DELETE]}>
                        <button
                          onClick={() => handleDelete(category)}
                          disabled={actionLoading === category.id}
                          className="px-3 py-1 bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200 rounded-full text-xs font-medium hover:bg-red-200 dark:hover:bg-red-800 transition-colors disabled:opacity-50"
                        >
                          {actionLoading === category.id ? '...' : 'Eliminar'}
                        </button>
                      </PermissionGate>
                    </div>
                  </div>
                </div>
              </div>
            ))}
          </div>

          {/* Empty State */}
          {categories.length === 0 && !loading && (
            <div className="text-center py-12">
              <div className="text-6xl mb-4">📂</div>
              <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-2">
                No se encontraron categorías
              </h3>
              <p className="text-gray-600 dark:text-gray-400 mb-6 max-w-md mx-auto">
                {filters.search || filters.isActive !== undefined || filters.hasProducts !== undefined
                  ? 'No hay categorías que coincidan con los filtros aplicados.'
                  : 'Aún no hay categorías creadas en el sistema.'
                }
              </p>
              <div className="flex flex-col sm:flex-row gap-2 justify-center">
                <PermissionGate permissions={[PERMISSIONS.CATEGORIES.CREATE]}>
                  <Link
                    to="/categories/create"
                    className="inline-flex items-center justify-center px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-medium transition-colors"
                  >
                    <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                    </svg>
                    Crear Primera Categoría
                  </Link>
                </PermissionGate>
                {(filters.search || filters.isActive !== undefined || filters.hasProducts !== undefined) && (
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
              Mostrando {((currentPage - 1) * (filters.pageSize || 10)) + 1} - {Math.min(currentPage * (filters.pageSize || 10), totalItems)} de {totalItems} resultados
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
                      onClick={() => handlePageChange(1)}
                      className="px-3 py-2 text-sm font-medium text-gray-500 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 dark:bg-gray-800 dark:border-gray-600 dark:text-gray-400 dark:hover:bg-gray-700"
                    >
                      1
                    </button>
                    {currentPage > 4 && (
                      <span className="px-2 text-gray-500 dark:text-gray-400">...</span>
                    )}
                  </>
                )}
                
                {pages.map((page) => (
                  <button
                    key={page}
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
                      <span className="px-2 text-gray-500 dark:text-gray-400">...</span>
                    )}
                    <button
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
        {loading && categories.length > 0 && (
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

export default CategoryList;