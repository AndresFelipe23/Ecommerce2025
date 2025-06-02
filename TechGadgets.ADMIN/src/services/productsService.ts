import axios, { AxiosError } from 'axios';
import {
  ProductDto,
  ProductFilterDto,
  CreateProductDto,
  UpdateProductDto,
  BulkToggleStatusDto,
  BulkOperationDto,
  ProductSearchResponse,
  CreateProductImageDto,
  ProductImageDto,
  ProductSummaryDto,
} from '../types/products';

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

export interface ApiErrorResponse {
  message?: string;
  errors?: Record<string, string[]>;
  title?: string;
}

export interface PagedResult<T> {
  items: T[];
  totalItems: number;
  page: number;
  totalPages: number;
}

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "http://localhost:5260/api";

// Crear instancia de axios con configuración base
const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json"
  }
});

// Interceptor para agregar token de autenticación
api.interceptors.request.use((config) => {
  let token = localStorage.getItem("authToken");
  
  // Limpiar token si tiene comillas extra
  if (token && (token.startsWith('"') || token.endsWith('"'))) {
    token = token.replace(/^"/, '').replace(/"$/, '');
    localStorage.setItem("authToken", token);
  }
  
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  
  return config;
});

// Interfaces para datos sin normalizar
interface RawImageData {
  Id?: number;
  id?: number;
  Url?: string;
  url?: string;
  AltText?: string;
  altText?: string;
  Orden?: number;
  orden?: number;
  EsPrincipal?: boolean;
  esPrincipal?: boolean;
}

interface RawProductData {
  Id?: number;
  id?: number;
  SKU?: string;
  sku?: string;
  Nombre?: string;
  nombre?: string;
  DescripcionCorta?: string;
  descripcionCorta?: string;
  DescripcionLarga?: string;
  descripcionLarga?: string;
  Slug?: string;
  slug?: string;
  Precio?: number;
  precio?: number;
  PrecioComparacion?: number;
  precioComparacion?: number;
  Costo?: number;
  costo?: number;
  Tipo?: string;
  tipo?: string;
  Estado?: string;
  estado?: string;
  Destacado?: boolean;
  destacado?: boolean;
  Nuevo?: boolean;
  nuevo?: boolean;
  EnOferta?: boolean;
  enOferta?: boolean;
  Peso?: number;
  peso?: number;
  Dimensiones?: string;
  dimensiones?: string;
  MetaTitulo?: string;
  metaTitulo?: string;
  MetaDescripcion?: string;
  metaDescripcion?: string;
  PalabrasClaves?: string;
  palabrasClaves?: string;
  RequiereEnvio?: boolean;
  requiereEnvio?: boolean;
  PermiteReseñas?: boolean;
  permiteReseñas?: boolean;
  Garantia?: string;
  garantia?: string;
  Orden?: number;
  orden?: number;
  Activo?: boolean;
  activo?: boolean;
  FechaCreacion?: string;
  fechaCreacion?: string;
  FechaModificacion?: string;
  fechaModificacion?: string;
  CategoriaId?: number;
  categoriaId?: number;
  CategoriaNombre?: string;
  categoriaNombre?: string;
  CategoriaRuta?: string;
  categoriaRuta?: string;
  MarcaId?: number;
  marcaId?: number;
  MarcaNombre?: string;
  marcaNombre?: string;
  MarcaLogo?: string;
  marcaLogo?: string;
  Imagenes?: RawImageData[];
  imagenes?: RawImageData[];
  ImagenPrincipal?: string;
  imagenPrincipal?: string;
  StockActual?: number;
  stockActual?: number;
  StockReservado?: number;
  stockReservado?: number;
}

interface RawProductSummaryData {
  id?: number;
  Id?: number;
  nombre?: string;
  Nombre?: string;
  slug?: string;
  Slug?: string;
  precio?: number;
  Precio?: number;
  precioOferta?: number;
  PrecioOferta?: number;
  imagenPrincipal?: string;
  ImagenPrincipal?: string;
  activo?: boolean;
  Activo?: boolean;
  destacado?: boolean;
  Destacado?: boolean;
  stock?: number;
  Stock?: number;
  StockActual?: number;
  marcaNombre?: string;
  MarcaNombre?: string;
  categoriaNombre?: string;
  CategoriaNombre?: string;
}

// Funciones de normalización de datos
const normalizeImageData = (images: RawImageData[]): ProductImageDto[] => {
  if (!Array.isArray(images)) return [];
  
  return images.map(img => ({
    Id: img.Id || img.id || 0,
    Url: img.Url || img.url || '',
    AltText: img.AltText || img.altText || '',
    EsPrincipal: img.EsPrincipal || img.esPrincipal || false,
    Orden: img.Orden || img.orden || 0
  }));
};

const normalizeProductData = (data: RawProductData): ProductDto => {
  console.log('🔄 Normalizando datos del producto:', data);
  
  const normalized: ProductDto = {
    Id: data.Id || data.id || 0,
    SKU: data.SKU || data.sku || '',
    Nombre: data.Nombre || data.nombre || '',
    DescripcionCorta: data.DescripcionCorta || data.descripcionCorta,
    DescripcionLarga: data.DescripcionLarga || data.descripcionLarga,
    Slug: data.Slug || data.slug || '',
    Precio: data.Precio || data.precio || 0,
    PrecioComparacion: data.PrecioComparacion || data.precioComparacion,
    Costo: data.Costo || data.costo,
    Tipo: data.Tipo || data.tipo,
    Estado: data.Estado || data.estado,
    Destacado: data.Destacado || data.destacado || false,
    Nuevo: data.Nuevo || data.nuevo || false,
    EnOferta: data.EnOferta || data.enOferta || false,
    Peso: data.Peso || data.peso,
    Dimensiones: data.Dimensiones || data.dimensiones,
    MetaTitulo: data.MetaTitulo || data.metaTitulo,
    MetaDescripcion: data.MetaDescripcion || data.metaDescripcion,
    PalabrasClaves: data.PalabrasClaves || data.palabrasClaves,
    RequiereEnvio: data.RequiereEnvio || data.requiereEnvio || false,
    PermiteReseñas: data.PermiteReseñas || data.permiteReseñas || false,
    Garantia: data.Garantia || data.garantia,
    Orden: data.Orden || data.orden || 0,
    Activo: data.Activo || data.activo || false,
    FechaCreacion: data.FechaCreacion || data.fechaCreacion || new Date().toISOString(),
    FechaModificacion: data.FechaModificacion || data.fechaModificacion,
    
    // Relaciones
    CategoriaId: data.CategoriaId || data.categoriaId || 0,
    CategoriaNombre: data.CategoriaNombre || data.categoriaNombre || '',
    CategoriaRuta: data.CategoriaRuta || data.categoriaRuta || '',
    MarcaId: data.MarcaId || data.marcaId || 0,
    MarcaNombre: data.MarcaNombre || data.marcaNombre || '',
    MarcaLogo: data.MarcaLogo || data.marcaLogo,
    
    // Imágenes
    Imagenes: normalizeImageData(data.Imagenes || data.imagenes || []),
    ImagenPrincipal: data.ImagenPrincipal || data.imagenPrincipal,
    
    // Inventario
    StockActual: data.StockActual || data.stockActual || 0,
    StockReservado: data.StockReservado || data.stockReservado || 0,
  };
  
  console.log('✅ Producto normalizado:', normalized);
  return normalized;
};

const normalizeProductSummaryData = (items: RawProductSummaryData[]): ProductSummaryDto[] => {
  if (!Array.isArray(items)) return [];
  
  return items.map(item => ({
    id: item.id || item.Id || 0,
    nombre: item.nombre || item.Nombre || '',
    slug: item.slug || item.Slug || '',
    precio: item.precio || item.Precio || 0,
    precioOferta: item.precioOferta || item.PrecioOferta,
    imagenPrincipal: item.imagenPrincipal || item.ImagenPrincipal,
    activo: item.activo || item.Activo || false,
    destacado: item.destacado || item.Destacado || false,
    stock: item.stock || item.Stock || item.StockActual || 0,
    marcaNombre: item.marcaNombre || item.MarcaNombre || '',
    categoriaNombre: item.categoriaNombre || item.CategoriaNombre || ''
  }));
};

const productsService = {
  getProducts: async (filters: ProductFilterDto): Promise<ProductSearchResponse> => {
    try {
      const response = await api.get(`/Products/search`, { params: filters });
      const rawData = response.data.data || response.data;
      
      // Normalizar la respuesta de búsqueda
      const normalized: ProductSearchResponse = {
        productos: normalizeProductSummaryData(rawData.productos || rawData.Productos || []),
        totalResultados: rawData.totalResultados || rawData.TotalResultados || 0,
        pagina: rawData.pagina || rawData.Pagina || 1,
        totalPaginas: rawData.totalPaginas || rawData.TotalPaginas || 1,
        filtrosDisponibles: rawData.filtrosDisponibles || rawData.FiltrosDisponibles || {}
      };
      
      return normalized;
    } catch (error) {
      const axiosError = error as AxiosError<ApiResponse<ProductSearchResponse>>;
      throw new Error(axiosError.response?.data?.message || "Error al obtener productos");
    }
  },

  getProductById: async (id: number): Promise<ProductDto> => {
    try {
      console.log(`🔍 Solicitando producto con ID: ${id}`);
      
      const response = await api.get(`/Products/${id}`);
      const rawData = response.data.data || response.data;
      
      console.log('📦 Datos raw recibidos del backend:', rawData);
      
      // Normalizar los datos
      const normalizedData = normalizeProductData(rawData);
      
      console.log('🎯 Datos finales normalizados:', normalizedData);
      
      return normalizedData;
    } catch (error) {
      const axiosError = error as AxiosError<ApiResponse<ProductDto>>;
      console.error('❌ Error al obtener producto:', axiosError.response?.data);
      throw new Error(axiosError.response?.data?.message || "Error al obtener el producto");
    }
  },

  createProduct: async (product: CreateProductDto): Promise<ProductDto> => {
    try {
      console.log('📝 Enviando producto al backend:', product);
      const response = await api.post(`/Products`, product);
      const rawData = response.data.data || response.data;
      console.log('📦 Respuesta del backend:', rawData);
      
      return normalizeProductData(rawData);
    } catch (error) {
      const axiosError = error as AxiosError<ApiErrorResponse>;
      console.error('❌ Error completo del backend:', {
        status: axiosError.response?.status,
        statusText: axiosError.response?.statusText,
        data: axiosError.response?.data,
        message: axiosError.message
      });
      
      // Intentar extraer el mensaje de error más específico
      let errorMessage = "Error al crear el producto";
      
      if (axiosError.response?.data) {
        if (typeof axiosError.response.data === 'string') {
          errorMessage = axiosError.response.data;
        } else if (axiosError.response.data.message) {
          errorMessage = axiosError.response.data.message;
        } else if (axiosError.response.data.errors) {
          // Si hay errores de validación
          const validationErrors = Object.values(axiosError.response.data.errors).flat();
          errorMessage = `Errores de validación: ${validationErrors.join(', ')}`;
        } else if (axiosError.response.data.title) {
          errorMessage = axiosError.response.data.title;
        }
      }
      
      throw new Error(errorMessage);
    }
  },

  addImagesToProduct: async (productId: number, images: CreateProductImageDto[]): Promise<void> => {
    try {
      console.log('📸 Agregando imágenes al producto:', { productId, images });
      
      // Convertir las imágenes a URLs externas para el endpoint
      const externalUrls = images.map(img => img.Url);
      
      const formData = new FormData();
      externalUrls.forEach(url => {
        formData.append('externalUrls', url);
      });
      
      const response = await api.post(`/Products/${productId}/add-images`, formData, {
        headers: {
          'Content-Type': 'multipart/form-data'
        }
      });
      
      console.log('✅ Imágenes agregadas exitosamente:', response.data);
    } catch (error) {
      const axiosError = error as AxiosError<ApiErrorResponse>;
      console.error('❌ Error agregando imágenes:', axiosError.response?.data);
      
      let errorMessage = "Error al agregar imágenes al producto";
      if (axiosError.response?.data?.message) {
        errorMessage = axiosError.response.data.message;
      }
      
      throw new Error(errorMessage);
    }
  },

  update: async (id: number, data: UpdateProductDto): Promise<void> => {
    try {
      console.log('🔄 Actualizando producto:', { id, data });
      const response = await api.put(`/Products/${id}`, data);
      console.log('✅ Respuesta del backend:', response.data);
    } catch (error) {
      const axiosError = error as AxiosError<ApiErrorResponse>;
      console.error('❌ Error al actualizar:', axiosError.response?.data);
      
      let errorMessage = "Error al actualizar el producto";
      if (axiosError.response?.data?.message) {
        errorMessage = axiosError.response.data.message;
      } else if (axiosError.response?.data?.errors) {
        const validationErrors = Object.values(axiosError.response.data.errors).flat();
        errorMessage = `Errores de validación: ${validationErrors.join(', ')}`;
      }
      
      throw new Error(errorMessage);
    }
  },

  deleteProduct: async (id: number): Promise<void> => {
    try {
      await api.delete(`/Products/${id}`);
    } catch (error) {
      const axiosError = error as AxiosError<ApiResponse<void>>;
      throw new Error(axiosError.response?.data?.message || "Error al eliminar el producto");
    }
  },

  toggleProductStatus: async (id: number): Promise<void> => {
    try {
      await api.patch(`/Products/${id}/toggle-status`);
    } catch (error) {
      const axiosError = error as AxiosError<ApiResponse<void>>;
      throw new Error(axiosError.response?.data?.message || "Error al cambiar el estado del producto");
    }
  },

  bulkToggleStatus: async (dto: BulkToggleStatusDto): Promise<void> => {
    try {
      await api.post(`/Products/bulk-toggle-status`, dto);
    } catch (error) {
      const axiosError = error as AxiosError<ApiResponse<void>>;
      throw new Error(axiosError.response?.data?.message || "Error en operación masiva");
    }
  },

  bulkDelete: async (dto: BulkOperationDto): Promise<void> => {
    try {
      await api.post(`/Products/bulk-delete`, dto);
    } catch (error) {
      const axiosError = error as AxiosError<ApiResponse<void>>;
      throw new Error(axiosError.response?.data?.message || "Error al eliminar productos");
    }
  },

  getAllProducts: async (): Promise<ProductDto[]> => {
    try {
      const response = await api.get(`/Products`);
      const rawData = response.data.data || response.data;
      
      if (Array.isArray(rawData)) {
        return rawData.map(normalizeProductData);
      }
      
      return rawData ? [normalizeProductData(rawData)] : [];
    } catch (error) {
      const axiosError = error as AxiosError<ApiResponse<ProductDto[]>>;
      throw new Error(axiosError.response?.data?.message || "Error al obtener todos los productos");
    }
  },
};

export default productsService;