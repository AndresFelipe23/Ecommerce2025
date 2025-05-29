import { useState, useEffect } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { ChevronLeftIcon, PlusIcon } from "../../icons";

import { useAuth } from "../../context/AuthContext";
import { usePermissions } from "../../hooks/usePermissions";
import { PERMISSIONS } from "../../types/permissions";
import productsService from "../../services/productsService";

import brandsService from "../../services/brandsService";
import { 
  CreateProductDto, 
  UpdateProductDto, 
  CreateProductImageDto, 
  UpdateProductImageDto 
} from "../../types/products";
import { CategoryDto } from "../../types/categories";
import categoriesService from "../../services/categoryService";
import Label from "../../components/form/Label";
import Input from "../../components/form/input/InputField";
import Button from "../../components/ui/button/Button";
import Select from "../../components/form/Select";
import { BrandDto } from "../../types/brands";

interface ProductFormData {
  SKU: string;
  Nombre: string;
  DescripcionCorta: string;
  DescripcionLarga: string;
  Precio: number;
  PrecioComparacion: number | null;
  Costo: number | null;
  CategoriaId: number | null;
  MarcaId: number | null;
  Tipo: string;
  Estado: string;
  Destacado: boolean;
  Nuevo: boolean;
  EnOferta: boolean;
  Peso: number | null;
  Dimensiones: string;
  MetaTitulo: string;
  MetaDescripcion: string;
  PalabrasClaves: string;
  RequiereEnvio: boolean;
  PermiteReseñas: boolean;
  Garantia: string;
  Orden: number;
  StockInicial: number;
  Activo: boolean;
  imagenes: CreateProductImageDto[];
}

const initialFormData: ProductFormData = {
  SKU: "",
  Nombre: "",
  DescripcionCorta: "",
  DescripcionLarga: "",
  Precio: 0,
  PrecioComparacion: null,
  Costo: null,
  CategoriaId: null,
  MarcaId: null,
  Tipo: "Simple",
  Estado: "Borrador",
  Destacado: false,
  Nuevo: false,
  EnOferta: false,
  Peso: null,
  Dimensiones: "",
  MetaTitulo: "",
  MetaDescripcion: "",
  PalabrasClaves: "",
  RequiereEnvio: true,
  PermiteReseñas: true,
  Garantia: "",
  Orden: 0,
  StockInicial: 0,
  Activo: true,
  imagenes: []
};

const Textarea = ({ id, name, value, onChange, disabled, rows, placeholder, className }: {
  id: string;
  name: string;
  value: string;
  onChange: (e: React.ChangeEvent<HTMLTextAreaElement>) => void;
  disabled?: boolean;
  rows?: number;
  placeholder?: string;
  className?: string;
}) => (
  <textarea
    id={id}
    name={name}
    value={value}
    onChange={onChange}
    disabled={disabled}
    rows={rows}
    placeholder={placeholder}
    className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 ${
      disabled ? 'bg-gray-100 cursor-not-allowed' : 'bg-white'
    } ${className || ''}`}
  />
);

export default function ProductForm() {
  const [formData, setFormData] = useState<ProductFormData>(initialFormData);
  const [errors, setErrors] = useState<Record<string, string[]>>({});
  const [isLoading, setIsLoading] = useState(false);
  const [isLoadingData, setIsLoadingData] = useState(false);
  const [generalError, setGeneralError] = useState("");
  const [successMessage, setSuccessMessage] = useState("");
  
  // Datos para selects
  const [categories, setCategories] = useState<CategoryDto[]>([]);
  const [brands, setBrands] = useState<BrandDto[]>([]);
  const [loadingCategories, setLoadingCategories] = useState(true);
  const [loadingBrands, setLoadingBrands] = useState(true);
  
  // Image input
  const [imageInput, setImageInput] = useState("");

  useAuth();
  const { hasPermission } = usePermissions();
  const navigate = useNavigate();
  const { id } = useParams();
  
  const isEditMode = !!id;

  // Verificar permisos
  const canCreate = hasPermission(PERMISSIONS.PRODUCTS.CREATE);
  const canEdit = hasPermission(PERMISSIONS.PRODUCTS.EDIT);
  const hasRequiredPermission = isEditMode ? canEdit : canCreate;

  useEffect(() => {
    if (!hasRequiredPermission) {
      navigate('/unauthorized');
      return;
    }

    loadInitialData();
    
    if (isEditMode) {
      loadProduct();
    }
  }, [id, hasRequiredPermission]);

  const loadInitialData = async () => {
    await Promise.all([
      loadCategories(),
      loadBrands()
    ]);
  };

  const loadCategories = async () => {
    try {
      setLoadingCategories(true);
      const response = await categoriesService.getCategories({
        page: 1,
        pageSize: 100,
        sortBy: 'nombre',
        activo: true
      });
      setCategories(response.items || []);
    } catch (error) {
      console.error('Error cargando categorías:', error);
    } finally {
      setLoadingCategories(false);
    }
  };

  const loadBrands = async () => {
    try {
      setLoadingBrands(true);
      const response = await brandsService.getBrands({
        page: 1,
        pageSize: 100,
        sortBy: 'nombre',
        activo: true
      });
      setBrands(response.items || []);
    } catch (error) {
      console.error('Error cargando marcas:', error);
    } finally {
      setLoadingBrands(false);
    }
  };

  const loadProduct = async () => {
    if (!id) return;
    
    try {
      setIsLoadingData(true);
      const product = await productsService.getProduct(parseInt(id));
      
      setFormData({
        SKU: product.SKU,
        Nombre: product.Nombre,
        DescripcionCorta: product.DescripcionCorta || "",
        DescripcionLarga: product.DescripcionLarga || "",
        Precio: product.Precio,
        PrecioComparacion: product.PrecioComparacion || null,
        Costo: product.Costo || null,
        CategoriaId: product.CategoriaId,
        MarcaId: product.MarcaId,
        Tipo: product.Tipo || "Simple",
        Estado: product.Estado || "Borrador",
        Destacado: product.Destacado,
        Nuevo: product.Nuevo,
        EnOferta: product.EnOferta,
        Peso: product.Peso || null,
        Dimensiones: product.Dimensiones || "",
        MetaTitulo: product.MetaTitulo || "",
        MetaDescripcion: product.MetaDescripcion || "",
        PalabrasClaves: product.PalabrasClaves || "",
        RequiereEnvio: product.RequiereEnvio,
        PermiteReseñas: product.PermiteReseñas,
        Garantia: product.Garantia || "",
        Orden: product.Orden,
        StockInicial: product.StockActual,
        Activo: product.Activo,
        imagenes: product.Imagenes?.map(img => ({
          Url: img.Url,
          AltText: img.AltText || "",
          EsPrincipal: img.EsPrincipal,
          Orden: img.Orden
        })) || []
      });

    } catch (error) {
      console.error('Error cargando producto:', error);
      setGeneralError('Error al cargar el producto');
    } finally {
      setIsLoadingData(false);
    }
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;
    
    let processedValue: string | number | boolean | null = value;
    
    if (type === 'number') {
      processedValue = value === '' ? 0 : parseFloat(value);
    } else if (type === 'checkbox') {
      processedValue = (e.target as HTMLInputElement).checked;
    } else if (name === 'CategoriaId' || name === 'MarcaId') {
      processedValue = value === '' ? null : parseInt(value);
    }

    setFormData(prev => ({
      ...prev,
      [name]: processedValue
    }));

    // Clear errors for this field
    if (errors[name]) {
      setErrors(prev => ({
        ...prev,
        [name]: []
      }));
    }

    if (generalError) setGeneralError("");
    if (successMessage) setSuccessMessage("");
  };

  const generateSKU = () => {
    const timestamp = Date.now().toString().slice(-6);
    const randomNum = Math.floor(Math.random() * 100).toString().padStart(2, '0');
    const sku = `PRD-${timestamp}-${randomNum}`;
    
    setFormData(prev => ({
      ...prev,
      SKU: sku
    }));
  };

  const handleAddImage = () => {
    if (imageInput.trim() && !formData.imagenes.some(img => img.Url === imageInput.trim())) {
      const newImage: CreateProductImageDto = {
        Url: imageInput.trim(),
        AltText: formData.Nombre || "Imagen del producto",
        EsPrincipal: formData.imagenes.length === 0, // Primera imagen es principal
        Orden: formData.imagenes.length + 1
      };

      setFormData(prev => ({
        ...prev,
        imagenes: [...prev.imagenes, newImage]
      }));
      setImageInput("");
    }
  };

  const handleRemoveImage = (imageToRemove: string) => {
    setFormData(prev => ({
      ...prev,
      imagenes: prev.imagenes.filter(img => img.Url !== imageToRemove)
        .map((img, index) => ({
          ...img,
          Orden: index + 1,
          EsPrincipal: index === 0 // La primera imagen siempre es principal
        }))
    }));
  };

  const handleSetMainImage = (imageUrl: string) => {
    setFormData(prev => ({
      ...prev,
      imagenes: prev.imagenes.map(img => ({
        ...img,
        EsPrincipal: img.Url === imageUrl
      }))
    }));
  };

  const validateForm = (): boolean => {
    const newErrors: Record<string, string[]> = {};

    // Validaciones requeridas
    if (!formData.Nombre.trim()) {
      newErrors.Nombre = ["El nombre es requerido"];
    }

    if (!formData.SKU.trim()) {
      newErrors.SKU = ["El SKU es requerido"];
    }

    if (formData.Precio <= 0) {
      newErrors.Precio = ["El precio debe ser mayor a 0"];
    }

    if (formData.StockInicial < 0) {
      newErrors.StockInicial = ["El stock inicial no puede ser negativo"];
    }

    if (formData.PrecioComparacion && formData.PrecioComparacion <= formData.Precio) {
      newErrors.PrecioComparacion = ["El precio de comparación debe ser mayor al precio regular"];
    }

    if (!formData.CategoriaId) {
      newErrors.CategoriaId = ["La categoría es requerida"];
    }

    if (!formData.MarcaId) {
      newErrors.MarcaId = ["La marca es requerida"];
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }

    setIsLoading(true);
    setGeneralError("");
    setSuccessMessage("");

    try {
      if (isEditMode) {
        const updateData: UpdateProductDto = {
          ...formData,
          CategoriaId: formData.CategoriaId!,
          MarcaId: formData.MarcaId!,
          PrecioComparacion: formData.PrecioComparacion || undefined,
          Costo: formData.Costo || undefined,
          Peso: formData.Peso || undefined,
          Imagenes: formData.imagenes.map((img) => ({
            ...img,
            Id: undefined,
            Eliminar: false
          } as UpdateProductImageDto))
        };
        await productsService.updateProduct(parseInt(id!), updateData);
      } else {
        const createData: CreateProductDto = {
          ...formData,
          CategoriaId: formData.CategoriaId!,
          MarcaId: formData.MarcaId!,
          PrecioComparacion: formData.PrecioComparacion || undefined,
          Costo: formData.Costo || undefined,
          Peso: formData.Peso || undefined,
          Imagenes: formData.imagenes
        };
        await productsService.createProduct(createData);
      }

      setSuccessMessage(isEditMode ? 'Producto actualizado exitosamente' : 'Producto creado exitosamente');
      setTimeout(() => {
        navigate('/products');
      }, 1500);

    } catch (error: unknown) {
      console.error('Error al guardar producto:', error);
      const errorMessage = error instanceof Error ? error.message : 'Error de conexión con el servidor';
      setGeneralError(errorMessage);
    } finally {
      setIsLoading(false);
    }
  };

  const handleSelectChange = (name: string, value: string) => {
    setFormData(prev => ({
      ...prev,
      [name]: value === '' ? null : value
    }));

    if (errors[name]) {
      setErrors(prev => ({
        ...prev,
        [name]: []
      }));
    }

    if (generalError) setGeneralError("");
    if (successMessage) setSuccessMessage("");
  };

  if (!hasRequiredPermission) {
    return null;
  }

  if (isLoadingData) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <div className="w-8 h-8 border-2 border-blue-600 border-t-transparent rounded-full animate-spin mx-auto mb-4"></div>
          <p className="text-gray-600 dark:text-gray-400">Cargando producto...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <div className="flex items-center gap-2 mb-2">
            <Link
              to="/products"
              className="inline-flex items-center text-sm text-gray-500 transition-colors hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300"
            >
              <ChevronLeftIcon className="size-4 mr-1" />
              Volver a productos
            </Link>
          </div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white">
            {isEditMode ? 'Editar Producto' : 'Crear Producto'}
          </h1>
        </div>
      </div>

      {/* Messages */}
      {generalError && (
        <div className="p-4 text-sm text-red-700 bg-red-100 border border-red-200 rounded-md dark:bg-red-900/20 dark:text-red-400 dark:border-red-800">
          {generalError}
        </div>
      )}

      {successMessage && (
        <div className="p-4 text-sm text-green-700 bg-green-100 border border-green-200 rounded-md dark:bg-green-900/20 dark:text-green-400 dark:border-green-800">
          {successMessage}
        </div>
      )}

      {/* Form */}
      <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700">
        <form onSubmit={handleSubmit} className="p-6 space-y-6">
          {/* Información básica */}
          <div>
            <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-4">
              Información básica
            </h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <Label htmlFor="Nombre">
                  Nombre del Producto <span className="text-red-500">*</span>
                </Label>
                <Input
                  id="Nombre"
                  name="Nombre"
                  type="text"
                  value={formData.Nombre}
                  onChange={handleInputChange}
                  disabled={isLoading}
                  className={errors.Nombre ? "border-red-500" : ""}
                />
                {errors.Nombre && (
                  <p className="mt-1 text-sm text-red-600 dark:text-red-400">
                    {errors.Nombre[0]}
                  </p>
                )}
              </div>

              <div>
                <Label htmlFor="SKU">
                  SKU <span className="text-red-500">*</span>
                </Label>
                <div className="flex gap-2">
                  <Input
                    id="SKU"
                    name="SKU"
                    type="text"
                    value={formData.SKU}
                    onChange={handleInputChange}
                    disabled={isLoading}
                    className={errors.SKU ? "border-red-500" : ""}
                  />
                  <Button
                    onClick={generateSKU}
                    disabled={isLoading}
                    variant="outline"
                    size="sm"
                  >
                    Generar
                  </Button>
                </div>
                {errors.SKU && (
                  <p className="mt-1 text-sm text-red-600 dark:text-red-400">
                    {errors.SKU[0]}
                  </p>
                )}
              </div>

              <div className="md:col-span-2">
                <Label htmlFor="DescripcionCorta">Descripción Corta</Label>
                <Textarea
                  id="DescripcionCorta"
                  name="DescripcionCorta"
                  value={formData.DescripcionCorta}
                  onChange={handleInputChange}
                  disabled={isLoading}
                  rows={2}
                />
              </div>

              <div className="md:col-span-2">
                <Label htmlFor="DescripcionLarga">Descripción Detallada</Label>
                <Textarea
                  id="DescripcionLarga"
                  name="DescripcionLarga"
                  value={formData.DescripcionLarga}
                  onChange={handleInputChange}
                  disabled={isLoading}
                  rows={4}
                />
              </div>
            </div>
          </div>

          {/* Precios */}
          <div>
            <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-4">
              Precios
            </h3>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div>
                <Label htmlFor="Precio">
                  Precio de Venta <span className="text-red-500">*</span>
                </Label>
                <Input
                  id="Precio"
                  name="Precio"
                  type="number"
                  step={0.01}
                  min={0}
                  value={formData.Precio.toString()}
                  onChange={handleInputChange}
                  disabled={isLoading}
                  className={errors.Precio ? "border-red-500" : ""}
                />
                {errors.Precio && (
                  <p className="mt-1 text-sm text-red-600 dark:text-red-400">
                    {errors.Precio[0]}
                  </p>
                )}
              </div>

              <div>
                <Label htmlFor="PrecioComparacion">Precio de Comparación</Label>
                <Input
                  id="PrecioComparacion"
                  name="PrecioComparacion"
                  type="number"
                  step={0.01}
                  min={0}
                  value={formData.PrecioComparacion?.toString() || ''}
                  onChange={handleInputChange}
                  disabled={isLoading}
                  className={errors.PrecioComparacion ? "border-red-500" : ""}
                />
                {errors.PrecioComparacion && (
                  <p className="mt-1 text-sm text-red-600 dark:text-red-400">
                    {errors.PrecioComparacion[0]}
                  </p>
                )}
              </div>

              <div>
                <Label htmlFor="Costo">Costo del Producto</Label>
                <Input
                  id="Costo"
                  name="Costo"
                  type="number"
                  step={0.01}
                  min={0}
                  value={formData.Costo?.toString() || ''}
                  onChange={handleInputChange}
                  disabled={isLoading}
                />
              </div>
            </div>
          </div>

          {/* Inventario */}
          <div>
            <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-4">
              Inventario
            </h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <Label htmlFor="StockInicial">
                  Stock Inicial <span className="text-red-500">*</span>
                </Label>
                <Input
                  id="StockInicial"
                  name="StockInicial"
                  type="number"
                  min="0"
                  value={formData.StockInicial}
                  onChange={handleInputChange}
                  disabled={isLoading}
                  className={errors.StockInicial ? "border-red-500" : ""}
                />
                {errors.StockInicial && (
                  <p className="mt-1 text-sm text-red-600 dark:text-red-400">
                    {errors.StockInicial[0]}
                  </p>
                )}
              </div>

              <div>
                <Label htmlFor="Peso">Peso (kg)</Label>
                <Input
                  id="Peso"
                  name="Peso"
                  type="number"
                  step={0.01}
                  min={0}
                  value={formData.Peso?.toString() || ''}
                  onChange={handleInputChange}
                  disabled={isLoading}
                />
              </div>
            </div>
          </div>

          {/* Categorización */}
          <div>
            <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-4">
              Categorización
            </h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <Label htmlFor="CategoriaId">
                  Categoría <span className="text-red-500">*</span>
                </Label>
                <Select
                  id="CategoriaId"
                  name="CategoriaId"
                  value={formData.CategoriaId || ''}
                  onChange={(value) => handleSelectChange('CategoriaId', value)}
                  disabled={isLoading || loadingCategories}
                  className={errors.CategoriaId ? "border-red-500" : ""}
                  options={[
                    { value: '', label: 'Seleccionar categoría' },
                    ...categories.map(category => ({
                      value: category.id.toString(),
                      label: category.nombre
                    }))
                  ]}
                />
                {errors.CategoriaId && (
                  <p className="mt-1 text-sm text-red-600 dark:text-red-400">
                    {errors.CategoriaId[0]}
                  </p>
                )}
              </div>

              <div>
                <Label htmlFor="MarcaId">
                  Marca <span className="text-red-500">*</span>
                </Label>
                <Select
                  id="MarcaId"
                  name="MarcaId"
                  value={formData.MarcaId || ''}
                  onChange={(value) => handleSelectChange('MarcaId', value)}
                  disabled={isLoading || loadingBrands}
                  className={errors.MarcaId ? "border-red-500" : ""}
                  options={[
                    { value: '', label: 'Seleccionar marca' },
                    ...brands.map(brand => ({
                      value: brand.id.toString(),
                      label: brand.nombre
                    }))
                  ]}
                />
                {errors.MarcaId && (
                  <p className="mt-1 text-sm text-red-600 dark:text-red-400">
                    {errors.MarcaId[0]}
                  </p>
                )}
              </div>
            </div>
          </div>

          {/* Estado y configuración */}
          <div>
            <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-4">
              Estado y configuración
            </h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <Label htmlFor="Estado">Estado</Label>
                <Select
                  id="Estado"
                  name="Estado"
                  value={formData.Estado}
                  onChange={(value) => handleSelectChange('Estado', value)}
                  disabled={isLoading}
                  options={[
                    { value: 'Borrador', label: 'Borrador' },
                    { value: 'Publicado', label: 'Publicado' },
                    { value: 'Agotado', label: 'Agotado' }
                  ]}
                />
              </div>

              <div>
                <Label htmlFor="Tipo">Tipo de Producto</Label>
                <Select
                  id="Tipo"
                  name="Tipo"
                  value={formData.Tipo}
                  onChange={(value) => handleSelectChange('Tipo', value)}
                  disabled={isLoading}
                  options={[
                    { value: 'Simple', label: 'Simple' },
                    { value: 'Variable', label: 'Variable' },
                    { value: 'Digital', label: 'Digital' }
                  ]}
                />
              </div>

              <div className="md:col-span-2 grid grid-cols-2 md:grid-cols-4 gap-4">
                <div className="flex items-center">
                  <input
                    id="Destacado"
                    name="Destacado"
                    type="checkbox"
                    checked={formData.Destacado}
                    onChange={handleInputChange}
                    disabled={isLoading}
                    className="w-4 h-4 text-blue-600 bg-gray-100 border-gray-300 rounded focus:ring-blue-500"
                  />
                  <Label htmlFor="Destacado" className="ml-2 cursor-pointer">
                    Destacado
                  </Label>
                </div>

                <div className="flex items-center">
                  <input
                    id="Nuevo"
                    name="Nuevo"
                    type="checkbox"
                    checked={formData.Nuevo}
                    onChange={handleInputChange}
                    disabled={isLoading}
                    className="w-4 h-4 text-blue-600 bg-gray-100 border-gray-300 rounded focus:ring-blue-500"
                  />
                  <Label htmlFor="Nuevo" className="ml-2 cursor-pointer">
                    Nuevo
                  </Label>
                </div>

                <div className="flex items-center">
                  <input
                    id="EnOferta"
                    name="EnOferta"
                    type="checkbox"
                    checked={formData.EnOferta}
                    onChange={handleInputChange}
                    disabled={isLoading}
                    className="w-4 h-4 text-blue-600 bg-gray-100 border-gray-300 rounded focus:ring-blue-500"
                  />
                  <Label htmlFor="EnOferta" className="ml-2 cursor-pointer">
                    En Oferta
                  </Label>
                </div>

                <div className="flex items-center">
                  <input
                    id="Activo"
                    name="Activo"
                    type="checkbox"
                    checked={formData.Activo}
                    onChange={handleInputChange}
                    disabled={isLoading}
                    className="w-4 h-4 text-blue-600 bg-gray-100 border-gray-300 rounded focus:ring-blue-500"
                  />
                  <Label htmlFor="Activo" className="ml-2 cursor-pointer">
                    Activo
                  </Label>
                </div>
              </div>
            </div>
          </div>

          {/* Imágenes */}
          <div>
            <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-4">
              Imágenes del Producto
            </h3>
            <div className="space-y-3">
              <div className="flex gap-2">
                <Input
                  type="url"
                  placeholder="URL de la imagen"
                  value={imageInput}
                  onChange={(e) => setImageInput(e.target.value)}
                  onKeyDown={(e) => e.key === 'Enter' && (e.preventDefault(), handleAddImage())}
                  disabled={isLoading}
                />
                <Button
                  onClick={handleAddImage}
                  disabled={isLoading || !imageInput.trim()}
                  variant="outline"
                  size="sm"
                >
                  <PlusIcon className="size-4" />
                </Button>
              </div>
              {formData.imagenes.length > 0 && (
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                  {formData.imagenes.map((image, index) => (
                    <div key={index} className="relative group">
                      <img
                        src={image.Url}
                        alt={image.AltText || `Producto ${index + 1}`}
                        className={`w-full h-24 object-cover rounded-lg border-2 ${
                          image.EsPrincipal 
                            ? 'border-blue-500' 
                            : 'border-gray-200 dark:border-gray-600'
                        }`}
                        onError={(e) => {
                          (e.target as HTMLImageElement).src = '/images/placeholder-image.jpg';
                        }}
                      />
                      {image.EsPrincipal && (
                        <span className="absolute top-1 left-1 bg-blue-500 text-white text-xs px-2 py-1 rounded">
                          Principal
                        </span>
                      )}
                      <div className="absolute top-1 right-1 flex gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                        {!image.EsPrincipal && (
                          <button
                            type="button"
                            onClick={() => handleSetMainImage(image.Url)}
                            disabled={isLoading}
                            className="bg-green-500 hover:bg-green-600 text-white rounded-full p-1 text-xs"
                            title="Establecer como imagen principal"
                          >
                            ★
                          </button>
                        )}
                        <button
                          type="button"
                          onClick={() => handleRemoveImage(image.Url)}
                          disabled={isLoading}
                          className="bg-red-500 hover:bg-red-600 text-white rounded-full p-1"
                        >
                          ×
                        </button>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>

          {/* SEO y Metadatos */}
          <div>
            <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-4">
              SEO y Metadatos
            </h3>
            <div className="space-y-4">
              <div>
                <Label htmlFor="MetaTitulo">Meta Título</Label>
                <Input
                  id="MetaTitulo"
                  name="MetaTitulo"
                  type="text"
                  value={formData.MetaTitulo}
                  onChange={handleInputChange}
                  disabled={isLoading}
                  placeholder="Título para SEO (máx. 60 caracteres)"
                />
                <p className="text-xs text-gray-500 mt-1">
                  {formData.MetaTitulo.length}/60 caracteres
                </p>
              </div>

              <div>
                <Label htmlFor="MetaDescripcion">Meta Descripción</Label>
                <Textarea
                  id="MetaDescripcion"
                  name="MetaDescripcion"
                  value={formData.MetaDescripcion}
                  onChange={handleInputChange}
                  disabled={isLoading}
                  rows={3}
                  placeholder="Descripción para SEO (máx. 160 caracteres)"
                />
                <p className="text-xs text-gray-500 mt-1">
                  {formData.MetaDescripcion.length}/160 caracteres
                </p>
              </div>

              <div>
                <Label htmlFor="PalabrasClaves">Palabras Clave</Label>
                <Input
                  id="PalabrasClaves"
                  name="PalabrasClaves"
                  type="text"
                  value={formData.PalabrasClaves}
                  onChange={handleInputChange}
                  disabled={isLoading}
                  placeholder="palabra1, palabra2, palabra3"
                />
                <p className="text-xs text-gray-500 mt-1">
                  Separar con comas
                </p>
              </div>
            </div>
          </div>

          {/* Configuraciones adicionales */}
          <div>
            <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-4">
              Configuraciones Adicionales
            </h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <Label htmlFor="Dimensiones">Dimensiones</Label>
                <Input
                  id="Dimensiones"
                  name="Dimensiones"
                  type="text"
                  value={formData.Dimensiones}
                  onChange={handleInputChange}
                  disabled={isLoading}
                  placeholder="Ej: 10cm x 15cm x 5cm"
                />
              </div>

              <div>
                <Label htmlFor="Garantia">Garantía</Label>
                <Input
                  id="Garantia"
                  name="Garantia"
                  type="text"
                  value={formData.Garantia}
                  onChange={handleInputChange}
                  disabled={isLoading}
                  placeholder="Ej: 12 meses"
                />
              </div>

              <div>
                <Label htmlFor="Orden">Orden de Visualización</Label>
                <Input
                  id="Orden"
                  name="Orden"
                  type="number"
                  min="0"
                  value={formData.Orden}
                  onChange={handleInputChange}
                  disabled={isLoading}
                />
              </div>

              <div className="space-y-3">
                <div className="flex items-center">
                  <input
                    id="RequiereEnvio"
                    name="RequiereEnvio"
                    type="checkbox"
                    checked={formData.RequiereEnvio}
                    onChange={handleInputChange}
                    disabled={isLoading}
                    className="w-4 h-4 text-blue-600 bg-gray-100 border-gray-300 rounded focus:ring-blue-500"
                  />
                  <Label htmlFor="RequiereEnvio" className="ml-2 cursor-pointer">
                    Requiere Envío
                  </Label>
                </div>

                <div className="flex items-center">
                  <input
                    id="PermiteReseñas"
                    name="PermiteReseñas"
                    type="checkbox"
                    checked={formData.PermiteReseñas}
                    onChange={handleInputChange}
                    disabled={isLoading}
                    className="w-4 h-4 text-blue-600 bg-gray-100 border-gray-300 rounded focus:ring-blue-500"
                  />
                  <Label htmlFor="PermiteReseñas" className="ml-2 cursor-pointer">
                    Permite Reseñas
                  </Label>
                </div>
              </div>
            </div>
          </div>

          {/* Buttons */}
          <div className="flex justify-end gap-3 pt-6 border-t border-gray-200 dark:border-gray-700">
            <Button
              onClick={() => navigate('/products')}
              disabled={isLoading}
              variant="outline"
            >
              Cancelar
            </Button>
            <Button
              onClick={handleSubmit}
              disabled={isLoading}
            >
              {isLoading ? (
                <div className="flex items-center gap-2">
                  <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"></div>
                  {isEditMode ? 'Actualizando...' : 'Creando...'}
                </div>
              ) : (
                isEditMode ? 'Actualizar Producto' : 'Crear Producto'
              )}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
}