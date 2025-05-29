// pages/Brands/BrandDetails.tsx
import React, { useState, useEffect } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import brandsService, { Brand } from '../../services/brandsService';
import { usePermissions } from '../../hooks/usePermissions';
import PermissionGate from '../../components/auth/PermissionGate';
import { PERMISSIONS } from '../../types/permissions';
import { AxiosError } from 'axios';
import { ApiResponse } from '../../services/brandsService';

const BrandDetails: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const [brand, setBrand] = useState<Brand | null>(null);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState(false);
  const [showMobileActions, setShowMobileActions] = useState(false);

  usePermissions();

  useEffect(() => {
    if (id) {
      loadBrand(parseInt(id));
    }
  }, [id]);

  const loadBrand = async (brandId: number) => {
    try {
      setLoading(true);
      const brandData = await brandsService.getBrand(brandId);
      setBrand(brandData);
    } catch (error) {
      const axiosError = error as AxiosError<ApiResponse<unknown>>;
      console.error('Error loading brand:', axiosError);
      alert(axiosError.response?.data?.message || 'Error al cargar la marca');
      navigate('/brands');
    } finally {
      setLoading(false);
    }
  };

  const handleToggleStatus = async () => {
    if (!brand) return;
    
    try {
      setActionLoading(true);
      await brandsService.toggleBrandStatus(brand.id);
      setBrand(prev => prev ? { ...prev, activo: !prev.activo } : null);
      setShowMobileActions(false);
    } catch (error) {
      const axiosError = error as AxiosError<ApiResponse<unknown>>;
      console.error('Error toggling status:', axiosError);
      alert(axiosError.response?.data?.message || 'Error al cambiar el estado de la marca');
    } finally {
      setActionLoading(false);
    }
  };

  const handleDelete = async () => {
    if (!brand) return;
    
    const confirmed = window.confirm(
      `¿Está seguro de que desea eliminar la marca "${brand.nombre}"?\n\n` +
      `${brand.totalProductos > 0 
        ? `Esta marca tiene ${brand.totalProductos} producto(s) asociado(s) y será desactivada en lugar de eliminada.`
        : 'Esta acción no se puede deshacer.'
      }`
    );
    
    if (!confirmed) return;

    try {
      setActionLoading(true);
      await brandsService.deleteBrand(brand.id);
      navigate('/brands');
    } catch (error) {
      const axiosError = error as AxiosError<ApiResponse<unknown>>;
      console.error('Error deleting brand:', axiosError);
      alert(axiosError.response?.data?.message || 'Error al eliminar la marca');
    } finally {
      setActionLoading(false);
      setShowMobileActions(false);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen p-4 sm:p-6">
        <div className="flex items-center justify-center h-64">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
          <span className="ml-2 text-gray-600 dark:text-gray-400">Cargando marca...</span>
        </div>
      </div>
    );
  }

  if (!brand) {
    return (
      <div className="min-h-screen p-4 sm:p-6">
        <div className="text-center max-w-md mx-auto mt-20">
          <h2 className="text-xl sm:text-2xl font-bold text-gray-900 dark:text-white mb-2">
            Marca no encontrada
          </h2>
          <p className="text-gray-600 dark:text-gray-400 mb-4 text-sm sm:text-base">
            La marca que busca no existe o ha sido eliminada.
          </p>
          <Link
            to="/brands"
            className="inline-block bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg transition-colors text-sm sm:text-base"
          >
            Volver a Marcas
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
      <div className="max-w-7xl mx-auto p-4 sm:p-6 lg:p-8">
        {/* Mobile-First Header */}
        <div className="mb-6">
          {/* Back Button and Title */}
          <div className="flex items-start gap-3 mb-4">
            <button
              onClick={() => navigate('/brands')}
              className="mt-1 text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200 transition-colors"
            >
              <svg className="w-5 h-5 sm:w-6 sm:h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
              </svg>
            </button>
            <div className="flex-1 min-w-0">
              <h1 className="text-xl sm:text-2xl lg:text-3xl font-bold text-gray-900 dark:text-white break-words">
                {brand.nombre}
              </h1>
              <div className="flex flex-wrap items-center gap-2 mt-2">
                <span className={`px-2 py-1 rounded-full text-xs font-medium ${
                  brand.activo
                    ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
                    : 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200'
                }`}>
                  {brand.activo ? 'Activa' : 'Inactiva'}
                </span>
                <span className="text-xs sm:text-sm text-gray-500 dark:text-gray-400">
                  ID: {brand.id}
                </span>
              </div>
            </div>
          </div>

          {/* Desktop Actions */}
          <PermissionGate permissions={[PERMISSIONS.BRANDS.EDIT, PERMISSIONS.BRANDS.DELETE]}>
            <div className="hidden sm:flex flex-wrap gap-2">
              <button
                onClick={handleToggleStatus}
                disabled={actionLoading}
                className={`px-3 py-2 rounded-lg text-sm font-medium transition-colors disabled:opacity-50 ${
                  brand.activo
                    ? 'bg-yellow-100 hover:bg-yellow-200 text-yellow-800 dark:bg-yellow-900 dark:hover:bg-yellow-800 dark:text-yellow-200'
                    : 'bg-green-100 hover:bg-green-200 text-green-800 dark:bg-green-900 dark:hover:bg-green-800 dark:text-green-200'
                }`}
              >
                {brand.activo ? 'Desactivar' : 'Activar'}
              </button>
              
              <PermissionGate permissions={[PERMISSIONS.BRANDS.EDIT]}>
                <Link
                  to={`/brands/${brand.id}/edit`}
                  className="px-3 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-medium transition-colors"
                >
                  Editar
                </Link>
              </PermissionGate>
              
              <PermissionGate permissions={[PERMISSIONS.BRANDS.DELETE]}>
                <button
                  onClick={handleDelete}
                  disabled={actionLoading}
                  className="px-3 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg text-sm font-medium transition-colors disabled:opacity-50"
                >
                  Eliminar
                </button>
              </PermissionGate>
            </div>
          </PermissionGate>

          {/* Mobile Actions Button */}
          <PermissionGate permissions={[PERMISSIONS.BRANDS.EDIT, PERMISSIONS.BRANDS.DELETE]}>
            <div className="sm:hidden">
              <button
                onClick={() => setShowMobileActions(!showMobileActions)}
                className="w-full px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-medium transition-colors flex items-center justify-center gap-2"
              >
                Acciones
                <svg 
                  className={`w-4 h-4 transition-transform ${showMobileActions ? 'rotate-180' : ''}`} 
                  fill="none" 
                  stroke="currentColor" 
                  viewBox="0 0 24 24"
                >
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                </svg>
              </button>
              
              {/* Mobile Actions Dropdown */}
              {showMobileActions && (
                <div className="mt-2 bg-white dark:bg-gray-800 rounded-lg shadow-lg border border-gray-200 dark:border-gray-700 overflow-hidden">
                  <button
                    onClick={handleToggleStatus}
                    disabled={actionLoading}
                    className={`w-full px-4 py-3 text-left text-sm font-medium transition-colors disabled:opacity-50 flex items-center gap-3 ${
                      brand.activo
                        ? 'hover:bg-yellow-50 text-yellow-800 dark:hover:bg-yellow-900/20 dark:text-yellow-200'
                        : 'hover:bg-green-50 text-green-800 dark:hover:bg-green-900/20 dark:text-green-200'
                    }`}
                  >
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} 
                        d={brand.activo 
                          ? "M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728L5.636 5.636m12.728 12.728L18.364 5.636M5.636 18.364l12.728-12.728"
                          : "M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
                        } 
                      />
                    </svg>
                    {brand.activo ? 'Desactivar' : 'Activar'}
                  </button>
                  
                  <PermissionGate permissions={[PERMISSIONS.BRANDS.EDIT]}>
                    <Link
                      to={`/brands/${brand.id}/edit`}
                      className="w-full px-4 py-3 text-left text-sm font-medium text-blue-800 dark:text-blue-200 hover:bg-blue-50 dark:hover:bg-blue-900/20 transition-colors flex items-center gap-3"
                      onClick={() => setShowMobileActions(false)}
                    >
                      <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                      </svg>
                      Editar
                    </Link>
                  </PermissionGate>
                  
                  <PermissionGate permissions={[PERMISSIONS.BRANDS.DELETE]}>
                    <button
                      onClick={handleDelete}
                      disabled={actionLoading}
                      className="w-full px-4 py-3 text-left text-sm font-medium text-red-800 dark:text-red-200 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors disabled:opacity-50 flex items-center gap-3 border-t border-gray-200 dark:border-gray-700"
                    >
                      <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                      </svg>
                      Eliminar
                    </button>
                  </PermissionGate>
                </div>
              )}
            </div>
          </PermissionGate>
        </div>

        {/* Main Content - Responsive Grid */}
        <div className="grid grid-cols-1 lg:grid-cols-4 gap-4 sm:gap-6">
          {/* Mobile Logo Card - Shown first on mobile */}
          <div className="lg:hidden">
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-4 sm:p-6">
              <h3 className="text-base sm:text-lg font-semibold text-gray-900 dark:text-white mb-4">
                Logo
              </h3>
              
              <div className="text-center">
                {brand.logo ? (
                  <div className="space-y-3">
                    <img 
                      src={brand.logo} 
                      alt={`Logo de ${brand.nombre}`}
                      className="mx-auto h-16 w-16 sm:h-20 sm:w-20 object-contain border border-gray-200 dark:border-gray-600 rounded-lg p-2 bg-gray-50 dark:bg-gray-700"
                      onError={(e) => {
                        const target = e.target as HTMLImageElement;
                        target.style.display = 'none';
                        target.nextElementSibling?.classList.remove('hidden');
                      }}
                    />
                    <div className="hidden">
                      <div className="h-16 w-16 sm:h-20 sm:w-20 bg-gray-200 dark:bg-gray-600 rounded-lg flex items-center justify-center mx-auto">
                        <span className="text-gray-500 dark:text-gray-400 text-lg sm:text-xl font-bold">
                          {brand.nombre.charAt(0).toUpperCase()}
                        </span>
                      </div>
                      <p className="text-xs sm:text-sm text-gray-500 dark:text-gray-400 mt-2">
                        Error al cargar el logo
                      </p>
                    </div>
                    <p className="text-xs text-gray-500 dark:text-gray-400">
                      Logo oficial de {brand.nombre}
                    </p>
                  </div>
                ) : (
                  <div className="space-y-3">
                    <div className="h-16 w-16 sm:h-20 sm:w-20 bg-gray-200 dark:bg-gray-600 rounded-lg flex items-center justify-center mx-auto">
                      <span className="text-gray-500 dark:text-gray-400 text-lg sm:text-xl font-bold">
                        {brand.nombre.charAt(0).toUpperCase()}
                      </span>
                    </div>
                    <p className="text-xs sm:text-sm text-gray-500 dark:text-gray-400">
                      Sin logo configurado
                    </p>
                  </div>
                )}
              </div>
            </div>
          </div>

          {/* Main Content */}
          <div className="lg:col-span-3 space-y-4 sm:space-y-6">
            {/* Basic Info Card */}
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-4 sm:p-6">
              <h2 className="text-base sm:text-lg font-semibold text-gray-900 dark:text-white mb-4">
                Información Básica
              </h2>
              
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label className="block text-xs sm:text-sm font-medium text-gray-500 dark:text-gray-400 mb-1">
                    Nombre
                  </label>
                  <p className="text-sm sm:text-base text-gray-900 dark:text-white font-medium break-words">
                    {brand.nombre}
                  </p>
                </div>
                
                <div>
                  <label className="block text-xs sm:text-sm font-medium text-gray-500 dark:text-gray-400 mb-1">
                    Total de Productos
                  </label>
                  <p className="text-sm sm:text-base text-gray-900 dark:text-white font-medium">
                    <span className="bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200 px-2 py-1 rounded-full text-xs sm:text-sm">
                      {brand.totalProductos}
                    </span>
                  </p>
                </div>
                
                <div>
                  <label className="block text-xs sm:text-sm font-medium text-gray-500 dark:text-gray-400 mb-1">
                    Fecha de Creación
                  </label>
                  <p className="text-xs sm:text-sm text-gray-900 dark:text-white">
                    {new Date(brand.fechaCreacion).toLocaleDateString('es-ES', {
                      year: 'numeric',
                      month: 'long',
                      day: 'numeric'
                    })}
                  </p>
                </div>
                
                <div>
                  <label className="block text-xs sm:text-sm font-medium text-gray-500 dark:text-gray-400 mb-1">
                    Estado
                  </label>
                  <span className={`inline-flex px-2 py-1 rounded-full text-xs font-medium ${
                    brand.activo
                      ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
                      : 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200'
                  }`}>
                    {brand.activo ? 'Activa' : 'Inactiva'}
                  </span>
                </div>
              </div>

              {/* Description */}
              {brand.descripcion && (
                <div className="mt-6">
                  <label className="block text-xs sm:text-sm font-medium text-gray-500 dark:text-gray-400 mb-2">
                    Descripción
                  </label>
                  <p className="text-sm sm:text-base text-gray-900 dark:text-white leading-relaxed">
                    {brand.descripcion}
                  </p>
                </div>
              )}

              {/* Website */}
              {brand.sitioWeb && (
                <div className="mt-4">
                  <label className="block text-xs sm:text-sm font-medium text-gray-500 dark:text-gray-400 mb-2">
                    Sitio Web
                  </label>
                  <a
                    href={brand.sitioWeb}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-blue-600 hover:text-blue-800 dark:text-blue-400 dark:hover:text-blue-300 flex items-center gap-1 text-sm sm:text-base break-all"
                  >
                    {brand.sitioWeb}
                    <svg className="w-3 h-3 sm:w-4 sm:h-4 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14" />
                    </svg>
                  </a>
                </div>
              )}
            </div>

            {/* Products Section */}
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-4 sm:p-6">
              <div className="flex items-center justify-between mb-4">
                <h2 className="text-base sm:text-lg font-semibold text-gray-900 dark:text-white">
                  Productos de la Marca
                </h2>
                <PermissionGate permissions={[PERMISSIONS.PRODUCTS.VIEW]}>
                  <Link
                    to={`/products?brand=${brand.id}`}
                    className="text-blue-600 hover:text-blue-800 dark:text-blue-400 dark:hover:text-blue-300 text-xs sm:text-sm font-medium"
                  >
                    Ver todos →
                  </Link>
                </PermissionGate>
              </div>
              
              <div className="text-center py-6 sm:py-8">
                <div className="text-3xl sm:text-4xl mb-2">📦</div>
                <p className="text-sm sm:text-base text-gray-600 dark:text-gray-400 mb-4">
                  {brand.totalProductos === 0 
                    ? 'Esta marca no tiene productos asociados'
                    : `Esta marca tiene ${brand.totalProductos} producto(s) asociado(s)`
                  }
                </p>
                <PermissionGate permissions={[PERMISSIONS.PRODUCTS.CREATE]}>
                  <Link
                    to={`/products/create?brand=${brand.id}`}
                    className="inline-block bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg text-xs sm:text-sm transition-colors"
                  >
                    Agregar Producto
                  </Link>
                </PermissionGate>
              </div>
            </div>

            {/* Help Card - Mobile */}
            <div className="lg:hidden bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg p-4">
              <h4 className="text-sm font-medium text-blue-800 dark:text-blue-200 mb-2">
                💡 Información
              </h4>
              <div className="text-xs sm:text-sm text-blue-700 dark:text-blue-300 space-y-1">
                <p>
                  • Las marcas con productos solo se pueden desactivar, no eliminar
                </p>
                <p>
                  • Use logos en formato PNG con fondo transparente para mejor resultado
                </p>
                <p>
                  • Mantenga actualizada la información de contacto de la marca
                </p>
              </div>
            </div>
          </div>

          {/* Desktop Sidebar */}
          <div className="hidden lg:block space-y-6">
            {/* Logo Card */}
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
                Logo
              </h3>
              
              <div className="text-center">
                {brand.logo ? (
                  <div className="space-y-3">
                    <img 
                      src={brand.logo} 
                      alt={`Logo de ${brand.nombre}`}
                      className="mx-auto h-24 w-24 object-contain border border-gray-200 dark:border-gray-600 rounded-lg p-2 bg-gray-50 dark:bg-gray-700"
                      onError={(e) => {
                        const target = e.target as HTMLImageElement;
                        target.style.display = 'none';
                        target.nextElementSibling?.classList.remove('hidden');
                      }}
                    />
                    <div className="hidden">
                      <div className="h-24 w-24 bg-gray-200 dark:bg-gray-600 rounded-lg flex items-center justify-center mx-auto">
                        <span className="text-gray-500 dark:text-gray-400 text-xl font-bold">
                          {brand.nombre.charAt(0).toUpperCase()}
                        </span>
                      </div>
                      <p className="text-sm text-gray-500 dark:text-gray-400 mt-2">
                        Error al cargar el logo
                      </p>
                    </div>
                    <p className="text-xs text-gray-500 dark:text-gray-400">
                      Logo oficial de {brand.nombre}
                    </p>
                  </div>
                ) : (
                  <div className="space-y-3">
                    <div className="h-24 w-24 bg-gray-200 dark:bg-gray-600 rounded-lg flex items-center justify-center mx-auto">
                      <span className="text-gray-500 dark:text-gray-400 text-xl font-bold">
                        {brand.nombre.charAt(0).toUpperCase()}
                      </span>
                    </div>
                    <p className="text-sm text-gray-500 dark:text-gray-400">
                      Sin logo configurado
                    </p>
                  </div>
                )}
              </div>
            </div>

            {/* Quick Stats Card */}
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
                Estadísticas Rápidas
              </h3>
              
              <div className="space-y-4">
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600 dark:text-gray-400">
                    Productos Activos
                  </span>
                  <span className="text-lg font-semibold text-green-600 dark:text-green-400">
                    {brand.totalProductos}
                  </span>
                </div>
                
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600 dark:text-gray-400">
                    Estado
                  </span>
                  <span className={`text-sm font-medium ${
                    brand.activo 
                      ? 'text-green-600 dark:text-green-400' 
                      : 'text-red-600 dark:text-red-400'
                  }`}>
                    {brand.activo ? 'Activa' : 'Inactiva'}
                  </span>
                </div>
                
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600 dark:text-gray-400">
                    Creada hace
                  </span>
                  <span className="text-sm font-medium text-gray-900 dark:text-white">
                    {(() => {
                      const days = Math.floor((Date.now() - new Date(brand.fechaCreacion).getTime()) / (1000 * 60 * 60 * 24));
                      if (days < 30) return `${days} días`;
                      if (days < 365) return `${Math.floor(days / 30)} meses`;
                      return `${Math.floor(days / 365)} años`;
                    })()}
                  </span>
                </div>
              </div>
            </div>

            {/* Actions Card */}
            <PermissionGate permissions={[PERMISSIONS.BRANDS.EDIT]}>
              <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
                <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
                  Acciones Rápidas
                </h3>
                
                <div className="space-y-3">
                  <Link
                    to={`/brands/${brand.id}/edit`}
                    className="w-full flex items-center justify-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg transition-colors"
                  >
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                    </svg>
                    Editar Marca
                  </Link>
                  
                  <button
                    onClick={handleToggleStatus}
                    disabled={actionLoading}
                    className={`w-full flex items-center justify-center gap-2 px-4 py-2 rounded-lg transition-colors disabled:opacity-50 ${
                      brand.activo
                        ? 'bg-yellow-600 hover:bg-yellow-700 text-white'
                        : 'bg-green-600 hover:bg-green-700 text-white'
                    }`}
                  >
                    {actionLoading ? (
                      <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                    ) : (
                      <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} 
                          d={brand.activo 
                            ? "M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728L5.636 5.636m12.728 12.728L18.364 5.636M5.636 18.364l12.728-12.728"
                            : "M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
                          } 
                        />
                      </svg>
                    )}
                    {brand.activo ? 'Desactivar' : 'Activar'}
                  </button>
                  
                  <div className="border-t border-gray-200 dark:border-gray-700 pt-3">
                    <button
                      onClick={handleDelete}
                      disabled={actionLoading}
                      className="w-full flex items-center justify-center gap-2 px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg transition-colors disabled:opacity-50"
                    >
                      {actionLoading ? (
                        <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                      ) : (
                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                        </svg>
                      )}
                      Eliminar
                    </button>
                  </div>
                </div>
              </div>
            </PermissionGate>

            {/* Help Card */}
            <div className="bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg p-4">
              <h4 className="text-sm font-medium text-blue-800 dark:text-blue-200 mb-2">
                💡 Información
              </h4>
              <div className="text-sm text-blue-700 dark:text-blue-300 space-y-1">
                <p>
                  • Las marcas con productos solo se pueden desactivar, no eliminar
                </p>
                <p>
                  • Use logos en formato PNG con fondo transparente para mejor resultado
                </p>
                <p>
                  • Mantenga actualizada la información de contacto de la marca
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default BrandDetails;