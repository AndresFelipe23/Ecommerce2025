import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { CreateProductDto, UpdateProductDto } from '../../types/products';
import { CategorySummaryDto } from '../../types/categories';
import { BrandSummaryDto } from '../../types/brands';
import productsService from '../../services/productsService';
import categoriesService from '../../services/categoryService';
import brandsService from '../../services/brandsService';

interface ProductFormProps {
  mode: 'create' | 'edit';
}

interface FormData {
  SKU: string;
  Nombre: string;
  DescripcionCorta: string;
  DescripcionLarga: string;
  Slug: string;
  Precio: number;
  PrecioComparacion?: number;
  Costo?: number;
  CategoriaId: number;
  MarcaId: number;
  Tipo: string;
  Estado: string;
  Destacado: boolean;
  Nuevo: boolean;
  EnOferta: boolean;
  Peso?: number;
  Dimensiones: string;
  MetaTitulo: string;
  MetaDescripcion: string;
  PalabrasClaves: string;
  RequiereEnvio: boolean;
  PermiteReseñas: boolean;
  Garantia: string;
  Orden: number;
  Activo: boolean;
  StockInicial?: number;
}

interface ValidationErrors {
  [key: string]: string;
}

const ProductForm: React.FC<ProductFormProps> = ({ mode }) => {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditing = mode === 'edit';

  // Estados del formulario
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [validationErrors, setValidationErrors] = useState<ValidationErrors>({});
  const [categories, setCategories] = useState<CategorySummaryDto[]>([]);
  const [brands, setBrands] = useState<BrandSummaryDto[]>([]);
  const [currentStep, setCurrentStep] = useState(1);
  const [isDirty, setIsDirty] = useState(false);

  // Estado del producto
  const [formData, setFormData] = useState<FormData>({
    SKU: '',
    Nombre: '',
    DescripcionCorta: '',
    DescripcionLarga: '',
    Slug: '',
    Precio: 0,
    PrecioComparacion: undefined,
    Costo: undefined,
    CategoriaId: 0,
    MarcaId: 0,
    Tipo: 'simple',
    Estado: 'borrador',
    Destacado: false,
    Nuevo: false,
    EnOferta: false,
    Peso: undefined,
    Dimensiones: '',
    MetaTitulo: '',
    MetaDescripcion: '',
    PalabrasClaves: '',
    RequiereEnvio: true,
    PermiteReseñas: true,
    Garantia: '',
    Orden: 0,
    Activo: true,
    ...(isEditing ? {} : { StockInicial: 0 })
  });

  // Auto-generar slug cuando cambie el nombre
  const generateSlug = useCallback((name: string) => {
    return name
      .toLowerCase()
      .replace(/[^a-z0-9\s-]/g, '')
      .replace(/\s+/g, '-')
      .replace(/-+/g, '-')
      .trim();
  }, []);

  useEffect(() => {
    if (formData.Nombre && !formData.Slug) {
      setFormData(prev => ({
        ...prev,
        Slug: generateSlug(prev.Nombre)
      }));
    }
  }, [formData.Nombre, generateSlug]);

  // Validación en tiempo real
  const validateField = (name: string, value: any): string => {
    switch (name) {
      case 'SKU':
        if (!value || value.trim().length < 2) return 'SKU debe tener al menos 2 caracteres';
        break;
      case 'Nombre':
        if (!value || value.trim().length < 2) return 'Nombre debe tener al menos 2 caracteres';
        break;
      case 'Precio':
        if (!value || value <= 0) return 'Precio debe ser mayor a 0';
        break;
      case 'CategoriaId':
        if (!value || value === 0) return 'Debe seleccionar una categoría';
        break;
      case 'MarcaId':
        if (!value || value === 0) return 'Debe seleccionar una marca';
        break;
      case 'PrecioComparacion':
        if (value && value <= formData.Precio) return 'Precio de comparación debe ser mayor al precio actual';
        break;
      case 'Costo':
        if (value && value >= formData.Precio) return 'Costo debe ser menor al precio de venta';
        break;
    }
    return '';
  };

  // Cargar datos iniciales
  useEffect(() => {
    const loadInitialData = async () => {
      setLoading(true);
      try {
        const [categoriesResponse, brandsResponse] = await Promise.all([
          categoriesService.getCategories({ page: 1, pageSize: 100, activo: true }),
          brandsService.getBrands({ page: 1, pageSize: 100, activo: true })
        ]);

        setCategories(categoriesResponse.items.map((cat: CategorySummaryDto) => ({
          id: cat.id,
          nombre: cat.nombre,
          slug: cat.slug,
          activo: cat.activo,
          rutaCompleta: cat.rutaCompleta
        })));

        setBrands(brandsResponse.items.map((brand: BrandSummaryDto) => ({
          id: brand.id,
          nombre: brand.nombre,
          logo: brand.logo,
          slug: brand.slug,
          activo: brand.activo,
          totalProductos: brand.totalProductos
        })));

        if (isEditing && id) {
          const product = await productsService.getProductById(parseInt(id));
          setFormData({
            SKU: product.SKU,
            Nombre: product.Nombre,
            DescripcionCorta: product.DescripcionCorta || '',
            DescripcionLarga: product.DescripcionLarga || '',
            Slug: product.Slug,
            Precio: product.Precio,
            PrecioComparacion: product.PrecioComparacion || undefined,
            Costo: product.Costo || undefined,
            CategoriaId: product.CategoriaId,
            MarcaId: product.MarcaId,
            Tipo: product.Tipo || 'simple',
            Estado: product.Estado || 'borrador',
            Destacado: product.Destacado,
            Nuevo: product.Nuevo,
            EnOferta: product.EnOferta,
            Peso: product.Peso || undefined,
            Dimensiones: product.Dimensiones || '',
            MetaTitulo: product.MetaTitulo || '',
            MetaDescripcion: product.MetaDescripcion || '',
            PalabrasClaves: product.PalabrasClaves || '',
            RequiereEnvio: product.RequiereEnvio,
            PermiteReseñas: product.PermiteReseñas,
            Garantia: product.Garantia || '',
            Orden: product.Orden,
            Activo: product.Activo
          });
        }
      } catch (err) {
        setError('Error al cargar los datos del formulario');
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    loadInitialData();
  }, [isEditing, id]);

  // Manejar cambios en el formulario
  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;
    const newValue = type === 'checkbox' 
      ? (e.target as HTMLInputElement).checked
      : type === 'number' 
        ? parseFloat(value) || 0
        : value;

    setFormData(prev => ({
      ...prev,
      [name]: newValue
    }));

    setIsDirty(true);

    // Validación en tiempo real
    const errorMessage = validateField(name, newValue);
    setValidationErrors(prev => ({
      ...prev,
      [name]: errorMessage
    }));

    // Limpiar error general si se está corrigiendo
    if (error) setError(null);
  };

  // Validar formulario completo
  const validateForm = (): boolean => {
    const errors: ValidationErrors = {};
    
    Object.keys(formData).forEach(key => {
      const error = validateField(key, formData[key as keyof FormData]);
      if (error) errors[key] = error;
    });

    setValidationErrors(errors);
    return Object.keys(errors).length === 0;
  };

  // Enviar formulario
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateForm()) {
      setError('Por favor corrige los errores antes de continuar');
      return;
    }

    setSaving(true);
    setError(null);

    try {
      if (isEditing && id) {
        const updateData: UpdateProductDto = {
          SKU: formData.SKU,
          Nombre: formData.Nombre,
          DescripcionCorta: formData.DescripcionCorta || undefined,
          DescripcionLarga: formData.DescripcionLarga || undefined,
          Slug: formData.Slug || undefined,
          Precio: Number(formData.Precio),
          PrecioComparacion: formData.PrecioComparacion,
          Costo: formData.Costo,
          CategoriaId: Number(formData.CategoriaId),
          MarcaId: Number(formData.MarcaId),
          Tipo: formData.Tipo || undefined,
          Estado: formData.Estado || undefined,
          Destacado: Boolean(formData.Destacado),
          Nuevo: Boolean(formData.Nuevo),
          EnOferta: Boolean(formData.EnOferta),
          Peso: formData.Peso,
          Dimensiones: formData.Dimensiones || undefined,
          MetaTitulo: formData.MetaTitulo || undefined,
          MetaDescripcion: formData.MetaDescripcion || undefined,
          PalabrasClaves: formData.PalabrasClaves || undefined,
          RequiereEnvio: Boolean(formData.RequiereEnvio),
          PermiteReseñas: Boolean(formData.PermiteReseñas),
          Garantia: formData.Garantia || undefined,
          Orden: Number(formData.Orden),
          Activo: Boolean(formData.Activo),
          Imagenes: []
        };
        await productsService.update(parseInt(id), updateData);
      } else {
        const createData: CreateProductDto = {
          SKU: formData.SKU,
          Nombre: formData.Nombre,
          DescripcionCorta: formData.DescripcionCorta || undefined,
          DescripcionLarga: formData.DescripcionLarga || undefined,
          Slug: formData.Slug || undefined,
          Precio: Number(formData.Precio),
          PrecioComparacion: formData.PrecioComparacion,
          Costo: formData.Costo,
          CategoriaId: Number(formData.CategoriaId),
          MarcaId: Number(formData.MarcaId),
          Tipo: formData.Tipo || undefined,
          Estado: formData.Estado || undefined,
          Destacado: Boolean(formData.Destacado),
          Nuevo: Boolean(formData.Nuevo),
          EnOferta: Boolean(formData.EnOferta),
          Peso: formData.Peso,
          Dimensiones: formData.Dimensiones || undefined,
          MetaTitulo: formData.MetaTitulo || undefined,
          MetaDescripcion: formData.MetaDescripcion || undefined,
          PalabrasClaves: formData.PalabrasClaves || undefined,
          RequiereEnvio: Boolean(formData.RequiereEnvio),
          PermiteReseñas: Boolean(formData.PermiteReseñas),
          Garantia: formData.Garantia || undefined,
          Orden: Number(formData.Orden),
          StockInicial: formData.StockInicial || 0,
          Imagenes: []
        };
        await productsService.createProduct(createData);
      }
      
      setIsDirty(false);
      navigate('/products');
    } catch (error: unknown) {
      console.error('Error completo:', error);
      setError(error instanceof Error ? error.message : 'Error al guardar el producto');
    } finally {
      setSaving(false);
    }
  };

  // Calcular margen si hay precio y costo
  const calcularMargen = () => {
    if (formData.Precio && formData.Costo) {
      return (((formData.Precio - formData.Costo) / formData.Precio) * 100).toFixed(1);
    }
    return null;
  };

  // Pasos del formulario
  const steps = [
    { id: 1, name: 'Información Básica', icon: '📝' },
    { id: 2, name: 'Precios y Categoría', icon: '💰' },
    { id: 3, name: 'Configuración', icon: '⚙️' },
    { id: 4, name: 'SEO y Extras', icon: '🔍' }
  ];

  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100 flex items-center justify-center">
        <div className="bg-white rounded-2xl shadow-xl p-8 max-w-md w-full mx-4">
          <div className="flex flex-col items-center space-y-4">
            <div className="relative">
              <div className="animate-spin rounded-full h-16 w-16 border-4 border-blue-200"></div>
              <div className="animate-spin rounded-full h-16 w-16 border-4 border-blue-600 border-t-transparent absolute top-0"></div>
            </div>
            <div className="text-center">
              <h3 className="text-xl font-bold text-gray-900 mb-2">
                Cargando formulario
              </h3>
              <p className="text-gray-600">
                Preparando los datos necesarios...
              </p>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100">
      {/* Header */}
      <div className="bg-white/80 backdrop-blur-lg border-b border-gray-200/50 sticky top-0 z-40">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between h-20">
            <div className="flex items-center space-x-4">
              <button
                onClick={() => navigate('/products')}
                className="flex items-center space-x-2 text-gray-600 hover:text-gray-900 transition-colors duration-200"
              >
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 19l-7-7m0 0l7-7m-7 7h18" />
                </svg>
                <span className="font-medium">Volver</span>
              </button>
              <div className="h-6 w-px bg-gray-300"></div>
              <div>
                <h1 className="text-2xl font-bold bg-gradient-to-r from-gray-900 to-gray-700 bg-clip-text text-transparent">
                  {isEditing ? 'Editar Producto' : 'Crear Producto'}
                </h1>
                <p className="text-sm text-gray-600">
                  {isEditing ? 'Modifica la información del producto' : 'Completa los datos del nuevo producto'}
                </p>
              </div>
            </div>
            
            {isDirty && (
              <div className="flex items-center space-x-2 text-orange-600 bg-orange-50 px-3 py-1 rounded-full">
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.084 16.5c-.77.833.192 2.5 1.732 2.5z" />
                </svg>
                <span className="text-sm font-medium">Cambios sin guardar</span>
              </div>
            )}
          </div>
        </div>
      </div>

      <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Indicador de pasos */}
        <div className="mb-8">
          <div className="flex items-center justify-between">
            {steps.map((step) => (
              <div key={step.id} className="flex items-center">
                <div className={`flex items-center justify-center w-12 h-12 rounded-full border-2 transition-all duration-200 ${
                  currentStep >= step.id
                    ? 'bg-blue-600 border-blue-600 text-white'
                    : 'bg-white border-gray-300 text-gray-400'
                }`}>
                  <span className="text-lg">{step.icon}</span>
                </div>
                <div className="ml-3 hidden sm:block">
                  <p className={`text-sm font-medium ${
                    currentStep >= step.id ? 'text-blue-600' : 'text-gray-400'
                  }`}>
                    {step.name}
                  </p>
                </div>
                {step.id < steps.length && (
                  <div className={`hidden sm:block w-16 h-0.5 ml-4 ${
                    currentStep > step.id ? 'bg-blue-600' : 'bg-gray-300'
                  }`}></div>
                )}
              </div>
            ))}
          </div>
        </div>

        {/* Alertas */}
        {error && (
          <div className="mb-8 bg-red-50/80 backdrop-blur-sm border border-red-200 rounded-2xl p-6 flex items-start space-x-4">
            <div className="w-8 h-8 bg-red-100 rounded-full flex items-center justify-center flex-shrink-0">
              <svg className="w-5 h-5 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
            <div>
              <h3 className="text-red-800 font-semibold">Error en el formulario</h3>
              <p className="text-red-700 text-sm mt-1">{error}</p>
            </div>
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-8">
          {/* Paso 1: Información básica */}
          {currentStep === 1 && (
            <div className="bg-white/70 backdrop-blur-sm rounded-2xl border border-gray-200/50 p-8">
              <div className="flex items-center space-x-3 mb-6">
                <span className="text-2xl">📝</span>
                <h2 className="text-2xl font-bold text-gray-900">Información Básica</h2>
              </div>
              
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                  <label className="block text-sm font-semibold text-gray-700 mb-2">
                    SKU *
                  </label>
                  <input
                    type="text"
                    name="SKU"
                    value={formData.SKU}
                    onChange={handleInputChange}
                    className={`w-full border rounded-xl px-4 py-3 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-all duration-200 ${
                      validationErrors.SKU ? 'border-red-300 bg-red-50' : 'border-gray-300'
                    }`}
                    placeholder="Ej: PROD-001"
                    required
                  />
                  {validationErrors.SKU && (
                    <p className="text-red-600 text-sm mt-1">{validationErrors.SKU}</p>
                  )}
                </div>

                <div>
                  <label className="block text-sm font-semibold text-gray-700 mb-2">
                    Nombre del Producto *
                  </label>
                  <input
                    type="text"
                    name="Nombre"
                    value={formData.Nombre}
                    onChange={handleInputChange}
                    className={`w-full border rounded-xl px-4 py-3 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-all duration-200 ${
                      validationErrors.Nombre ? 'border-red-300 bg-red-50' : 'border-gray-300'
                    }`}
                    placeholder="Nombre descriptivo del producto"
                    required
                  />
                  {validationErrors.Nombre && (
                    <p className="text-red-600 text-sm mt-1">{validationErrors.Nombre}</p>
                  )}
                </div>

                <div>
                  <label className="block text-sm font-semibold text-gray-700 mb-2">
                    Slug URL
                  </label>
                  <input
                    type="text"
                    name="Slug"
                    value={formData.Slug}
                    onChange={handleInputChange}
                    className="w-full border border-gray-300 rounded-xl px-4 py-3 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-all duration-200"
                    placeholder="Se genera automáticamente"
                  />
                  <p className="text-gray-500 text-xs mt-1">
                    URL amigable para el producto (se genera automáticamente del nombre)
                  </p>
                </div>

                {!isEditing && (
                  <div>
                    <label className="block text-sm font-semibold text-gray-700 mb-2">
                      Stock Inicial
                    </label>
                    <input
                      type="number"
                      name="StockInicial"
                      value={formData.StockInicial || 0}
                      onChange={handleInputChange}
                      min="0"
                      className="w-full border border-gray-300 rounded-xl px-4 py-3 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-all duration-200"
                      placeholder="Cantidad inicial en inventario"
                    />
                  </div>
                )}
              </div>

              <div className="mt-6">
                <label className="block text-sm font-semibold text-gray-700 mb-2">
                  Descripción Corta
                </label>
                <textarea
                  name="DescripcionCorta"
                  value={formData.DescripcionCorta}
                  onChange={handleInputChange}
                  rows={3}
                  className="w-full border border-gray-300 rounded-xl px-4 py-3 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-all duration-200 resize-none"
                  placeholder="Descripción breve para listados y búsquedas"
                />
                <p className="text-gray-500 text-xs mt-1">
                  Máximo 160 caracteres - aparece en listados de productos
                </p>
              </div>

              <div className="mt-6">
                <label className="block text-sm font-semibold text-gray-700 mb-2">
                  Descripción Detallada
                </label>
                <textarea
                  name="DescripcionLarga"
                  value={formData.DescripcionLarga}
                  onChange={handleInputChange}
                  rows={6}
                  className="w-full border border-gray-300 rounded-xl px-4 py-3 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-all duration-200 resize-none"
                  placeholder="Descripción completa del producto, características, beneficios..."
                />
              </div>

              <div className="flex justify-end mt-8">
                <button
                  type="button"
                  onClick={() => setCurrentStep(2)}
                  className="bg-gradient-to-r from-blue-600 to-purple-600 text-white px-8 py-3 rounded-xl hover:from-blue-700 hover:to-purple-700 focus:outline-none focus:ring-2 focus:ring-blue-500 font-medium transition-all duration-200 transform hover:scale-105"
                >
                  Siguiente: Precios
                </button>
              </div>
            </div>
          )}

          {/* Paso 2: Precios y categorías */}
          {currentStep === 2 && (
            <div className="bg-white/70 backdrop-blur-sm rounded-2xl border border-gray-200/50 p-8">
              <div className="flex items-center space-x-3 mb-6">
                <span className="text-2xl">💰</span>
                <h2 className="text-2xl font-bold text-gray-900">Precios y Categorización</h2>
              </div>
              
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                <div>
                  <label className="block text-sm font-semibold text-gray-700 mb-2">
                    Precio de Venta *
                  </label>
                  <div className="relative">
                    <span className="absolute left-3 top-3 text-gray-500">$</span>
                    <input
                      type="number"
                      name="Precio"
                      value={formData.Precio}
                      onChange={handleInputChange}
                      step="0.01"
                      min="0"
                      className={`w-full border rounded-xl pl-8 pr-4 py-3 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-all duration-200 ${
                        validationErrors.Precio ? 'border-red-300 bg-red-50' : 'border-gray-300'
                      }`}
                      placeholder="0.00"
                      required
                    />
                  </div>
                  {validationErrors.Precio && (
                    <p className="text-red-600 text-sm mt-1">{validationErrors.Precio}</p>
                  )}
                </div>

                <div>
                  <label className="block text-sm font-semibold text-gray-700 mb-2">
                    Precio de Comparación
                  </label>
                  <div className="relative">
                    <span className="absolute left-3 top-3 text-gray-500">$</span>
                    <input
                      type="number"
                      name="PrecioComparacion"
                      value={formData.PrecioComparacion || ''}
                      onChange={handleInputChange}
                      step="0.01"
                      min="0"
                      className={`w-full border rounded-xl pl-8 pr-4 py-3 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-all duration-200 ${
                        validationErrors.PrecioComparacion ? 'border-red-300 bg-red-50' : 'border-gray-300'
                      }`}
                      placeholder="Precio anterior"
                    />
                  </div>
                  {validationErrors.PrecioComparacion && (
                    <p className="text-red-600 text-sm mt-1">{validationErrors.PrecioComparacion}</p>
                  )}
                  <p className="text-gray-500 text-xs mt-1">
                    Para mostrar descuentos
                  </p>
                </div>

                <div>
                  <label className="block text-sm font-semibold text-gray-700 mb-2">
                    Costo del Producto
                  </label>
                  <div className="relative">
                    <span className="absolute left-3 top-3 text-gray-500">$</span>
                    <input
                      type="number"
                      name="Costo"
                      value={formData.Costo || ''}
                      onChange={handleInputChange}
                      step="0.01"
                      min="0"
                      className={`w-full border rounded-xl pl-8 pr-4 py-3 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-all duration-200 ${
                        validationErrors.Costo ? 'border-red-300 bg-red-50' : 'border-gray-300'
                      }`}
                      placeholder="Costo interno"
                    />
                  </div>
                  {validationErrors.Costo && (
                    <p className="text-red-600 text-sm mt-1">{validationErrors.Costo}</p>
                  )}
                </div>
              </div>

              {/* Margen calculado */}
              {calcularMargen() && (
                <div className="mt-6 bg-green-50 border border-green-200 rounded-xl p-4">
                  <div className="flex items-center space-x-2">
                    <svg className="w-5 h-5 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
                    </svg>
                    <span className="text-green-800 font-semibold">
                      Margen de ganancia: {calcularMargen()}%
                    </span>
                  </div>
                </div>
              )}

              <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mt-6">
                <div>
                  <label className="block text-sm font-semibold text-gray-700 mb-2">
                    Categoría *
                  </label>
                  <select
                    name="CategoriaId"
                    value={formData.CategoriaId}
                    onChange={handleInputChange}
                    className={`w-full border rounded-xl px-4 py-3 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-all duration-200 ${
                      validationErrors.CategoriaId ? 'border-red-300 bg-red-50' : 'border-gray-300'
                    }`}
                    required
                  >
                    <option value={0}>Seleccionar categoría</option>
                    {categories.map(category => (
                      <option key={category.id} value={category.id}>
                        {category.nombre}
                      </option>
                    ))}
                  </select>
                  {validationErrors.CategoriaId && (
                    <p className="text-red-600 text-sm mt-1">{validationErrors.CategoriaId}</p>
                  )}
                </div>

                <div>
                  <label className="block text-sm font-semibold text-gray-700 mb-2">
                    Marca *
                  </label>
                  <select
                    name="MarcaId"
                    value={formData.MarcaId}
                    onChange={handleInputChange}
                    className={`w-full border rounded-xl px-4 py-3 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-all duration-200 ${
                      validationErrors.MarcaId ? 'border-red-300 bg-red-50' : 'border-gray-300'
                    }`}
                    required
                  >
                    <option value={0}>Seleccionar marca</option>
                    {brands.map(brand => (
                      <option key={brand.id} value={brand.id}>
                        {brand.nombre}
                      </option>
                    ))}
                  </select>
                  {validationErrors.MarcaId && (
                    <p className="text-red-600 text-sm mt-1">{validationErrors.MarcaId}</p>
                  )}
                </div>
              </div>

              <div className="flex justify-between mt-8">
                <button
                  type="button"
                  onClick={() => setCurrentStep(1)}
                  className="bg-gray-100 text-gray-700 px-8 py-3 rounded-xl hover:bg-gray-200 focus:outline-none focus:ring-2 focus:ring-gray-500 font-medium transition-all duration-200"
                >
                  Anterior
                </button>
                <button
                  type="button"
                  onClick={() => setCurrentStep(3)}
                  className="bg-gradient-to-r from-blue-600 to-purple-600 text-white px-8 py-3 rounded-xl hover:from-blue-700 hover:to-purple-700 focus:outline-none focus:ring-2 focus:ring-blue-500 font-medium transition-all duration-200 transform hover:scale-105"
                >
                  Siguiente: Configuración
                </button>
              </div>
            </div>
          )}

          {/* Paso 3: Configuración */}
          {currentStep === 3 && (
            <div className="bg-white/70 backdrop-blur-sm rounded-2xl border border-gray-200/50 p-8">
              <div className="flex items-center space-x-3 mb-6">
                <span className="text-2xl">⚙️</span>
                <h2 className="text-2xl font-bold text-gray-900">Configuración del Producto</h2>
              </div>
              
              <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-6">
                <div>
                  <label className="block text-sm font-semibold text-gray-700 mb-2">
                    Tipo de Producto
                  </label>
                  <select
                    name="Tipo"
                    value={formData.Tipo}
                    onChange={handleInputChange}
                    className="w-full border border-gray-300 rounded-xl px-4 py-3 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-all duration-200"
                  >
                    <option value="simple">Simple</option>
                    <option value="variable">Variable</option>
                    <option value="digital">Digital</option>
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-semibold text-gray-700 mb-2">
                    Estado
                  </label>
                  <select
                    name="Estado"
                    value={formData.Estado}
                    onChange={handleInputChange}
                    className="w-full border border-gray-300 rounded-xl px-4 py-3 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-all duration-200"
                  >
                    <option value="borrador">Borrador</option>
                    <option value="publicado">Publicado</option>
                    <option value="pendiente">Pendiente</option>
                    <option value="archivado">Archivado</option>
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-semibold text-gray-700 mb-2">
                    Orden de Visualización
                  </label>
                  <input
                    type="number"
                    name="Orden"
                    value={formData.Orden}
                    onChange={handleInputChange}
                    min="0"
                    className="w-full border border-gray-300 rounded-xl px-4 py-3 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-all duration-200"
                    placeholder="0"
                  />
                </div>
              </div>

              {/* Características del producto */}
              <div className="bg-gray-50 rounded-xl p-6 mb-6">
                <h3 className="text-lg font-semibold text-gray-900 mb-4">Características Especiales</h3>
                <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-4">
                  <label className="flex items-center space-x-3 p-3 bg-white rounded-lg border border-gray-200 hover:border-blue-300 transition-colors duration-200 cursor-pointer">
                    <input
                      type="checkbox"
                      name="Destacado"
                      checked={formData.Destacado}
                      onChange={handleInputChange}
                      className="rounded border-gray-300 text-blue-600 shadow-sm focus:border-blue-300 focus:ring focus:ring-blue-200"
                    />
                    <div>
                      <div className="text-sm font-medium">Destacado</div>
                      <div className="text-xs text-gray-500">Aparece primero</div>
                    </div>
                  </label>

                  <label className="flex items-center space-x-3 p-3 bg-white rounded-lg border border-gray-200 hover:border-green-300 transition-colors duration-200 cursor-pointer">
                    <input
                      type="checkbox"
                      name="Nuevo"
                      checked={formData.Nuevo}
                      onChange={handleInputChange}
                      className="rounded border-gray-300 text-green-600 shadow-sm focus:border-green-300 focus:ring focus:ring-green-200"
                    />
                    <div>
                      <div className="text-sm font-medium">Nuevo</div>
                      <div className="text-xs text-gray-500">Badge "Nuevo"</div>
                    </div>
                  </label>

                  <label className="flex items-center space-x-3 p-3 bg-white rounded-lg border border-gray-200 hover:border-orange-300 transition-colors duration-200 cursor-pointer">
                    <input
                      type="checkbox"
                      name="EnOferta"
                      checked={formData.EnOferta}
                      onChange={handleInputChange}
                      className="rounded border-gray-300 text-orange-600 shadow-sm focus:border-orange-300 focus:ring focus:ring-orange-200"
                    />
                    <div>
                      <div className="text-sm font-medium">En Oferta</div>
                      <div className="text-xs text-gray-500">Badge "Oferta"</div>
                    </div>
                  </label>

                  <label className="flex items-center space-x-3 p-3 bg-white rounded-lg border border-gray-200 hover:border-purple-300 transition-colors duration-200 cursor-pointer">
                    <input
                      type="checkbox"
                      name="RequiereEnvio"
                      checked={formData.RequiereEnvio}
                      onChange={handleInputChange}
                      className="rounded border-gray-300 text-purple-600 shadow-sm focus:border-purple-300 focus:ring focus:ring-purple-200"
                    />
                    <div>
                      <div className="text-sm font-medium">Req. Envío</div>
                      <div className="text-xs text-gray-500">Producto físico</div>
                    </div>
                  </label>

                  <label className="flex items-center space-x-3 p-3 bg-white rounded-lg border border-gray-200 hover:border-indigo-300 transition-colors duration-200 cursor-pointer">
                    <input
                      type="checkbox"
                      name="PermiteReseñas"
                      checked={formData.PermiteReseñas}
                      onChange={handleInputChange}
                      className="rounded border-gray-300 text-indigo-600 shadow-sm focus:border-indigo-300 focus:ring focus:ring-indigo-200"
                    />
                    <div>
                      <div className="text-sm font-medium">Reseñas</div>
                      <div className="text-xs text-gray-500">Permite valorar</div>
                    </div>
                  </label>

                  {isEditing && (
                    <label className="flex items-center space-x-3 p-3 bg-white rounded-lg border border-gray-200 hover:border-green-300 transition-colors duration-200 cursor-pointer">
                      <input
                        type="checkbox"
                        name="Activo"
                        checked={formData.Activo}
                        onChange={handleInputChange}
                        className="rounded border-gray-300 text-green-600 shadow-sm focus:border-green-300 focus:ring focus:ring-green-200"
                      />
                      <div>
                        <div className="text-sm font-medium">Activo</div>
                        <div className="text-xs text-gray-500">Visible en tienda</div>
                      </div>
                    </label>
                  )}
                </div>
              </div>

              {/* Información física */}
              <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                <div>
                  <label className="block text-sm font-semibold text-gray-700 mb-2">
                    Peso (kg)
                  </label>
                  <input
                    type="number"
                    name="Peso"
                    value={formData.Peso || ''}
                    onChange={handleInputChange}
                    step="0.01"
                    min="0"
                    className="w-full border border-gray-300 rounded-xl px-4 py-3 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-all duration-200"
                    placeholder="0.00"
                  />
                </div>

                <div>
                  <label className="block text-sm font-semibold text-gray-700 mb-2">
                    Dimensiones
                  </label>
                  <input
                    type="text"
                    name="Dimensiones"
                    value={formData.Dimensiones}
                    onChange={handleInputChange}
                    className="w-full border border-gray-300 rounded-xl px-4 py-3 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-all duration-200"
                    placeholder="30x20x15 cm"
                  />
                </div>

                <div>
                  <label className="block text-sm font-semibold text-gray-700 mb-2">
                    Garantía
                  </label>
                  <input
                    type="text"
                    name="Garantia"
                    value={formData.Garantia}
                    onChange={handleInputChange}
                    className="w-full border border-gray-300 rounded-xl px-4 py-3 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-all duration-200"
                    placeholder="12 meses"
                  />
                </div>
              </div>

              <div className="flex justify-between mt-8">
                <button
                  type="button"
                  onClick={() => setCurrentStep(2)}
                  className="bg-gray-100 text-gray-700 px-8 py-3 rounded-xl hover:bg-gray-200 focus:outline-none focus:ring-2 focus:ring-gray-500 font-medium transition-all duration-200"
                >
                  Anterior
                </button>
                <button
                  type="button"
                  onClick={() => setCurrentStep(4)}
                  className="bg-gradient-to-r from-blue-600 to-purple-600 text-white px-8 py-3 rounded-xl hover:from-blue-700 hover:to-purple-700 focus:outline-none focus:ring-2 focus:ring-blue-500 font-medium transition-all duration-200 transform hover:scale-105"
                >
                  Siguiente: SEO
                </button>
              </div>
            </div>
          )}

          {/* Paso 4: SEO y extras */}
          {currentStep === 4 && (
            <div className="bg-white/70 backdrop-blur-sm rounded-2xl border border-gray-200/50 p-8">
              <div className="flex items-center space-x-3 mb-6">
                <span className="text-2xl">🔍</span>
                <h2 className="text-2xl font-bold text-gray-900">SEO y Metadatos</h2>
              </div>
              
              <div className="space-y-6">
                <div>
                  <label className="block text-sm font-semibold text-gray-700 mb-2">
                    Meta Título (SEO)
                  </label>
                  <input
                    type="text"
                    name="MetaTitulo"
                    value={formData.MetaTitulo}
                    onChange={handleInputChange}
                    className="w-full border border-gray-300 rounded-xl px-4 py-3 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-all duration-200"
                    placeholder="Título optimizado para motores de búsqueda"
                    maxLength={60}
                  />
                  <p className="text-gray-500 text-xs mt-1">
                    Máximo 60 caracteres - aparece en resultados de Google
                  </p>
                </div>

                <div>
                  <label className="block text-sm font-semibold text-gray-700 mb-2">
                    Meta Descripción (SEO)
                  </label>
                  <textarea
                    name="MetaDescripcion"
                    value={formData.MetaDescripcion}
                    onChange={handleInputChange}
                    rows={3}
                    className="w-full border border-gray-300 rounded-xl px-4 py-3 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-all duration-200 resize-none"
                    placeholder="Descripción que aparece en resultados de búsqueda"
                    maxLength={160}
                  />
                  <p className="text-gray-500 text-xs mt-1">
                    Máximo 160 caracteres - descripción en Google
                  </p>
                </div>

                <div>
                  <label className="block text-sm font-semibold text-gray-700 mb-2">
                    Palabras Clave (SEO)
                  </label>
                  <input
                    type="text"
                    name="PalabrasClaves"
                    value={formData.PalabrasClaves}
                    onChange={handleInputChange}
                    className="w-full border border-gray-300 rounded-xl px-4 py-3 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-all duration-200"
                    placeholder="palabra1, palabra2, palabra3"
                  />
                  <p className="text-gray-500 text-xs mt-1">
                    Separadas por comas - ayudan en la búsqueda interna
                  </p>
                </div>
              </div>

              {/* Información sobre imágenes */}
              <div className="mt-8 bg-gradient-to-r from-blue-50 to-purple-50 border border-blue-200 rounded-2xl p-6">
                <div className="flex items-start space-x-4">
                  <div className="w-12 h-12 bg-gradient-to-r from-blue-500 to-purple-500 rounded-xl flex items-center justify-center flex-shrink-0">
                    <svg className="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                    </svg>
                  </div>
                  <div>
                    <h3 className="text-lg font-semibold text-gray-900 mb-2">
                      📸 Próximo paso: Gestión de Imágenes
                    </h3>
                    <p className="text-gray-700 text-sm leading-relaxed">
                      Después de {isEditing ? 'actualizar' : 'crear'} el producto, podrás gestionar sus imágenes de forma 
                      independiente. Esto te permite un mejor control sobre la galería de fotos, establecer imagen principal, 
                      ajustar el orden de visualización y optimizar la carga de la página.
                    </p>
                    <div className="mt-3 flex items-center space-x-4 text-sm text-gray-600">
                      <div className="flex items-center space-x-1">
                        <svg className="w-4 h-4 text-green-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                        </svg>
                        <span>Múltiples imágenes</span>
                      </div>
                      <div className="flex items-center space-x-1">
                        <svg className="w-4 h-4 text-green-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                        </svg>
                        <span>Imagen principal</span>
                      </div>
                      <div className="flex items-center space-x-1">
                        <svg className="w-4 h-4 text-green-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                        </svg>
                        <span>Orden personalizable</span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <div className="flex justify-between mt-8">
                <button
                  type="button"
                  onClick={() => setCurrentStep(3)}
                  className="bg-gray-100 text-gray-700 px-8 py-3 rounded-xl hover:bg-gray-200 focus:outline-none focus:ring-2 focus:ring-gray-500 font-medium transition-all duration-200"
                >
                  Anterior
                </button>
                <button
                  type="submit"
                  disabled={saving}
                  className="bg-gradient-to-r from-green-600 to-blue-600 text-white px-8 py-3 rounded-xl hover:from-green-700 hover:to-blue-700 disabled:opacity-50 focus:outline-none focus:ring-2 focus:ring-green-500 font-medium transition-all duration-200 transform hover:scale-105 flex items-center space-x-2"
                >
                  {saving ? (
                    <>
                      <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-white"></div>
                      <span>Guardando...</span>
                    </>
                  ) : (
                    <>
                      <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                      </svg>
                      <span>{isEditing ? 'Actualizar Producto' : 'Crear Producto'}</span>
                    </>
                  )}
                </button>
              </div>
            </div>
          )}
        </form>

        {/* Navegación rápida entre pasos */}
        <div className="mt-8 bg-white/70 backdrop-blur-sm rounded-2xl border border-gray-200/50 p-4">
          <div className="flex justify-center space-x-2">
            {steps.map((step) => (
              <button
                key={step.id}
                onClick={() => setCurrentStep(step.id)}
                className={`px-4 py-2 rounded-lg text-sm font-medium transition-all duration-200 ${
                  currentStep === step.id
                    ? 'bg-blue-600 text-white shadow-lg'
                    : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
                }`}
              >
                {step.icon} {step.name}
              </button>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};

export default ProductForm;