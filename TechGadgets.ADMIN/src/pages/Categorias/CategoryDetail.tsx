// pages/Categories/CategoryDetails.tsx
import React, { useState, useEffect } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';

import { usePermissions } from '../../hooks/usePermissions';
import PermissionGate from '../../components/auth/PermissionGate';
import { PERMISSIONS } from '../../types/permissions';
import { AxiosError } from 'axios';
import { CategoryBreadcrumbDto, CategoryDto, CategorySummaryDto } from '../../types/categories';
import categoriesService, { ApiResponse } from '../../services/categoryService';


const CategoryDetails: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const [category, setCategory] = useState<CategoryDto | null>(null);
  const [breadcrumb, setBreadcrumb] = useState<CategoryBreadcrumbDto[]>([]);
  const [subcategories, setSubcategories] = useState<CategorySummaryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState(false);
  const [showMobileActions, setShowMobileActions] = useState(false);
  const [error, setError] = useState<string | null>(null);

  usePermissions();

  useEffect(() => {
    if (id) {
      loadCategoryData(parseInt(id));
    }
  }, [id]);

  const loadCategoryData = async (categoryId: number) => {
    try {
      setLoading(true);
      setError(null);

      // Load category data
      const categoryData = await categoriesService.getCategory(categoryId);
      setCategory(categoryData);

      // Load breadcrumb
      const breadcrumbData = await categoriesService.getBreadcrumb(categoryId);
      setBreadcrumb(breadcrumbData);

      // Load subcategories if any
      if (categoryData.totalSubcategorias > 0) {
        try {
          const result = await categoriesService.getCategories({ categoriaPadreId: categoryId });
          setSubcategories(result.items);
        } catch (error) {
          console.warn('Could not load subcategories:', error);
        }
      }

    } catch (error) {
      const axiosError = error as AxiosError<ApiResponse<unknown>>;
      console.error('Error loading category:', axiosError);
      setError(axiosError.response?.data?.message || 'Error al cargar la categoría');
    } finally {
      setLoading(false);
    }
  };

  const handleToggleStatus = async () => {
    if (!category) return;
    
    try {
      setActionLoading(true);
      await categoriesService.toggleCategoryStatus(category.id);
      setCategory(prev => prev ? { ...prev, activo: !prev.activo } : null);
      setShowMobileActions(false);
    } catch (error) {
      const axiosError = error as AxiosError<ApiResponse<unknown>>;
      console.error('Error toggling status:', axiosError);
      alert(axiosError.response?.data?.message || 'Error al cambiar el estado de la categoría');
    } finally {
      setActionLoading(false);
    }
  };

  const handleDelete = async () => {
    if (!category) return;
    
    const confirmed = window.confirm(
      `¿Está seguro de que desea eliminar la categoría "${category.nombre}"?\n\n` +
      `${category.totalProductos > 0 || category.totalSubcategorias > 0
        ? `Esta categoría tiene ${category.totalProductos} producto(s) y ${category.totalSubcategorias} subcategoría(s) asociados y será desactivada en lugar de eliminada.`
        : 'Esta acción no se puede deshacer.'
      }`
    );
    
    if (!confirmed) return;

    try {
      setActionLoading(true);
      await categoriesService.deleteCategory(category.id);
      navigate('/categories');
    } catch (error) {
      const axiosError = error as AxiosError<ApiResponse<unknown>>;
      console.error('Error deleting category:', axiosError);
      alert(axiosError.response?.data?.message || 'Error al eliminar la categoría');
    } finally {
      setActionLoading(false);
      setShowMobileActions(false);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900 p-4 sm:p-6">
        <div className="max-w-7xl mx-auto">
          <div className="flex items-center justify-center h-64">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
            <span className="ml-2 text-gray-600 dark:text-gray-400">Cargando categoría...</span>
          </div>
        </div>
      </div>
    );
  }

  if (error || !category) {
    return (
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900 p-4 sm:p-6">
        <div className="max-w-4xl mx-auto">
          <div className="text-center mt-20">
            <div className="text-6xl mb-4">😕</div>
            <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-2">
              {error || 'Categoría no encontrada'}
            </h2>
            <p className="text-gray-600 dark:text-gray-400 mb-6">
              {error || 'La categoría que busca no existe o ha sido eliminada.'}
            </p>
            <div className="flex flex-col sm:flex-row gap-3 justify-center">
              <Link
                to="/categories"
                className="inline-flex items-center justify-center px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg transition-colors"
              >
                <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                </svg>
                Volver a Categorías
              </Link>
              {error && (
                <button
                  onClick={() => loadCategoryData(parseInt(id!))}
                  className="inline-flex items-center justify-center px-4 py-2 bg-gray-600 hover:bg-gray-700 text-white rounded-lg transition-colors"
                >
                  <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                  </svg>
                  Reintentar
                </button>
              )}
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
      <div className="max-w-7xl mx-auto p-4 sm:p-6 lg:p-8">
        {/* Breadcrumb */}
        {breadcrumb.length > 0 && (
          <div className="mb-4">
            <nav className="flex" aria-label="Breadcrumb">
              <ol className="inline-flex items-center space-x-1 md:space-x-3">
                <li className="inline-flex items-center">
                  <Link
                    to="/categories"
                    className="inline-flex items-center text-sm font-medium text-gray-700 hover:text-blue-600 dark:text-gray-400 dark:hover:text-blue-500"
                  >
                    <svg className="w-3 h-3 mr-2.5" fill="currentColor" viewBox="0 0 20 20">
                      <path d="M10.707 2.293a1 1 0 00-1.414 0l-7 7a1 1 0 001.414 1.414L4 10.414V17a1 1 0 001 1h2a1 1 0 001-1v-2a1 1 0 011-1h2a1 1 0 011 1v2a1 1 0 001 1h2a1 1 0 001-1v-6.586l.293.293a1 1 0 001.414-1.414l-7-7z"></path>
                    </svg>
                    Categorías
                  </Link>
                </li>
                {breadcrumb.map((crumb, index) => (
                  <li key={crumb.id}>
                    <div className="flex items-center">
                      <svg className="w-3 h-3 text-gray-400 mx-1" fill="currentColor" viewBox="0 0 20 20">
                        <path fillRule="evenodd" d="M7.293 14.707a1 1 0 010-1.414L10.586 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z" clipRule="evenodd"></path>
                      </svg>
                      {index === breadcrumb.length - 1 ? (
                        <span className="ml-1 text-sm font-medium text-gray-500 dark:text-gray-400">
                          {crumb.nombre}
                        </span>
                      ) : (
                        <Link
                          to={`/categories/${crumb.id}`}
                          className="ml-1 text-sm font-medium text-gray-700 hover:text-blue-600 dark:text-gray-400 dark:hover:text-blue-500"
                        >
                          {crumb.nombre}
                        </Link>
                      )}
                    </div>
                  </li>
                ))}
              </ol>
            </nav>
          </div>
        )}

        {/* Header */}
        <div className="mb-6">
          <div className="flex items-start gap-4 mb-4">
            <button
              onClick={() => navigate('/categories')}
              className="mt-1 text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200 transition-colors"
            >
              <svg className="w-5 h-5 sm:w-6 sm:h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
              </svg>
            </button>
            
            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-3 mb-2">
                {category.icono && (
                  <div className="w-12 h-12 sm:w-16 sm:h-16 bg-gray-100 dark:bg-gray-700 rounded-lg flex items-center justify-center">
                    <span className="text-2xl sm:text-3xl">{category.icono}</span>
                  </div>
                )}
                <div>
                  <h1 className="text-2xl sm:text-3xl lg:text-4xl font-bold text-gray-900 dark:text-white break-words">
                    {category.nombre}
                  </h1>
                  <div className="flex flex-wrap items-center gap-2 mt-2">
                    <span className={`px-2 py-1 rounded-full text-xs font-medium ${
                      category.activo
                        ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
                        : 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200'
                    }`}>
                      {category.activo ? 'Activa' : 'Inactiva'}
                    </span>
                    <span className="text-xs sm:text-sm text-gray-500 dark:text-gray-400">
                      ID: {category.id}
                    </span>
                    {category.slug && (
                      <span className="text-xs sm:text-sm text-blue-600 dark:text-blue-400">
                        /{category.slug}
                      </span>
                    )}
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Desktop Actions */}
          <PermissionGate permissions={[PERMISSIONS.CATEGORIES.EDIT, PERMISSIONS.CATEGORIES.DELETE]}>
            <div className="hidden sm:flex flex-wrap gap-2">
              <PermissionGate permissions={[PERMISSIONS.CATEGORIES.EDIT]}>
                <button
                  onClick={handleToggleStatus}
                  disabled={actionLoading}
                  className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors disabled:opacity-50 ${
                    category.activo
                      ? 'bg-yellow-100 hover:bg-yellow-200 text-yellow-800 dark:bg-yellow-900 dark:hover:bg-yellow-800 dark:text-yellow-200'
                      : 'bg-green-100 hover:bg-green-200 text-green-800 dark:bg-green-900 dark:hover:bg-green-800 dark:text-green-200'
                  }`}
                >
                  {category.activo ? 'Desactivar' : 'Activar'}
                </button>
                
                <Link
                  to={`/categories/${category.id}/edit`}
                  className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-medium transition-colors"
                >
                  Editar
                </Link>
              </PermissionGate>
              
              <PermissionGate permissions={[PERMISSIONS.CATEGORIES.DELETE]}>
                <button
                  onClick={handleDelete}
                  disabled={actionLoading}
                  className="px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg text-sm font-medium transition-colors disabled:opacity-50"
                >
                  Eliminar
                </button>
              </PermissionGate>
            </div>
          </PermissionGate>

          {/* Mobile Actions Button */}
          <PermissionGate permissions={[PERMISSIONS.CATEGORIES.EDIT, PERMISSIONS.CATEGORIES.DELETE]}>
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
                  <PermissionGate permissions={[PERMISSIONS.CATEGORIES.EDIT]}>
                    <button
                      onClick={handleToggleStatus}
                      disabled={actionLoading}
                      className={`w-full px-4 py-3 text-left text-sm font-medium transition-colors disabled:opacity-50 flex items-center gap-3 ${
                        category.activo
                          ? 'hover:bg-yellow-50 text-yellow-800 dark:hover:bg-yellow-900/20 dark:text-yellow-200'
                          : 'hover:bg-green-50 text-green-800 dark:hover:bg-green-900/20 dark:text-green-200'
                      }`}
                    >
                      <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} 
                          d={category.activo 
                            ? "M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728L5.636 5.636m12.728 12.728L18.364 5.636M5.636 18.364l12.728-12.728"
                            : "M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
                          } 
                        />
                      </svg>
                      {category.activo ? 'Desactivar' : 'Activar'}
                    </button>
                    
                    <Link
                      to={`/categories/${category.id}/edit`}
                      className="w-full px-4 py-3 text-left text-sm font-medium text-blue-800 dark:text-blue-200 hover:bg-blue-50 dark:hover:bg-blue-900/20 transition-colors flex items-center gap-3"
                      onClick={() => setShowMobileActions(false)}
                    >
                      <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                      </svg>
                      Editar
                    </Link>
                  </PermissionGate>
                  
                  <PermissionGate permissions={[PERMISSIONS.CATEGORIES.DELETE]}>
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

        {/* Main Content */}
        <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
          {/* Main Information */}
          <div className="lg:col-span-3 space-y-6">
            {/* Basic Information Card */}
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
              <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-6">
                Información General
              </h2>
              
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
                <div>
                  <label className="block text-sm font-medium text-gray-500 dark:text-gray-400 mb-2">
                    Nombre
                  </label>
                  <p className="text-base font-medium text-gray-900 dark:text-white">
                    {category.nombre}
                  </p>
                </div>
                
                <div>
                  <label className="block text-sm font-medium text-gray-500 dark:text-gray-400 mb-2">
                    Estado
                  </label>
                  <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-sm font-medium ${
                    category.activo
                      ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
                      : 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200'
                  }`}>
                    {category.activo ? 'Activa' : 'Inactiva'}
                  </span>
                </div>
                
                <div>
                  <label className="block text-sm font-medium text-gray-500 dark:text-gray-400 mb-2">
                    Orden
                  </label>
                  <p className="text-base text-gray-900 dark:text-white">
                    {category.orden || 0}
                  </p>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-500 dark:text-gray-400 mb-2">
                    Total de Productos
                  </label>
                  <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-sm font-medium bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200">
                    {category.totalProductos || 0}
                  </span>
                </div>
                
                <div>
                  <label className="block text-sm font-medium text-gray-500 dark:text-gray-400 mb-2">
                    Subcategorías
                  </label>
                  <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-sm font-medium bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200">
                    {category.totalSubcategorias || 0}
                  </span>
                </div>
                
                <div>
                  <label className="block text-sm font-medium text-gray-500 dark:text-gray-400 mb-2">
                    Fecha de Creación
                  </label>
                  <p className="text-sm text-gray-900 dark:text-white">
                    {new Date(category.fechaCreacion).toLocaleDateString('es-ES', {
                      year: 'numeric',
                      month: 'long',
                      day: 'numeric',
                      hour: '2-digit',
                      minute: '2-digit'
                    })}
                  </p>
                </div>
              </div>

              {/* Description */}
              {category.descripcion && (
                <div className="mt-6 pt-6 border-t border-gray-200 dark:border-gray-700">
                  <label className="block text-sm font-medium text-gray-500 dark:text-gray-400 mb-2">
                    Descripción
                  </label>
                  <p className="text-base text-gray-900 dark:text-white leading-relaxed">
                    {category.descripcion}
                  </p>
                </div>
              )}

              {/* Hierarchy */}
              {category.categoriaPadreNombre && (
                <div className="mt-6 pt-6 border-t border-gray-200 dark:border-gray-700">
                  <label className="block text-sm font-medium text-gray-500 dark:text-gray-400 mb-2">
                    Categoría Padre
                  </label>
                  <p className="text-base text-blue-600 dark:text-blue-400 font-medium">
                    {category.categoriaPadreNombre}
                  </p>
                </div>
              )}
            </div>

            {/* SEO Information */}
            {(category.slug || category.metaTitle || category.metaDescription || category.keywords) && (
              <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
                <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-6">
                  Información SEO
                </h2>
                
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-6">
                  {category.slug && (
                    <div>
                      <label className="block text-sm font-medium text-gray-500 dark:text-gray-400 mb-2">
                        Slug (URL)
                      </label>
                      <p className="text-base font-mono text-gray-900 dark:text-white bg-gray-50 dark:bg-gray-700 px-3 py-2 rounded-lg">
                        /{category.slug}
                      </p>
                    </div>
                  )}
                  
                  {category.metaTitle && (
                    <div>
                      <label className="block text-sm font-medium text-gray-500 dark:text-gray-400 mb-2">
                        Meta Título
                      </label>
                      <p className="text-base text-gray-900 dark:text-white">
                        {category.metaTitle}
                      </p>
                    </div>
                  )}
                </div>

                {category.metaDescription && (
                  <div className="mt-4">
                    <label className="block text-sm font-medium text-gray-500 dark:text-gray-400 mb-2">
                      Meta Descripción
                    </label>
                    <p className="text-base text-gray-900 dark:text-white">
                      {category.metaDescription}
                    </p>
                  </div>
                )}

                {category.keywords && (
                  <div className="mt-4">
                    <label className="block text-sm font-medium text-gray-500 dark:text-gray-400 mb-2">
                      Palabras Clave
                    </label>
                    <div className="flex flex-wrap gap-2">
                      {category.keywords.split(',').map((keyword, index) => (
                        <span 
                          key={index}
                          className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-200"
                        >
                          #{keyword.trim()}
                        </span>
                      ))}
                    </div>
                  </div>
                )}

                {/* SEO Preview */}
                <div className="mt-6 pt-6 border-t border-gray-200 dark:border-gray-700">
                  <label className="block text-sm font-medium text-gray-500 dark:text-gray-400 mb-3">
                    Vista previa en buscadores
                  </label>
                  <div className="bg-gray-50 dark:bg-gray-700 rounded-lg p-4">
                    <h3 className="text-blue-600 dark:text-blue-400 text-lg font-medium hover:underline cursor-pointer mb-1">
                      {category.metaTitle || category.nombre}
                    </h3>
                    <p className="text-green-700 dark:text-green-400 text-sm mb-2">
                      https://ejemplo.com/categorias/{category.slug || 'categoria'}
                    </p>
                    <p className="text-gray-600 dark:text-gray-300 text-sm">
                      {category.metaDescription || category.descripcion || 'Descripción de la categoría...'}
                    </p>
                  </div>
                </div>
              </div>
            )}

            {/* Products Section */}
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
              <div className="flex items-center justify-between mb-6">
                <h2 className="text-lg font-semibold text-gray-900 dark:text-white">
                  Productos de la Categoría
                </h2>
                <PermissionGate permissions={[PERMISSIONS.PRODUCTS.VIEW]}>
                  <Link
                    to={`/products?category=${category.id}`}
                    className="text-blue-600 hover:text-blue-800 dark:text-blue-400 dark:hover:text-blue-300 text-sm font-medium flex items-center gap-1"
                  >
                    Ver todos
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                    </svg>
                  </Link>
                </PermissionGate>
              </div>
              
              <div className="text-center py-8">
                <div className="text-4xl mb-4">📦</div>
                <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-2">
                  {category.totalProductos === 0 
                    ? 'No hay productos en esta categoría'
                    : `${category.totalProductos} producto(s) en esta categoría`
                  }
                </h3>
                <p className="text-gray-600 dark:text-gray-400 mb-6">
                  {category.totalProductos === 0 
                    ? 'Aún no se han agregado productos a esta categoría.'
                    : 'Gestiona los productos asociados a esta categoría.'
                  }
                </p>
                <div className="flex flex-col sm:flex-row gap-3 justify-center">
                  <PermissionGate permissions={[PERMISSIONS.PRODUCTS.CREATE]}>
                    <Link
                      to={`/products/create?category=${category.id}`}
                      className="inline-flex items-center justify-center px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-medium transition-colors"
                    >
                      <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                      </svg>
                      Agregar Producto
                    </Link>
                  </PermissionGate>
                  {category.totalProductos > 0 && (
                    <PermissionGate permissions={[PERMISSIONS.PRODUCTS.VIEW]}>
                      <Link
                        to={`/products?category=${category.id}`}
                        className="inline-flex items-center justify-center px-4 py-2 bg-gray-600 hover:bg-gray-700 text-white rounded-lg text-sm font-medium transition-colors"
                      >
                        <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                        </svg>
                        Ver Productos
                      </Link>
                    </PermissionGate>
                  )}
                </div>
              </div>
            </div>

            {/* Subcategories Section */}
            {subcategories.length > 0 && (
              <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6 mb-6">
                <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
                  Subcategorías ({subcategories.length})
                </h2>
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                  {subcategories.map((subcat) => (
                    <Link
                      key={subcat.id}
                      to={`/categories/${subcat.id}`}
                      className="block p-4 bg-gray-50 dark:bg-gray-700 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-600 transition-colors"
                    >
                      <div className="flex items-center gap-3">
                        {subcat.icono && (
                          <div className="w-10 h-10 bg-white dark:bg-gray-600 rounded-lg flex items-center justify-center">
                            <span className="text-xl">{subcat.icono}</span>
                          </div>
                        )}
                        <div className="flex-1 min-w-0">
                          <h3 className="font-medium text-gray-900 dark:text-white truncate">
                            {subcat.nombre}
                          </h3>
                          <div className="flex items-center gap-2 mt-1">
                            <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${
                              subcat.activo
                                ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
                                : 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200'
                            }`}>
                              {subcat.activo ? 'Activa' : 'Inactiva'}
                            </span>
                            <span className="text-xs text-gray-500 dark:text-gray-400">
                              /{subcat.slug}
                            </span>
                          </div>
                        </div>
                      </div>
                    </Link>
                  ))}
                </div>
              </div>
            )}
          </div>

          {/* Sidebar */}
          <div className="space-y-6">
            {/* Icon Card */}
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
                Icono
              </h3>
              
              <div className="text-center">
                {category.icono ? (
                  <div className="space-y-3">
                    <div className="w-20 h-20 bg-gray-100 dark:bg-gray-700 rounded-lg flex items-center justify-center mx-auto">
                      <span className="text-3xl">{category.icono}</span>
                    </div>
                    <p className="text-sm text-gray-600 dark:text-gray-400">
                      Icono de la categoría
                    </p>
                  </div>
                ) : (
                  <div className="space-y-3">
                    <div className="w-20 h-20 bg-gray-200 dark:bg-gray-600 rounded-lg flex items-center justify-center mx-auto">
                      <svg className="w-8 h-8 text-gray-500 dark:text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                      </svg>
                    </div>
                    <p className="text-sm text-gray-500 dark:text-gray-400">
                      Sin icono configurado
                    </p>
                  </div>
                )}
              </div>
            </div>

            {/* Quick Stats Card */}
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
                Estadísticas
              </h3>
              
              <div className="space-y-4">
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600 dark:text-gray-400">
                    Productos
                  </span>
                  <span className="text-lg font-semibold text-blue-600 dark:text-blue-400">
                    {category.totalProductos || 0}
                  </span>
                </div>
                
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600 dark:text-gray-400">
                    Subcategorías
                  </span>
                  <span className="text-lg font-semibold text-purple-600 dark:text-purple-400">
                    {category.totalSubcategorias || 0}
                  </span>
                </div>
                
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600 dark:text-gray-400">
                    Estado
                  </span>
                  <span className={`text-sm font-medium ${
                    category.activo 
                      ? 'text-green-600 dark:text-green-400' 
                      : 'text-red-600 dark:text-red-400'
                  }`}>
                    {category.activo ? 'Activa' : 'Inactiva'}
                  </span>
                </div>
                
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600 dark:text-gray-400">
                    Nivel
                  </span>
                  <span className="text-sm font-medium text-gray-900 dark:text-white">
                    {category.categoriaPadreId ? 'Subcategoría' : 'Principal'}
                  </span>
                </div>
                
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600 dark:text-gray-400">
                    Creada hace
                  </span>
                  <span className="text-sm font-medium text-gray-900 dark:text-white">
                    {(() => {
                      const days = Math.floor((Date.now() - new Date(category.fechaCreacion).getTime()) / (1000 * 60 * 60 * 24));
                      if (days < 30) return `${days} días`;
                      if (days < 365) return `${Math.floor(days / 30)} meses`;
                      return `${Math.floor(days / 365)} años`;
                    })()}
                  </span>
                </div>
              </div>
            </div>

            {/* Actions Card */}
            <PermissionGate permissions={[PERMISSIONS.CATEGORIES.EDIT]}>
              <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
                <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
                  Acciones Rápidas
                </h3>
                
                <div className="space-y-3">
                  <Link
                    to={`/categories/${category.id}/edit`}
                    className="w-full flex items-center justify-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg transition-colors"
                  >
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                    </svg>
                    Editar Categoría
                  </Link>
                  
                  <button
                    onClick={handleToggleStatus}
                    disabled={actionLoading}
                    className={`w-full flex items-center justify-center gap-2 px-4 py-2 rounded-lg transition-colors disabled:opacity-50 ${
                      category.activo
                        ? 'bg-yellow-600 hover:bg-yellow-700 text-white'
                        : 'bg-green-600 hover:bg-green-700 text-white'
                    }`}
                  >
                    {actionLoading ? (
                      <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                    ) : (
                      <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} 
                          d={category.activo 
                            ? "M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728L5.636 5.636m12.728 12.728L18.364 5.636M5.636 18.364l12.728-12.728"
                            : "M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
                          } 
                        />
                      </svg>
                    )}
                    {category.activo ? 'Desactivar' : 'Activar'}
                  </button>
                  
                  <PermissionGate permissions={[PERMISSIONS.CATEGORIES.CREATE]}>
                    <Link
                      to={`/categories/create?parent=${category.id}`}
                      className="w-full flex items-center justify-center gap-2 px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white rounded-lg transition-colors"
                    >
                      <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                      </svg>
                      Nueva Subcategoría
                    </Link>
                  </PermissionGate>
                  
                  <div className="border-t border-gray-200 dark:border-gray-700 pt-3">
                    <PermissionGate permissions={[PERMISSIONS.CATEGORIES.DELETE]}>
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
                    </PermissionGate>
                  </div>
                </div>
              </div>
            </PermissionGate>

            {/* Related Actions Card */}
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
                Acciones Relacionadas
              </h3>
              
              <div className="space-y-3">
                <PermissionGate permissions={[PERMISSIONS.PRODUCTS.VIEW]}>
                  <Link
                    to={`/products?category=${category.id}`}
                    className="w-full flex items-center justify-center gap-2 px-4 py-2 bg-gray-100 hover:bg-gray-200 dark:bg-gray-700 dark:hover:bg-gray-600 text-gray-700 dark:text-gray-300 rounded-lg transition-colors"
                  >
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
                    </svg>
                    Ver Productos
                  </Link>
                </PermissionGate>
                
                <Link
                  to={`/categories?parent=${category.id}`}
                  className="w-full flex items-center justify-center gap-2 px-4 py-2 bg-gray-100 hover:bg-gray-200 dark:bg-gray-700 dark:hover:bg-gray-600 text-gray-700 dark:text-gray-300 rounded-lg transition-colors"
                >
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
                  </svg>
                  Ver Subcategorías
                </Link>
                
                <Link
                  to="/categories"
                  className="w-full flex items-center justify-center gap-2 px-4 py-2 bg-gray-100 hover:bg-gray-200 dark:bg-gray-700 dark:hover:bg-gray-600 text-gray-700 dark:text-gray-300 rounded-lg transition-colors"
                >
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 10h16M4 14h16M4 18h16" />
                  </svg>
                  Todas las Categorías
                </Link>
              </div>
            </div>

            {/* Help Card */}
            <div className="bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg p-4">
              <h4 className="text-sm font-medium text-blue-800 dark:text-blue-200 mb-2">
                💡 Información
              </h4>
              <div className="text-sm text-blue-700 dark:text-blue-300 space-y-1">
                <p>
                  • Las categorías con productos/subcategorías solo se pueden desactivar
                </p>
                <p>
                  • Use iconos para facilitar la identificación visual
                </p>
                <p>
                  • Configure SEO para mejorar el posicionamiento
                </p>
                <p>
                  • El orden determina la posición en el sitio web
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default CategoryDetails;