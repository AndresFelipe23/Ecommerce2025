// pages/Brands/BrandForm.tsx
import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import brandsService, { CreateBrandRequest, UpdateBrandRequest } from '../../services/brandsService';
import { AxiosError } from 'axios';
import { ApiResponse } from '../../services/brandsService';

interface BrandFormData {
  nombre: string;
  descripcion: string;
  logo: string;
  sitioWeb: string;
  activo: boolean;
}

const BrandForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = Boolean(id);

  const [formData, setFormData] = useState<BrandFormData>({
    nombre: '',
    descripcion: '',
    logo: '',
    sitioWeb: '',
    activo: true
  });

  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  // Load brand data for editing
  useEffect(() => {
    if (isEdit && id) {
      loadBrand(parseInt(id));
    }
  }, [isEdit, id]);

  const loadBrand = async (brandId: number) => {
    try {
      setLoading(true);
      const brand = await brandsService.getBrand(brandId);
      setFormData({
        nombre: brand.nombre,
        descripcion: brand.descripcion || '',
        logo: brand.logo || '',
        sitioWeb: brand.sitioWeb || '',
        activo: brand.activo
      });
    } catch (error) {
      const axiosError = error as AxiosError<ApiResponse<unknown>>;
      console.error('Error loading brand:', axiosError);
      alert(axiosError.response?.data?.message || 'Error al cargar la marca');
      navigate('/brands');
    } finally {
      setLoading(false);
    }
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value, type } = e.target;
    const finalValue = type === 'checkbox' ? (e.target as HTMLInputElement).checked : value;
    
    setFormData(prev => ({
      ...prev,
      [name]: finalValue
    }));

    // Clear error when user starts typing
    if (errors[name]) {
      setErrors(prev => ({
        ...prev,
        [name]: ''
      }));
    }
  };

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.nombre.trim()) {
      newErrors.nombre = 'El nombre es requerido';
    } else if (formData.nombre.trim().length < 2) {
      newErrors.nombre = 'El nombre debe tener al menos 2 caracteres';
    }

    if (formData.sitioWeb && !isValidUrl(formData.sitioWeb)) {
      newErrors.sitioWeb = 'Ingrese una URL válida';
    }

    if (formData.logo && !isValidUrl(formData.logo)) {
      newErrors.logo = 'Ingrese una URL válida para el logo';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const isValidUrl = (string: string): boolean => {
    try {
      new URL(string);
      return true;
    } catch {
      return false;
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateForm()) return;

    try {
      setSaving(true);
      
      if (isEdit && id) {
        const updateData: UpdateBrandRequest = {
          nombre: formData.nombre.trim(),
          descripcion: formData.descripcion.trim() || undefined,
          logo: formData.logo.trim() || undefined,
          sitioWeb: formData.sitioWeb.trim() || undefined,
          activo: formData.activo
        };
        await brandsService.updateBrand(parseInt(id), updateData);
      } else {
        const createData: CreateBrandRequest = {
          nombre: formData.nombre.trim(),
          descripcion: formData.descripcion.trim() || undefined,
          logo: formData.logo.trim() || undefined,
          sitioWeb: formData.sitioWeb.trim() || undefined
        };
        await brandsService.createBrand(createData);
      }

      navigate('/brands');
    } catch (error) {
      const axiosError = error as AxiosError<ApiResponse<unknown>>;
      console.error('Error saving brand:', axiosError);
      alert(axiosError.response?.data?.message || 'Error al guardar la marca');
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <div className="p-6">
        <div className="flex items-center justify-center h-64">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
          <span className="ml-2 text-gray-600 dark:text-gray-400">Cargando...</span>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 max-w-4xl mx-auto">
      {/* Header */}
      <div className="mb-6">
        <div className="flex items-center gap-4 mb-2">
          <button
            onClick={() => navigate('/brands')}
            className="text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
          >
            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
            </svg>
          </button>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white">
            {isEdit ? 'Editar Marca' : 'Nueva Marca'}
          </h1>
        </div>
        <p className="text-gray-600 dark:text-gray-400">
          {isEdit ? 'Modifica la información de la marca' : 'Completa los datos para crear una nueva marca'}
        </p>
      </div>

      {/* Form */}
      <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700">
        <form onSubmit={handleSubmit} className="p-6 space-y-6">
          {/* Basic Information */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {/* Brand Name */}
            <div className="md:col-span-2">
              <label htmlFor="nombre" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Nombre de la Marca <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                id="nombre"
                name="nombre"
                value={formData.nombre}
                onChange={handleInputChange}
                className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent dark:bg-gray-700 dark:text-white transition-colors ${
                  errors.nombre 
                    ? 'border-red-500 focus:ring-red-500' 
                    : 'border-gray-300 dark:border-gray-600'
                }`}
                placeholder="Ej: Apple, Samsung, Sony..."
                maxLength={100}
              />
              {errors.nombre && (
                <p className="mt-1 text-sm text-red-600 dark:text-red-400">{errors.nombre}</p>
              )}
            </div>

            {/* Logo URL */}
            <div>
              <label htmlFor="logo" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                URL del Logo
              </label>
              <input
                type="url"
                id="logo"
                name="logo"
                value={formData.logo}
                onChange={handleInputChange}
                className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent dark:bg-gray-700 dark:text-white transition-colors ${
                  errors.logo 
                    ? 'border-red-500 focus:ring-red-500' 
                    : 'border-gray-300 dark:border-gray-600'
                }`}
                placeholder="https://ejemplo.com/logo.png"
              />
              {errors.logo && (
                <p className="mt-1 text-sm text-red-600 dark:text-red-400">{errors.logo}</p>
              )}
              
              {/* Logo Preview */}
              {formData.logo && !errors.logo && (
                <div className="mt-2">
                  <img 
                    src={formData.logo} 
                    alt="Vista previa del logo"
                    className="h-12 w-12 object-contain border border-gray-200 dark:border-gray-600 rounded"
                    onError={(e) => {
                      (e.target as HTMLImageElement).style.display = 'none';
                    }}
                  />
                </div>
              )}
            </div>

            {/* Website URL */}
            <div>
              <label htmlFor="sitioWeb" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Sitio Web
              </label>
              <input
                type="url"
                id="sitioWeb"
                name="sitioWeb"
                value={formData.sitioWeb}
                onChange={handleInputChange}
                className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent dark:bg-gray-700 dark:text-white transition-colors ${
                  errors.sitioWeb 
                    ? 'border-red-500 focus:ring-red-500' 
                    : 'border-gray-300 dark:border-gray-600'
                }`}
                placeholder="https://www.marca.com"
              />
              {errors.sitioWeb && (
                <p className="mt-1 text-sm text-red-600 dark:text-red-400">{errors.sitioWeb}</p>
              )}
            </div>
          </div>

          {/* Description */}
          <div>
            <label htmlFor="descripcion" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Descripción
            </label>
            <textarea
              id="descripcion"
              name="descripcion"
              value={formData.descripcion}
              onChange={handleInputChange}
              rows={4}
              className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent dark:bg-gray-700 dark:text-white resize-none"
              placeholder="Descripción de la marca, historia, productos que ofrece..."
              maxLength={500}
            />
            <div className="mt-1 text-sm text-gray-500 dark:text-gray-400 text-right">
              {formData.descripcion.length}/500 caracteres
            </div>
          </div>

          {/* Status (only for edit) */}
          {isEdit && (
            <div className="flex items-center">
              <input
                type="checkbox"
                id="activo"
                name="activo"
                checked={formData.activo}
                onChange={handleInputChange}
                className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
              />
              <label htmlFor="activo" className="ml-2 block text-sm text-gray-700 dark:text-gray-300">
                Marca activa
              </label>
            </div>
          )}

          {/* Form Actions */}
          <div className="flex justify-end gap-4 pt-6 border-t border-gray-200 dark:border-gray-700">
            <button
              type="button"
              onClick={() => navigate('/brands')}
              className="px-4 py-2 text-gray-700 dark:text-gray-300 bg-gray-100 hover:bg-gray-200 dark:bg-gray-700 dark:hover:bg-gray-600 rounded-lg transition-colors"
              disabled={saving}
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={saving}
              className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
            >
              {saving && (
                <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
              )}
              {saving ? 'Guardando...' : (isEdit ? 'Actualizar Marca' : 'Crear Marca')}
            </button>
          </div>
        </form>
      </div>

      {/* Help Section */}
      <div className="mt-6 bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg p-4">
        <h3 className="text-sm font-medium text-blue-800 dark:text-blue-200 mb-2">
          💡 Consejos para crear una marca exitosa:
        </h3>
        <ul className="text-sm text-blue-700 dark:text-blue-300 space-y-1">
          <li>• Use un nombre claro y memorable</li>
          <li>• Agregue un logo de alta calidad (formato PNG con fondo transparente recomendado)</li>
          <li>• Incluya el sitio web oficial para mayor credibilidad</li>
          <li>• Escriba una descripción que destaque los valores de la marca</li>
        </ul>
      </div>
    </div>
  );
};

export default BrandForm;