// pages/Categories/CategoryForm.tsx
import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { CreateCategoryDto, UpdateCategoryDto, CategorySummaryDto } from '../../types/categories';
import { usePermissions } from '../../hooks/usePermissions';
import PermissionGate from '../../components/auth/PermissionGate';
import { PERMISSIONS } from '../../types/permissions';
import { AxiosError } from 'axios';
import categoriesService, { ApiResponse } from '../../services/categoryService';

const CategoryForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEditing = Boolean(id);

  const [loading, setLoading] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [loadingParents, setLoadingParents] = useState(false);
  const [parentCategories, setParentCategories] = useState<CategorySummaryDto[]>([]);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [showIconPicker, setShowIconPicker] = useState(false);

  // Form data
  const [formData, setFormData] = useState({
    nombre: '',
    descripcion: '',
    icono: '',
    activo: true,
    categoriaPadreId: null as number | null,
    orden: 0,
    slug: '',
    metaTitle: '',
    metaDescription: '',
    keywords: ''
  });

  const commonIcons = [
    '📱', '💻', '🖥️', '⌚', '🎧', '📷', '🎮', '📺', '🏠', '🔧',
    '🛠️', '🔌', '⌨️', '🖱️', '💾', '⚽', '🌐', '🎾', '🔋', '📞',
    '🎙️', '🖼️', '🎨', '🎭', '🎪', '🎵', '🎸', '🎹', '🎤', '🍔',
    '🍕', '🍜', '☕', '🍷', '🎂', '🌟', '⭐', '💎', '🔥', '⚡',
    '🌈', '🌸', '🌿', '🌊', '🏔️', '☀️', '🌙', '❄️', '🎄', '🎁'
  ];

  usePermissions();

  // Load form data
  useEffect(() => {
    const loadData = async () => {
      try {
        setLoading(true);
        
        // Load parent categories
        setLoadingParents(true);
        const parents = await categoriesService.getRootCategories();
        setParentCategories(parents);
        setLoadingParents(false);

        // Load category data if editing
        if (isEditing && id) {
          const category = await categoriesService.getCategory(parseInt(id));
          setFormData({
            nombre: category.nombre,
            descripcion: category.descripcion || '',
            icono: category.icono || '',
            activo: category.activo,
            categoriaPadreId: category.categoriaPadreId || null,
            orden: category.orden || 0,
            slug: category.slug || '',
            metaTitle: category.metaTitle || '',
            metaDescription: category.metaDescription || '',
            keywords: category.keywords || ''
          });
        }
      } catch (error) {
        const axiosError = error as AxiosError<ApiResponse<unknown>>;
        console.error('Error loading data:', axiosError);
        setErrors({ general: axiosError.response?.data?.message || 'Error al cargar los datos' });
      } finally {
        setLoading(false);
      }
    };

    loadData();
  }, [isEditing, id]);

  // Generate slug from name
  const generateSlug = (name: string) => {
    return name
      .toLowerCase()
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '') // Remove accents
      .replace(/[^a-z0-9\s-]/g, '') // Remove special characters
      .replace(/\s+/g, '-') // Replace spaces with hyphens
      .replace(/-+/g, '-') // Replace multiple hyphens with single
      .trim();
  };

  // Handle input changes
  const handleInputChange = (field: keyof typeof formData, value: string | number | boolean | null) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    
    // Auto-generate slug from name
    if (field === 'nombre' && typeof value === 'string') {
      const slug = generateSlug(value);
      setFormData(prev => ({ ...prev, slug }));
    }
    
    // Clear field error
    if (errors[field]) {
      setErrors(prev => ({ ...prev, [field]: '' }));
    }
  };

  // Validate form
  const validateForm = () => {
    const newErrors: Record<string, string> = {};

    if (!formData.nombre.trim()) {
      newErrors.nombre = 'El nombre es requerido';
    } else if (formData.nombre.length < 2) {
      newErrors.nombre = 'El nombre debe tener al menos 2 caracteres';
    } else if (formData.nombre.length > 100) {
      newErrors.nombre = 'El nombre no puede exceder 100 caracteres';
    }

    if (formData.descripcion && formData.descripcion.length > 500) {
      newErrors.descripcion = 'La descripción no puede exceder 500 caracteres';
    }

    if (!formData.slug.trim()) {
      newErrors.slug = 'El slug es requerido';
    } else if (!/^[a-z0-9-]+$/.test(formData.slug)) {
      newErrors.slug = 'El slug solo puede contener letras minúsculas, números y guiones';
    }

    if (formData.metaTitle && formData.metaTitle.length > 60) {
      newErrors.metaTitle = 'El meta título no puede exceder 60 caracteres';
    }

    if (formData.metaDescription && formData.metaDescription.length > 160) {
      newErrors.metaDescription = 'La meta descripción no puede exceder 160 caracteres';
    }

    if (formData.orden < 0) {
      newErrors.orden = 'El orden no puede ser negativo';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  // Handle submit
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateForm()) return;

    try {
      setSubmitting(true);
      setErrors({});

      if (isEditing && id) {
        const updateData: UpdateCategoryDto = {
          nombre: formData.nombre.trim(),
          descripcion: formData.descripcion.trim() || undefined,
          icono: formData.icono || undefined,
          activo: formData.activo,
          categoriaPadreId: formData.categoriaPadreId,
          orden: formData.orden,
          slug: formData.slug.trim()
        };

        await categoriesService.updateCategory(parseInt(id), updateData);
        navigate(`/categories/${id}`);
      } else {
        const createData: CreateCategoryDto = {
          nombre: formData.nombre.trim(),
          descripcion: formData.descripcion.trim() || undefined,
          icono: formData.icono || undefined,
          categoriaPadreId: formData.categoriaPadreId,
          orden: formData.orden,
          slug: formData.slug.trim()
        };

        const newCategory = await categoriesService.createCategory(createData);
        navigate(`/categories/${newCategory.id}`);
      }
    } catch (error) {
      const axiosError = error as AxiosError<ApiResponse<unknown>>;
      console.error('Error saving category:', axiosError);
      
      if (axiosError.response?.data?.errors) {
        const errorMessages: Record<string, string> = {};
        Object.entries(axiosError.response.data.errors).forEach(([key, messages]) => {
          errorMessages[key] = messages[0];
        });
        setErrors(errorMessages);
      } else {
        setErrors({ 
          general: axiosError.response?.data?.message || 'Error al guardar la categoría' 
        });
      }
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900 p-4 sm:p-6">
        <div className="max-w-3xl mx-auto">
          <div className="flex items-center justify-center h-64">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
            <span className="ml-2 text-gray-600 dark:text-gray-400">
              Cargando {isEditing ? 'categoría' : 'formulario'}...
            </span>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
      <div className="max-w-4xl mx-auto p-4 sm:p-6 lg:p-8">
        {/* Header */}
        <div className="mb-6">
          <div className="flex items-center gap-4 mb-4">
            <button
              onClick={() => navigate('/categories')}
              className="text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200 transition-colors"
            >
              <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
              </svg>
            </button>
            <h1 className="text-2xl sm:text-3xl font-bold text-gray-900 dark:text-white">
              {isEditing ? 'Editar Categoría' : 'Nueva Categoría'}
            </h1>
          </div>
          <p className="text-sm text-gray-600 dark:text-gray-400">
            {isEditing 
              ? 'Modifica la información de la categoría existente'
              : 'Completa la información para crear una nueva categoría'
            }
          </p>
        </div>

        {/* Error Alert */}
        {errors.general && (
          <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4 mb-6">
            <div className="flex items-center gap-2">
              <svg className="w-5 h-5 text-red-600 dark:text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <p className="text-sm text-red-800 dark:text-red-200">{errors.general}</p>
            </div>
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Main Information */}
            <div className="lg:col-span-2 space-y-6">
              {/* Basic Information Card */}
              <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
                <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
                  Información Básica
                </h2>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  {/* Name */}
                  <div className="sm:col-span-2">
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                      Nombre <span className="text-red-500">*</span>
                    </label>
                    <input
                      type="text"
                      value={formData.nombre}
                      onChange={(e) => handleInputChange('nombre', e.target.value)}
                      className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white text-sm ${
                        errors.nombre 
                          ? 'border-red-300 dark:border-red-600' 
                          : 'border-gray-300 dark:border-gray-600'
                      }`}
                      placeholder="Ej: Smartphones, Laptops, Ropa..."
                      maxLength={100}
                    />
                    {errors.nombre && (
                      <p className="text-red-600 dark:text-red-400 text-xs mt-1">{errors.nombre}</p>
                    )}
                    <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                      {formData.nombre.length}/100 caracteres
                    </p>
                  </div>

                  {/* Parent Category */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                      Categoría Padre
                    </label>
                    <select
                      value={formData.categoriaPadreId || ''}
                      onChange={(e) => handleInputChange('categoriaPadreId', e.target.value ? parseInt(e.target.value) : null)}
                      disabled={loadingParents}
                      className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white text-sm"
                    >
                      <option value="">Sin categoría padre (Raíz)</option>
                      {parentCategories.map((parent) => (
                        <option key={parent.id} value={parent.id}>
                          {parent.nombre}
                        </option>
                      ))}
                    </select>
                    {loadingParents && (
                      <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">Cargando categorías...</p>
                    )}
                  </div>

                  {/* Order */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                      Orden
                    </label>
                    <input
                      type="number"
                      value={formData.orden}
                      onChange={(e) => handleInputChange('orden', parseInt(e.target.value) || 0)}
                      className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white text-sm ${
                        errors.orden 
                          ? 'border-red-300 dark:border-red-600' 
                          : 'border-gray-300 dark:border-gray-600'
                      }`}
                      min="0"
                      placeholder="0"
                    />
                    {errors.orden && (
                      <p className="text-red-600 dark:text-red-400 text-xs mt-1">{errors.orden}</p>
                    )}
                    <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                      Orden de aparición (menor número = mayor prioridad)
                    </p>
                  </div>
                </div>

                {/* Description */}
                <div className="mt-4">
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                    Descripción
                  </label>
                  <textarea
                    value={formData.descripcion}
                    onChange={(e) => handleInputChange('descripcion', e.target.value)}
                    rows={3}
                    className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white text-sm ${
                      errors.descripcion 
                        ? 'border-red-300 dark:border-red-600' 
                        : 'border-gray-300 dark:border-gray-600'
                    }`}
                    placeholder="Descripción opcional de la categoría..."
                    maxLength={500}
                  />
                  {errors.descripcion && (
                    <p className="text-red-600 dark:text-red-400 text-xs mt-1">{errors.descripcion}</p>
                  )}
                  <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                    {formData.descripcion.length}/500 caracteres
                  </p>
                </div>

                {/* Slug */}
                <div className="mt-4">
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                    Slug (URL) <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="text"
                    value={formData.slug}
                    onChange={(e) => handleInputChange('slug', e.target.value.toLowerCase())}
                    className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white text-sm ${
                      errors.slug 
                        ? 'border-red-300 dark:border-red-600' 
                        : 'border-gray-300 dark:border-gray-600'
                    }`}
                    placeholder="ej: smartphones, laptops-gaming"
                    pattern="[a-z0-9-]+"
                  />
                  {errors.slug && (
                    <p className="text-red-600 dark:text-red-400 text-xs mt-1">{errors.slug}</p>
                  )}
                  <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                    URL amigable (solo letras minúsculas, números y guiones)
                  </p>
                </div>
              </div>

              {/* SEO Information Card */}
              <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
                <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
                  SEO (Opcional)
                </h2>

                <div className="space-y-4">
                  {/* Meta Title */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                      Meta Título
                    </label>
                    <input
                      type="text"
                      value={formData.metaTitle}
                      onChange={(e) => handleInputChange('metaTitle', e.target.value)}
                      className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white text-sm ${
                        errors.metaTitle 
                          ? 'border-red-300 dark:border-red-600' 
                          : 'border-gray-300 dark:border-gray-600'
                      }`}
                      placeholder="Título para SEO..."
                      maxLength={60}
                    />
                    {errors.metaTitle && (
                      <p className="text-red-600 dark:text-red-400 text-xs mt-1">{errors.metaTitle}</p>
                    )}
                    <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                      {formData.metaTitle.length}/60 caracteres (recomendado para Google)
                    </p>
                  </div>

                  {/* Meta Description */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                      Meta Descripción
                    </label>
                    <textarea
                      value={formData.metaDescription}
                      onChange={(e) => handleInputChange('metaDescription', e.target.value)}
                      rows={2}
                      className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white text-sm ${
                        errors.metaDescription 
                          ? 'border-red-300 dark:border-red-600' 
                          : 'border-gray-300 dark:border-gray-600'
                      }`}
                      placeholder="Descripción para SEO..."
                      maxLength={160}
                    />
                    {errors.metaDescription && (
                      <p className="text-red-600 dark:text-red-400 text-xs mt-1">{errors.metaDescription}</p>
                    )}
                    <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                      {formData.metaDescription.length}/160 caracteres (recomendado para Google)
                    </p>
                  </div>

                  {/* Keywords */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                      Palabras Clave
                    </label>
                    <input
                      type="text"
                      value={formData.keywords}
                      onChange={(e) => handleInputChange('keywords', e.target.value)}
                      className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white text-sm"
                      placeholder="palabra1, palabra2, palabra3..."
                    />
                    <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                      Separadas por comas
                    </p>
                  </div>
                </div>
              </div>
            </div>

            {/* Sidebar */}
            <div className="space-y-6">
              {/* Icon Selector Card */}
              <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
                <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
                  Icono
                </h3>

                <div className="space-y-4">
                  {/* Current Icon */}
                  <div className="text-center">
                    <div className="w-16 h-16 bg-gray-100 dark:bg-gray-700 rounded-lg flex items-center justify-center mx-auto mb-2">
                      {formData.icono ? (
                        <span className="text-2xl">{formData.icono}</span>
                      ) : (
                        <svg className="w-8 h-8 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                        </svg>
                      )}
                    </div>
                    <p className="text-sm text-gray-600 dark:text-gray-400">
                      {formData.icono ? 'Icono seleccionado' : 'Sin icono'}
                    </p>
                  </div>

                  {/* Custom Icon Input */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                      Icono personalizado
                    </label>
                    <input
                      type="text"
                      value={formData.icono}
                      onChange={(e) => handleInputChange('icono', e.target.value)}
                      className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white text-sm"
                      placeholder="Pega un emoji aquí..."
                    />
                  </div>

                  {/* Icon Picker */}
                  <div>
                    <button
                      type="button"
                      onClick={() => setShowIconPicker(!showIconPicker)}
                      className="w-full px-3 py-2 bg-gray-100 hover:bg-gray-200 dark:bg-gray-700 dark:hover:bg-gray-600 text-gray-700 dark:text-gray-300 rounded-lg text-sm font-medium transition-colors"
                    >
                      {showIconPicker ? 'Ocultar' : 'Mostrar'} Iconos Comunes
                    </button>

                    {showIconPicker && (
                      <div className="mt-3 grid grid-cols-5 gap-2 max-h-48 overflow-y-auto">
                        {commonIcons.map((icon, index) => (
                          <button
                            key={index}
                            type="button"
                            onClick={() => handleInputChange('icono', icon)}
                            className={`w-10 h-10 flex items-center justify-center rounded-lg text-lg transition-colors ${
                              formData.icono === icon
                                ? 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200'
                                : 'bg-gray-100 hover:bg-gray-200 dark:bg-gray-700 dark:hover:bg-gray-600'
                            }`}
                          >
                            {icon}
                          </button>
                        ))}
                      </div>
                    )}
                  </div>

                  {/* Clear Icon */}
                  {formData.icono && (
                    <button
                      type="button"
                      onClick={() => handleInputChange('icono', '')}
                      className="w-full px-3 py-2 bg-red-100 hover:bg-red-200 dark:bg-red-900 dark:hover:bg-red-800 text-red-800 dark:text-red-200 rounded-lg text-sm font-medium transition-colors"
                    >
                      Quitar Icono
                    </button>
                  )}
                </div>
              </div>

              {/* Status Card */}
              <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
                <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
                  Estado
                </h3>

                <div className="space-y-4">
                  <div className="flex items-center justify-between">
                    <div>
                      <label className="text-sm font-medium text-gray-700 dark:text-gray-300">
                        Categoría Activa
                      </label>
                      <p className="text-xs text-gray-500 dark:text-gray-400">
                        Las categorías inactivas no aparecen en el sitio web
                      </p>
                    </div>
                    <label className="relative inline-flex items-center cursor-pointer">
                      <input
                        type="checkbox"
                        checked={formData.activo}
                        onChange={(e) => handleInputChange('activo', e.target.checked)}
                        className="sr-only peer"
                      />
                      <div className="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-blue-300 dark:peer-focus:ring-blue-800 rounded-full peer dark:bg-gray-700 peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all dark:border-gray-600 peer-checked:bg-blue-600"></div>
                    </label>
                  </div>

                  <div className={`p-3 rounded-lg ${
                    formData.activo 
                      ? 'bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800'
                      : 'bg-yellow-50 dark:bg-yellow-900/20 border border-yellow-200 dark:border-yellow-800'
                  }`}>
                    <div className="flex items-center gap-2">
                      <div className={`w-2 h-2 rounded-full ${
                        formData.activo ? 'bg-green-500' : 'bg-yellow-500'
                      }`}></div>
                      <span className={`text-sm font-medium ${
                        formData.activo 
                          ? 'text-green-800 dark:text-green-200'
                          : 'text-yellow-800 dark:text-yellow-200'
                      }`}>
                        {formData.activo ? 'Activa' : 'Inactiva'}
                      </span>
                    </div>
                    <p className={`text-xs mt-1 ${
                      formData.activo 
                        ? 'text-green-700 dark:text-green-300'
                        : 'text-yellow-700 dark:text-yellow-300'
                    }`}>
                      {formData.activo 
                        ? 'La categoría será visible en el sitio web'
                        : 'La categoría no será visible en el sitio web'
                      }
                    </p>
                  </div>
                </div>
              </div>

              {/* Preview Card */}
              <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
                <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
                  Vista Previa
                </h3>

                <div className="space-y-3">
                  {/* Category Preview */}
                  <div className="border border-gray-200 dark:border-gray-600 rounded-lg p-3">
                    <div className="flex items-center gap-3">
                      <div className="w-10 h-10 bg-gray-100 dark:bg-gray-700 rounded-lg flex items-center justify-center">
                        {formData.icono ? (
                          <span className="text-lg">{formData.icono}</span>
                        ) : (
                          <svg className="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
                          </svg>
                        )}
                      </div>
                      <div className="flex-1 min-w-0">
                        <h4 className="font-medium text-gray-900 dark:text-white truncate">
                          {formData.nombre || 'Nombre de la categoría'}
                        </h4>
                        {formData.descripcion && (
                          <p className="text-sm text-gray-600 dark:text-gray-400 truncate">
                            {formData.descripcion}
                          </p>
                        )}
                        <div className="flex items-center gap-2 mt-1">
                          <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${
                            formData.activo
                              ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
                              : 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200'
                          }`}>
                            {formData.activo ? 'Activa' : 'Inactiva'}
                          </span>
                          {formData.slug && (
                            <span className="text-xs text-gray-500 dark:text-gray-400">
                              /{formData.slug}
                            </span>
                          )}
                        </div>
                      </div>
                    </div>
                  </div>

                  {/* SEO Preview */}
                  {(formData.metaTitle || formData.metaDescription) && (
                    <div className="border border-gray-200 dark:border-gray-600 rounded-lg p-3">
                      <h5 className="text-xs font-medium text-gray-500 dark:text-gray-400 mb-2">
                        Vista previa en Google:
                      </h5>
                      <div className="space-y-1">
                        <h6 className="text-blue-600 dark:text-blue-400 text-sm font-medium truncate">
                          {formData.metaTitle || formData.nombre}
                        </h6>
                        <p className="text-green-700 dark:text-green-400 text-xs">
                          ejemplo.com/categorias/{formData.slug || 'categoria'}
                        </p>
                        {formData.metaDescription && (
                          <p className="text-gray-600 dark:text-gray-400 text-xs line-clamp-2">
                            {formData.metaDescription}
                          </p>
                        )}
                      </div>
                    </div>
                  )}
                </div>
              </div>

              {/* Help Card */}
              <div className="bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg p-4">
                <h4 className="text-sm font-medium text-blue-800 dark:text-blue-200 mb-2">
                  💡 Consejos
                </h4>
                <ul className="text-sm text-blue-700 dark:text-blue-300 space-y-1">
                  <li>• Use nombres descriptivos y únicos</li>
                  <li>• Los iconos ayudan a identificar categorías</li>
                  <li>• El slug se usa en la URL de la categoría</li>
                  <li>• El orden determina la posición en listas</li>
                  <li>• Complete la información SEO para mejor posicionamiento</li>
                </ul>
              </div>
            </div>
          </div>

          {/* Action Buttons */}
          <div className="flex flex-col sm:flex-row gap-4 pt-6 border-t border-gray-200 dark:border-gray-700">
            <div className="flex-1">
              <div className="flex flex-col sm:flex-row gap-3">
                <button
                  type="submit"
                  disabled={submitting}
                  className="flex-1 sm:flex-initial px-6 py-3 bg-blue-600 hover:bg-blue-700 disabled:opacity-50 text-white rounded-lg font-medium transition-colors flex items-center justify-center gap-2"
                >
                  {submitting ? (
                    <>
                      <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                      {isEditing ? 'Actualizando...' : 'Creando...'}
                    </>
                  ) : (
                    <>
                      <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                      </svg>
                      {isEditing ? 'Actualizar Categoría' : 'Crear Categoría'}
                    </>
                  )}
                </button>

                <button
                  type="button"
                  onClick={() => navigate('/categories')}
                  disabled={submitting}
                  className="px-6 py-3 bg-gray-600 hover:bg-gray-700 disabled:opacity-50 text-white rounded-lg font-medium transition-colors flex items-center justify-center gap-2"
                >
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                  </svg>
                  Cancelar
                </button>
              </div>
            </div>

            {/* Additional Actions for Edit Mode */}
            {isEditing && (
              <div className="flex gap-3">
                <PermissionGate permissions={[PERMISSIONS.CATEGORIES.VIEW]}>
                  <button
                    type="button"
                    onClick={() => navigate(`/categories/${id}`)}
                    disabled={submitting}
                    className="px-4 py-3 bg-green-600 hover:bg-green-700 disabled:opacity-50 text-white rounded-lg font-medium transition-colors flex items-center gap-2"
                  >
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                    </svg>
                    Ver
                  </button>
                </PermissionGate>

                <PermissionGate permissions={[PERMISSIONS.CATEGORIES.DELETE]}>
                  <button
                    type="button"
                    onClick={() => {
                      if (window.confirm('¿Está seguro de que desea eliminar esta categoría?')) {
                        // Handle delete logic here or redirect to delete handler
                        console.log('Delete category');
                      }
                    }}
                    disabled={submitting}
                    className="px-4 py-3 bg-red-600 hover:bg-red-700 disabled:opacity-50 text-white rounded-lg font-medium transition-colors flex items-center gap-2"
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
        </form>
      </div>
    </div>
  );
};

export default CategoryForm;