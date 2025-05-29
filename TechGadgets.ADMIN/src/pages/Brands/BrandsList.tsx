// pages/Brands/BrandsList.tsx
import React, { useState, useEffect, useCallback } from 'react';
import { Link } from 'react-router-dom';
import brandsService, { Brand, BrandFilter } from '../../services/brandsService';
import { usePermissions } from '../../hooks/usePermissions';
import PermissionGate from '../../components/auth/PermissionGate';
import { PERMISSIONS } from '../../types/permissions';
import { AxiosError } from 'axios';
import { ApiResponse } from '../../services/brandsService';

const BrandsList: React.FC = () => {
  const [brands, setBrands] = useState<Brand[]>([]);
  const [loading, setLoading] = useState(true);
  const [totalItems, setTotalItems] = useState(0);
  const [selectedBrands, setSelectedBrands] = useState<number[]>([]);
  const [bulkLoading, setBulkLoading] = useState(false);
  
  // Filters and pagination
  const [filters, setFilters] = useState<BrandFilter>({
    page: 1,
    pageSize: 10,
    sortBy: 'nombre',
    sortDescending: false
  });

  // Search state
  const [searchTerm, setSearchTerm] = useState('');
  const [activeFilter, setActiveFilter] = useState<boolean | undefined>(undefined);

  usePermissions();

  // Load brands
  const loadBrands = useCallback(async () => {
    try {
      setLoading(true);
      const result = await brandsService.getBrands(filters);
      setBrands(result.items);
      setTotalItems(result.totalItems);
    } catch (error) {
      const axiosError = error as AxiosError<ApiResponse<unknown>>;
      console.error('Error loading brands:', axiosError);
      // Here you could show a toast notification
    } finally {
      setLoading(false);
    }
  }, [filters]);

  useEffect(() => {
    loadBrands();
  }, [loadBrands]);

  // Handle search
  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setFilters(prev => ({
      ...prev,
      page: 1,
      nombre: searchTerm || undefined
    }));
  };

  // Handle filter change
  const handleFilterChange = (newFilters: Partial<BrandFilter>) => {
    setFilters(prev => ({
      ...prev,
      page: 1,
      ...newFilters
    }));
  };

  // Handle sort
  const handleSort = (sortBy: string) => {
    setFilters(prev => ({
      ...prev,
      sortBy,
      sortDescending: prev.sortBy === sortBy ? !prev.sortDescending : false
    }));
  };

  // Handle pagination
  const handlePageChange = (page: number) => {
    setFilters(prev => ({ ...prev, page }));
  };

  // Handle selection
  const handleSelectBrand = (brandId: number, selected: boolean) => {
    if (selected) {
      setSelectedBrands(prev => [...prev, brandId]);
    } else {
      setSelectedBrands(prev => prev.filter(id => id !== brandId));
    }
  };

  const handleSelectAll = (selected: boolean) => {
    if (selected) {
      setSelectedBrands(brands.map(b => b.id));
    } else {
      setSelectedBrands([]);
    }
  };

  // Toggle status
  const handleToggleStatus = async (brandId: number) => {
    try {
      await brandsService.toggleBrandStatus(brandId);
      await loadBrands();
    } catch (error) {
      const axiosError = error as AxiosError<ApiResponse<unknown>>;
      console.error('Error toggling status:', axiosError);
    }
  };

  // Bulk operations
  const handleBulkToggleStatus = async (active: boolean) => {
    if (selectedBrands.length === 0) return;
    
    try {
      setBulkLoading(true);
      await brandsService.bulkToggleStatus(selectedBrands, active);
      setSelectedBrands([]);
      await loadBrands();
    } catch (error) {
      const axiosError = error as AxiosError<ApiResponse<unknown>>;
      console.error('Error in bulk operation:', axiosError);
    } finally {
      setBulkLoading(false);
    }
  };

  // Delete brand
  const handleDelete = async (brandId: number) => {
    if (!window.confirm('¿Está seguro de que desea eliminar esta marca?')) return;
    
    try {
      await brandsService.deleteBrand(brandId);
      await loadBrands();
    } catch (error) {
      const axiosError = error as AxiosError<ApiResponse<unknown>>;
      console.error('Error deleting brand:', axiosError);
      alert(axiosError.response?.data?.message || 'Error al eliminar la marca');
    }
  };

  const totalPages = Math.ceil(totalItems / (filters.pageSize || 10));

  return (
    <div className="p-6">
      {/* Header */}
      <div className="flex justify-between items-center mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white">
            Gestión de Marcas
          </h1>
          <p className="text-gray-600 dark:text-gray-400">
            Administra las marcas de productos de tu tienda
          </p>
        </div>
        
        <PermissionGate permissions={[PERMISSIONS.BRANDS.CREATE]}>
          <Link
            to="/brands/create"
            className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg flex items-center gap-2 transition-colors"
          >
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
            </svg>
            Nueva Marca
          </Link>
        </PermissionGate>
      </div>

      {/* Filters */}
      <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6 mb-6">
        <form onSubmit={handleSearch} className="flex flex-wrap gap-4">
          <div className="flex-1 min-w-64">
            <input
              type="text"
              placeholder="Buscar por nombre..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent dark:bg-gray-700 dark:text-white"
            />
          </div>
          
          <select
            value={activeFilter?.toString() || ''}
            onChange={(e) => {
              const value = e.target.value;
              handleFilterChange({ 
                activo: value === '' ? undefined : value === 'true' 
              });
            }}
            className="px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
          >
            <option value="">Todos los estados</option>
            <option value="true">Activas</option>
            <option value="false">Inactivas</option>
          </select>

          <button
            type="submit"
            className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg transition-colors"
          >
            Buscar
          </button>
          
          <button
            type="button"
            onClick={() => {
              setSearchTerm('');
              setActiveFilter(undefined);
              setFilters({
                page: 1,
                pageSize: 10,
                sortBy: 'nombre',
                sortDescending: false
              });
            }}
            className="px-4 py-2 bg-gray-500 hover:bg-gray-600 text-white rounded-lg transition-colors"
          >
            Limpiar
          </button>
        </form>
      </div>

      {/* Bulk Actions */}
      {selectedBrands.length > 0 && (
        <PermissionGate permissions={[PERMISSIONS.BRANDS.EDIT]}>
          <div className="bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg p-4 mb-6">
            <div className="flex items-center justify-between">
              <span className="text-blue-800 dark:text-blue-200">
                {selectedBrands.length} marca(s) seleccionada(s)
              </span>
              <div className="flex gap-2">
                <button
                  onClick={() => handleBulkToggleStatus(true)}
                  disabled={bulkLoading}
                  className="px-3 py-1 bg-green-600 hover:bg-green-700 text-white rounded text-sm transition-colors disabled:opacity-50"
                >
                  Activar
                </button>
                <button
                  onClick={() => handleBulkToggleStatus(false)}
                  disabled={bulkLoading}
                  className="px-3 py-1 bg-yellow-600 hover:bg-yellow-700 text-white rounded text-sm transition-colors disabled:opacity-50"
                >
                  Desactivar
                </button>
              </div>
            </div>
          </div>
        </PermissionGate>
      )}

      {/* Table */}
      <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden">
        {loading ? (
          <div className="p-8 text-center">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto mb-4"></div>
            <p className="text-gray-500 dark:text-gray-400">Cargando marcas...</p>
          </div>
        ) : brands.length === 0 ? (
          <div className="p-8 text-center">
            <p className="text-gray-500 dark:text-gray-400">No se encontraron marcas</p>
          </div>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-gray-50 dark:bg-gray-700">
                  <tr>
                    <th className="px-6 py-3 text-left">
                      <input
                        type="checkbox"
                        checked={selectedBrands.length === brands.length}
                        onChange={(e) => handleSelectAll(e.target.checked)}
                        className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                      />
                    </th>
                    <th 
                      className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider cursor-pointer hover:bg-gray-100 dark:hover:bg-gray-600"
                      onClick={() => handleSort('nombre')}
                    >
                      <div className="flex items-center gap-1">
                        Nombre
                        {filters.sortBy === 'nombre' && (
                          <svg className={`w-4 h-4 ${filters.sortDescending ? 'rotate-180' : ''}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 15l7-7 7 7" />
                          </svg>
                        )}
                      </div>
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                      Logo
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                      Productos
                    </th>
                    <th 
                      className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider cursor-pointer hover:bg-gray-100 dark:hover:bg-gray-600"
                      onClick={() => handleSort('activo')}
                    >
                      <div className="flex items-center gap-1">
                        Estado
                        {filters.sortBy === 'activo' && (
                          <svg className={`w-4 h-4 ${filters.sortDescending ? 'rotate-180' : ''}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 15l7-7 7 7" />
                          </svg>
                        )}
                      </div>
                    </th>
                    <th 
                      className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider cursor-pointer hover:bg-gray-100 dark:hover:bg-gray-600"
                      onClick={() => handleSort('fechacreacion')}
                    >
                      <div className="flex items-center gap-1">
                        Fecha Creación
                        {filters.sortBy === 'fechacreacion' && (
                          <svg className={`w-4 h-4 ${filters.sortDescending ? 'rotate-180' : ''}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 15l7-7 7 7" />
                          </svg>
                        )}
                      </div>
                    </th>
                    <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                      Acciones
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                  {brands.map((brand) => (
                    <tr key={brand.id} className="hover:bg-gray-50 dark:hover:bg-gray-700">
                      <td className="px-6 py-4">
                        <input
                          type="checkbox"
                          checked={selectedBrands.includes(brand.id)}
                          onChange={(e) => handleSelectBrand(brand.id, e.target.checked)}
                          className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                        />
                      </td>
                      <td className="px-6 py-4">
                        <div>
                          <div className="text-sm font-medium text-gray-900 dark:text-white">
                            {brand.nombre}
                          </div>
                          {brand.descripcion && (
                            <div className="text-sm text-gray-500 dark:text-gray-400 truncate max-w-xs">
                              {brand.descripcion}
                            </div>
                          )}
                        </div>
                      </td>
                      <td className="px-6 py-4">
                        {brand.logo ? (
                          <img 
                            src={brand.logo} 
                            alt={brand.nombre}
                            className="h-8 w-8 rounded object-contain"
                            onError={(e) => {
                              (e.target as HTMLImageElement).style.display = 'none';
                            }}
                          />
                        ) : (
                          <div className="h-8 w-8 bg-gray-200 dark:bg-gray-600 rounded flex items-center justify-center">
                            <span className="text-xs text-gray-500 dark:text-gray-400">
                              {brand.nombre.charAt(0).toUpperCase()}
                            </span>
                          </div>
                        )}
                      </td>
                      <td className="px-6 py-4 text-sm text-gray-900 dark:text-white">
                        <span className="bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200 px-2 py-1 rounded-full text-xs">
                          {brand.totalProductos}
                        </span>
                      </td>
                      <td className="px-6 py-4">
                        <button
                          onClick={() => handleToggleStatus(brand.id)}
                          className={`px-2 py-1 rounded-full text-xs font-medium ${
                            brand.activo
                              ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
                              : 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200'
                          }`}
                        >
                          {brand.activo ? 'Activa' : 'Inactiva'}
                        </button>
                      </td>
                      <td className="px-6 py-4 text-sm text-gray-500 dark:text-gray-400">
                        {new Date(brand.fechaCreacion).toLocaleDateString()}
                      </td>
                      <td className="px-6 py-4 text-right text-sm font-medium">
                        <div className="flex justify-end gap-2">
                          <Link
                            to={`/brands/${brand.id}`}
                            className="inline-flex items-center gap-1 px-3 py-1.5 text-blue-600 hover:text-blue-900 hover:bg-blue-50 dark:text-blue-400 dark:hover:text-blue-300 dark:hover:bg-blue-900/20 rounded-md transition-colors"
                            title="Ver detalles"
                          >
                            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                            </svg>
                          </Link>
                          
                          <PermissionGate permissions={[PERMISSIONS.BRANDS.EDIT]}>
                            <Link
                              to={`/brands/${brand.id}/edit`}
                              className="inline-flex items-center gap-1 px-3 py-1.5 text-indigo-600 hover:text-indigo-900 hover:bg-indigo-50 dark:text-indigo-400 dark:hover:text-indigo-300 dark:hover:bg-indigo-900/20 rounded-md transition-colors"
                              title="Editar marca"
                            >
                              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                              </svg>
                            </Link>
                            
                            <button
                              onClick={() => handleDelete(brand.id)}
                              className="inline-flex items-center gap-1 px-3 py-1.5 text-red-600 hover:text-red-900 hover:bg-red-50 dark:text-red-400 dark:hover:text-red-300 dark:hover:bg-red-900/20 rounded-md transition-colors"
                              title="Eliminar marca"
                            >
                              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                              </svg>
                            </button>
                          </PermissionGate>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {/* Pagination */}
            {totalPages > 1 && (
              <div className="bg-white dark:bg-gray-800 px-4 py-3 border-t border-gray-200 dark:border-gray-700 sm:px-6">
                <div className="flex items-center justify-between">
                  <div className="text-sm text-gray-700 dark:text-gray-300">
                    Mostrando {((filters.page! - 1) * filters.pageSize!) + 1} a{' '}
                    {Math.min(filters.page! * filters.pageSize!, totalItems)} de{' '}
                    {totalItems} resultados
                  </div>
                  <div className="flex gap-2">
                    <button
                      onClick={() => handlePageChange(filters.page! - 1)}
                      disabled={filters.page === 1}
                      className="px-3 py-1 text-sm bg-gray-200 hover:bg-gray-300 disabled:opacity-50 disabled:cursor-not-allowed rounded dark:bg-gray-600 dark:hover:bg-gray-500"
                    >
                      Anterior
                    </button>
                    
                    {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                      const page = Math.max(1, Math.min(totalPages - 4, filters.page! - 2)) + i;
                      return (
                        <button
                          key={page}
                          onClick={() => handlePageChange(page)}
                          className={`px-3 py-1 text-sm rounded ${
                            page === filters.page
                              ? 'bg-blue-600 text-white'
                              : 'bg-gray-200 hover:bg-gray-300 dark:bg-gray-600 dark:hover:bg-gray-500'
                          }`}
                        >
                          {page}
                        </button>
                      );
                    })}
                    
                    <button
                      onClick={() => handlePageChange(filters.page! + 1)}
                      disabled={filters.page === totalPages}
                      className="px-3 py-1 text-sm bg-gray-200 hover:bg-gray-300 disabled:opacity-50 disabled:cursor-not-allowed rounded dark:bg-gray-600 dark:hover:bg-gray-500"
                    >
                      Siguiente
                    </button>
                  </div>
                </div>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
};

export default BrandsList;