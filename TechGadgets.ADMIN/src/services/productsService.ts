import axios, { AxiosError, AxiosResponse } from "axios";
import {
  ProductDto,
  ProductFilterDto,
  CreateProductDto,
  UpdateProductDto,
  ProductSummaryDto,
  ProductStatsDto,
  ProductSearchResultDto,
  ProductSearchFiltersDto,
  AdjustStockDto,
  UpdateStockRequest,
  BulkToggleStatusDto,
  BulkPriceUpdateDto,
  BulkOperationDto
} from "../types/products";

export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
  errors?: Record<string, string[]>;
}

export interface PagedResult<T> {
  items: T[];
  totalItems: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "http://localhost:5260/api";

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json"
  }
});

// Interceptor de request con limpieza de token
api.interceptors.request.use((config) => {
  let token = localStorage.getItem("authToken");
  
  // Limpiar token si tiene comillas extra
  if (token && (token.startsWith('"') || token.endsWith('"'))) {
    token = token.replace(/^"/, '').replace(/"$/, '');
    localStorage.setItem("authToken", token);
    console.log('🧹 Token cleaned in interceptor');
  }
  
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  
  return config;
});

// Interceptor de respuesta para debug - ✅ MEJORADO
api.interceptors.response.use(
  (response) => {
    // ✅ DEBUG: Log detallado de respuestas
    console.log('=== API RESPONSE SUCCESS ===');
    console.log('URL:', response.config.url);
    console.log('Status:', response.status);
    console.log('Headers:', response.headers);
    console.log('Data Structure:', {
      success: response.data?.success,
      hasData: !!response.data?.data,
      dataType: typeof response.data?.data,
      dataKeys: response.data?.data ? Object.keys(response.data.data) : 'No data'
    });
    
    // ✅ IMPORTANTE: Log específico para productos individuales
    if (response.config.url?.includes('/products/') && response.config.url?.match(/\/products\/\d+$/)) {
      console.log('📦 PRODUCTO INDIVIDUAL - Respuesta detallada:', {
        hasData: !!response.data?.data,
        productData: response.data?.data,
        rawKeys: response.data?.data ? Object.keys(response.data.data) : 'No data'
      });
    }
    
    // ✅ IMPORTANTE: Log específico para productos lista
    if (response.config.url?.includes('/products') && response.data?.data?.items) {
      console.log('📦 PRODUCTOS LISTA - Respuesta detallada:', {
        totalItems: response.data.data.totalItems,
        itemsCount: response.data.data.items?.length,
        firstItem: response.data.data.items?.[0],
        sampleProperties: response.data.data.items?.[0] ? Object.keys(response.data.data.items[0]) : 'No items'
      });
    }
    
    return response;
  },
  (error) => {
    console.log('=== API RESPONSE ERROR ===');
    console.log('URL:', error.config?.url);
    console.log('Status:', error.response?.status);
    console.log('Message:', error.response?.data?.message);
    console.log('Full error data:', error.response?.data);
    console.log('Request data:', error.config?.data);
    return Promise.reject(error);
  }
);

// ✅ FUNCIÓN PARA MAPEAR UN PRODUCTO INDIVIDUAL (REUTILIZABLE)
const mapSingleProduct = (rawProduct: any): ProductDto => {
  console.log('🔄 === MAPEANDO PRODUCTO INDIVIDUAL ===');
  console.log('Raw product recibido:', rawProduct);
  console.log('Raw product keys:', Object.keys(rawProduct || {}));
  
  if (!rawProduct) {
    console.warn('⚠️ Raw product es null/undefined');
    throw new Error('Producto no encontrado');
  }
  
  // ✅ ESTRATEGIA ADAPTATIVA DE MAPEO - MISMA QUE EN getProducts
  const mappedProduct: ProductDto = {
    // Probar múltiples estrategias para cada propiedad
    Id: rawProduct.Id ?? rawProduct.PrdId ?? rawProduct.id ?? rawProduct.prdId ?? 0,
    SKU: rawProduct.SKU ?? rawProduct.PrdSku ?? rawProduct.sku ?? rawProduct.prdSku ?? 'Sin SKU',
    Nombre: rawProduct.Nombre ?? rawProduct.PrdNombre ?? rawProduct.nombre ?? rawProduct.prdNombre ?? 'Sin nombre',
    DescripcionCorta: rawProduct.DescripcionCorta ?? rawProduct.PrdDescripcionCorta ?? rawProduct.descripcionCorta ?? rawProduct.prdDescripcionCorta,
    DescripcionLarga: rawProduct.DescripcionLarga ?? rawProduct.PrdDescripcionLarga ?? rawProduct.descripcionLarga ?? rawProduct.prdDescripcionLarga,
    Slug: rawProduct.Slug ?? rawProduct.PrdSlug ?? rawProduct.slug ?? rawProduct.prdSlug ?? '',
    Precio: rawProduct.Precio ?? rawProduct.PrdPrecio ?? rawProduct.precio ?? rawProduct.prdPrecio ?? 0,
    PrecioComparacion: rawProduct.PrecioComparacion ?? rawProduct.PrdPrecioComparacion ?? rawProduct.precioComparacion ?? rawProduct.prdPrecioComparacion,
    Costo: rawProduct.Costo ?? rawProduct.PrdCosto ?? rawProduct.costo ?? rawProduct.prdCosto,
    Tipo: rawProduct.Tipo ?? rawProduct.PrdTipo ?? rawProduct.tipo ?? rawProduct.prdTipo ?? 'simple',
    Estado: rawProduct.Estado ?? rawProduct.PrdEstado ?? rawProduct.estado ?? rawProduct.prdEstado ?? 'disponible',
    Destacado: rawProduct.Destacado ?? rawProduct.PrdDestacado ?? rawProduct.destacado ?? rawProduct.prdDestacado ?? false,
    Nuevo: rawProduct.Nuevo ?? rawProduct.PrdNuevo ?? rawProduct.nuevo ?? rawProduct.prdNuevo ?? false,
    EnOferta: rawProduct.EnOferta ?? rawProduct.PrdEnOferta ?? rawProduct.enOferta ?? rawProduct.prdEnOferta ?? false,
    Peso: rawProduct.Peso ?? rawProduct.PrdPeso ?? rawProduct.peso ?? rawProduct.prdPeso,
    Dimensiones: rawProduct.Dimensiones ?? rawProduct.PrdDimensiones ?? rawProduct.dimensiones ?? rawProduct.prdDimensiones,
    MetaTitulo: rawProduct.MetaTitulo ?? rawProduct.PrdMetaTitulo ?? rawProduct.metaTitulo ?? rawProduct.prdMetaTitulo,
    MetaDescripcion: rawProduct.MetaDescripcion ?? rawProduct.PrdMetaDescripcion ?? rawProduct.metaDescripcion ?? rawProduct.prdMetaDescripcion,
    PalabrasClaves: rawProduct.PalabrasClaves ?? rawProduct.PrdPalabrasClaves ?? rawProduct.palabrasClaves ?? rawProduct.prdPalabrasClaves,
    RequiereEnvio: rawProduct.RequiereEnvio ?? rawProduct.PrdRequiereEnvio ?? rawProduct.requiereEnvio ?? rawProduct.prdRequiereEnvio ?? true,
    PermiteReseñas: rawProduct.PermiteReseñas ?? rawProduct.PrdPermiteReseñas ?? rawProduct.permiteReseñas ?? rawProduct.prdPermiteReseñas ?? true,
    Garantia: rawProduct.Garantia ?? rawProduct.PrdGarantia ?? rawProduct.garantia ?? rawProduct.prdGarantia,
    Orden: rawProduct.Orden ?? rawProduct.PrdOrden ?? rawProduct.orden ?? rawProduct.prdOrden ?? 0,
    Activo: rawProduct.Activo ?? rawProduct.PrdActivo ?? rawProduct.activo ?? rawProduct.prdActivo ?? false,
    FechaCreacion: rawProduct.FechaCreacion ?? rawProduct.PrdFechaCreacion ?? rawProduct.fechaCreacion ?? rawProduct.prdFechaCreacion ?? '',
    FechaModificacion: rawProduct.FechaModificacion ?? rawProduct.PrdFechaModificacion ?? rawProduct.fechaModificacion ?? rawProduct.prdFechaModificacion,
    CategoriaId: rawProduct.CategoriaId ?? rawProduct.PrdCategoriaId ?? rawProduct.categoriaId ?? rawProduct.prdCategoriaId ?? 0,
    CategoriaNombre: rawProduct.CategoriaNombre ?? rawProduct.categoriaNombre ?? rawProduct.categoria?.nombre ?? 'Sin categoría',
    CategoriaRuta: rawProduct.CategoriaRuta ?? rawProduct.categoriaRuta ?? rawProduct.categoria?.ruta ?? '',
    MarcaId: rawProduct.MarcaId ?? rawProduct.PrdMarcaId ?? rawProduct.marcaId ?? rawProduct.prdMarcaId ?? 0,
    MarcaNombre: rawProduct.MarcaNombre ?? rawProduct.marcaNombre ?? rawProduct.marca?.nombre ?? 'Sin marca',
    MarcaLogo: rawProduct.MarcaLogo ?? rawProduct.marcaLogo ?? rawProduct.marca?.logo,
    StockActual: rawProduct.StockActual ?? rawProduct.stockActual ?? rawProduct.stock ?? 0,
    StockReservado: rawProduct.StockReservado ?? rawProduct.stockReservado ?? 0,
    Imagenes: rawProduct.Imagenes ?? rawProduct.imagenes ?? [],
    ImagenPrincipal: rawProduct.ImagenPrincipal ?? rawProduct.imagenPrincipal
  };
  
  console.log('✅ Producto individual mapeado:', {
    Id: mappedProduct.Id,
    Nombre: mappedProduct.Nombre,
    SKU: mappedProduct.SKU,
    Precio: mappedProduct.Precio,
    Activo: mappedProduct.Activo,
    CategoriaNombre: mappedProduct.CategoriaNombre,
    MarcaNombre: mappedProduct.MarcaNombre
  });
  
  // ✅ VALIDAR SI EL MAPEO FUE EXITOSO
  const mappingSuccess = mappedProduct.Id > 0 && mappedProduct.Nombre !== 'Sin nombre';
  console.log(`${mappingSuccess ? '✅' : '❌'} Mapeo individual ${mappingSuccess ? 'exitoso' : 'falló'}`);
  
  if (!mappingSuccess) {
    console.error('❌ MAPEO FALLÓ - Datos raw:', rawProduct);
    throw new Error('Error en el mapeo del producto');
  }
  
  return mappedProduct;
};

// ✅ FUNCIÓN DE DEBUG PARA INVESTIGAR EL MAPEO DE PROPIEDADES
const debugPropertyMapping = (rawProduct: any, index: number = 0) => {
  console.log(`🔍 === DEBUG PRODUCTO ${index + 1} - MAPEO DE PROPIEDADES ===`);
  
  // Log del objeto raw completo
  console.log('📦 Producto RAW completo:', rawProduct);
  console.log('📦 Tipo del producto:', typeof rawProduct);
  console.log('📦 Es array?:', Array.isArray(rawProduct));
  console.log('📦 Es null?:', rawProduct === null);
  console.log('📦 Es undefined?:', rawProduct === undefined);
  
  if (rawProduct && typeof rawProduct === 'object') {
    // Todas las propiedades del objeto
    const allProps = Object.keys(rawProduct);
    console.log('🔑 Todas las propiedades encontradas:', allProps);
    
    // Valores de las propiedades principales
    console.log('📋 Valores de propiedades principales:');
    allProps.forEach(prop => {
      const value = rawProduct[prop];
      console.log(`  ${prop}: ${value} (${typeof value})`);
    });
    
    // Buscar propiedades que podrían ser ID
    const idProps = allProps.filter(prop => 
      prop.toLowerCase().includes('id') || 
      prop.toLowerCase() === 'prdid' ||
      prop === 'Id'
    );
    console.log('🆔 Propiedades de ID encontradas:', idProps);
    
    // Buscar propiedades que podrían ser Nombre
    const nameProps = allProps.filter(prop => 
      prop.toLowerCase().includes('nombre') || 
      prop.toLowerCase().includes('name') ||
      prop.toLowerCase() === 'prdnombre'
    );
    console.log('📝 Propiedades de Nombre encontradas:', nameProps);
    
    // Buscar propiedades que podrían ser SKU
    const skuProps = allProps.filter(prop => 
      prop.toLowerCase().includes('sku') ||
      prop.toLowerCase() === 'prdsku'
    );
    console.log('🏷️ Propiedades de SKU encontradas:', skuProps);
    
    // Buscar propiedades que podrían ser Precio
    const priceProps = allProps.filter(prop => 
      prop.toLowerCase().includes('precio') || 
      prop.toLowerCase().includes('price') ||
      prop.toLowerCase() === 'prdprecio'
    );
    console.log('💰 Propiedades de Precio encontradas:', priceProps);
    
    // Mapeo sugerido
    console.log('🎯 MAPEO SUGERIDO:');
    const mapping = {
      Id: idProps[0] || 'PROP_NO_ENCONTRADA',
      Nombre: nameProps[0] || 'PROP_NO_ENCONTRADA', 
      SKU: skuProps[0] || 'PROP_NO_ENCONTRADA',
      Precio: priceProps[0] || 'PROP_NO_ENCONTRADA'
    };
    console.log(mapping);
    
    return mapping;
  }
  
  return null;
};

// Service
const productsService = {
  // ✅ MÉTODO PRINCIPAL CORREGIDO CON DEBUG COMPLETO - getProducts
  async getProducts(filter: Partial<ProductFilterDto> = {}): Promise<PagedResult<ProductDto>> {
    console.log('🚀 === INICIANDO getProducts CON DEBUG DE MAPEO ===');
    console.log('📋 Filtros recibidos:', filter);
    
    // ✅ CONSTRUCCIÓN CORRECTA DE PARÁMETROS
    const params = new URLSearchParams();
    Object.entries(filter).forEach(([key, value]) => {
      if (value !== undefined && value !== null) {
        if (key === 'Activo' && typeof value === 'boolean') {
          params.append(key, value.toString());
        } else if (value !== '') {
          params.append(key, value.toString());
        }
      }
    });

    const finalUrl = `/products?${params.toString()}`;
    console.log('🌐 URL final:', finalUrl);
    console.log('📡 Parámetros enviados:', Object.fromEntries(params));

    try {
      const response: AxiosResponse<ApiResponse<PagedResult<ProductDto>>> = await api.get(finalUrl);
      
      console.log('📨 === ANÁLISIS DETALLADO DE LA RESPUESTA ===');
      console.log('Status:', response.status);
      console.log('Response.data completo:', response.data);
      
      if (!response.data) {
        throw new Error("Respuesta vacía del servidor");
      }
      
      if (!response.data.success) {
        throw new Error(response.data.message || "Error al obtener los productos");
      }
      
      if (!response.data.data) {
        console.warn('⚠️ response.data.data es null/undefined');
        return {
          items: [],
          totalItems: 0,
          page: filter.Page || 1,
          pageSize: filter.PageSize || 10,
          totalPages: 0
        };
      }

      const result = response.data.data;
      
      console.log('📦 === ANÁLISIS DE LA ESTRUCTURA DE DATOS ===');
      console.log('result completo:', result);
      console.log('result.items tipo:', typeof result.items);
      console.log('result.items es array:', Array.isArray(result.items));
      console.log('result.items length:', result.items?.length);
      
      if (!result.items || !Array.isArray(result.items)) {
        console.warn('⚠️ result.items no es un array válido');
        result.items = [];
      }

      // ✅ DEBUG ESPECÍFICO DE CADA PRODUCTO
      if (result.items.length > 0) {
        console.log('🔍 === ANÁLISIS DE PRODUCTOS INDIVIDUALES ===');
        
        for (let i = 0; i < Math.min(result.items.length, 3); i++) {
          const rawProduct = result.items[i];
          console.log(`\n--- PRODUCTO ${i + 1} ---`);
          const mapping = debugPropertyMapping(rawProduct, i);
          
          // ✅ INTENTAR MAPEAR CON DIFERENTES ESTRATEGIAS
          console.log('🔄 Intentando mapear con diferentes estrategias...');
          
          // Estrategia 1: Propiedades tal como vienen
          console.log('Estrategia 1 - Directo:', {
            Id: rawProduct.Id,
            Nombre: rawProduct.Nombre,
            SKU: rawProduct.SKU,
            Precio: rawProduct.Precio
          });
          
          // Estrategia 2: Propiedades con prefijo Prd
          console.log('Estrategia 2 - Con prefijo Prd:', {
            Id: rawProduct.PrdId,
            Nombre: rawProduct.PrdNombre,
            SKU: rawProduct.PrdSku,
            Precio: rawProduct.PrdPrecio
          });
          
          // Estrategia 3: Propiedades en minúsculas
          console.log('Estrategia 3 - Minúsculas:', {
            Id: rawProduct.id,
            Nombre: rawProduct.nombre,
            SKU: rawProduct.sku,
            Precio: rawProduct.precio
          });
          
          // Estrategia 4: Buscar la primera propiedad que contenga los valores
          const foundProps = Object.keys(rawProduct);
          console.log('Estrategia 4 - Búsqueda inteligente:');
          foundProps.forEach(prop => {
            const value = rawProduct[prop];
            if (value !== null && value !== undefined && value !== '') {
              console.log(`  ${prop}: ${value} (${typeof value})`);
            }
          });
        }
      }

      // ✅ MAPEAR PRODUCTOS CON ESTRATEGIA ADAPTATIVA
      const mappedItems = result.items.map((rawProduct: any, index: number) => {
        console.log(`\n🔄 Mapeando producto ${index + 1}:`);
        console.log('Raw product keys:', Object.keys(rawProduct));
        console.log('Raw product values sample:', {
          firstKey: Object.keys(rawProduct)[0],
          firstValue: rawProduct[Object.keys(rawProduct)[0]]
        });
        
        return mapSingleProduct(rawProduct);
      });

      const totalPages = result.totalPages || Math.ceil(result.totalItems / result.pageSize);

      const finalResult: PagedResult<ProductDto> = {
        items: mappedItems,
        totalItems: result.totalItems,
        page: result.page,
        pageSize: result.pageSize,
        totalPages: totalPages
      };

      console.log('✅ === FIN getProducts - RESULTADO FINAL ===');
      console.log('Productos mapeados exitosamente:', finalResult.items.length);
      console.log('Productos con datos válidos:', finalResult.items.filter(p => p.Id > 0 && p.Nombre !== 'Sin nombre').length);
      
      return finalResult;

    } catch (error) {
      console.error('❌ === ERROR en getProducts ===');
      const axiosError = error as AxiosError<ApiResponse<PagedResult<ProductDto>>>;
      
      console.error('Error completo:', {
        message: axiosError.message,
        status: axiosError.response?.status,
        responseData: axiosError.response?.data
      });

      throw new Error(axiosError.response?.data?.message || axiosError.message || "Error al obtener productos");
    }
  },

  // ✅ MÉTODO getProduct COMPLETAMENTE CORREGIDO
  async getProduct(id: number): Promise<ProductDto> {
    console.log(`🔍 === getProduct - Buscando producto ID: ${id} ===`);
    
    try {
      const response = await api.get<ApiResponse<ProductDto>>(`/products/${id}`);
      
      console.log('📨 getProduct - Respuesta completa:', {
        status: response.status,
        success: response.data?.success,
        hasData: !!response.data?.data,
        dataType: typeof response.data?.data,
        rawData: response.data?.data
      });
      
      if (!response.data) {
        console.error('❌ getProduct - Respuesta vacía del servidor');
        throw new Error("Respuesta vacía del servidor");
      }
      
      if (!response.data.success) {
        console.error('❌ getProduct - Respuesta no exitosa:', response.data.message);
        throw new Error(response.data.message || "Error al obtener el producto");
      }
      
      if (!response.data.data) {
        console.error('❌ getProduct - No hay datos del producto');
        throw new Error("Producto no encontrado");
      }
      
      // ✅ USAR LA MISMA FUNCIÓN DE MAPEO QUE EN getProducts
      console.log('🔄 getProduct - Aplicando mapeo al producto individual...');
      const mappedProduct = mapSingleProduct(response.data.data);
      
      console.log('✅ getProduct - Producto mapeado exitosamente:', {
        Id: mappedProduct.Id,
        Nombre: mappedProduct.Nombre,
        SKU: mappedProduct.SKU,
        Precio: mappedProduct.Precio
      });
      
      return mappedProduct;
      
    } catch (error) {
      console.error('❌ getProduct - Error:', error);
      const axiosError = error as AxiosError<ApiResponse<ProductDto>>;
      
      if (axiosError.response?.status === 404) {
        throw new Error("Producto no encontrado");
      }
      
      throw new Error(axiosError.response?.data?.message || axiosError.message || "Error al obtener el producto");
    }
  },

  // ✅ MÉTODO DE DEBUG PERSONALIZADO
  async debugProducts(): Promise<any> {
    console.log('🔧 === DEBUG PRODUCTS COMPLETO ===');
    
    try {
      // 1. Test del endpoint de debug de mapeo del backend
      console.log('1️⃣ Probando endpoint de debug de mapeo...');
      try {
        const mappingDebug = await api.get('/products/debug/mapping');
        console.log('🔍 Backend mapping debug:', mappingDebug.data);
      } catch (error) {
        console.log('⚠️ Endpoint de debug mapping no disponible:', error);
      }

      // 2. Test básico sin filtros
      console.log('2️⃣ Test básico sin filtros...');
      const basicTest = await this.getProducts({
        Page: 1,
        PageSize: 5
      });
      
      console.log('📊 Resultado básico:', {
        totalItems: basicTest.totalItems,
        itemsCount: basicTest.items.length,
        hasItems: basicTest.items.length > 0,
        firstItem: basicTest.items[0]
      });

      // 3. Test con filtro Activo = undefined (todos)
      console.log('3️⃣ Test con Activo = undefined...');
      const allTest = await this.getProducts({
        Activo: undefined,
        Page: 1,
        PageSize: 5
      });

      // 4. Test con filtro Activo = true
      console.log('4️⃣ Test con Activo = true...');
      const activeTest = await this.getProducts({
        Activo: true,
        Page: 1,
        PageSize: 5
      });

      // 5. Test con filtro Activo = false
      console.log('5️⃣ Test con Activo = false...');
      const inactiveTest = await this.getProducts({
        Activo: false,
        Page: 1,
        PageSize: 5
      });

      const debugResult = {
        basic: { total: basicTest.totalItems, count: basicTest.items.length },
        all: { total: allTest.totalItems, count: allTest.items.length },
        active: { total: activeTest.totalItems, count: activeTest.items.length },
        inactive: { total: inactiveTest.totalItems, count: inactiveTest.items.length },
        sampleProduct: basicTest.items[0] || null,
        mappingAnalysis: {
          hasValidProducts: basicTest.items.some(p => p.Id > 0 && p.Nombre !== 'Sin nombre'),
          validProductsCount: basicTest.items.filter(p => p.Id > 0 && p.Nombre !== 'Sin nombre').length,
          invalidProductsCount: basicTest.items.filter(p => p.Id === 0 || p.Nombre === 'Sin nombre').length
        },
        conclusion: 'Debug completado - revisa los logs de consola para detalles'
      };

      console.log('🎯 Resumen debug:', debugResult);
      return debugResult;

    } catch (error) {
      console.error('❌ Error en debug:', error);
      return { error: error.message };
    }
  },

  // ✅ MÉTODO ESPECÍFICO PARA DEBUG DE MAPEO DEL BACKEND
  async debugBackendMapping(): Promise<any> {
    try {
      console.log('🔍 Probando endpoint de debug de mapeo del backend...');
      const response = await api.get('/products/debug/mapping');
      console.log('📊 Resultado del debug de mapeo:', response.data);
      return response.data;
    } catch (error) {
      console.error('❌ Error en debug de mapeo del backend:', error);
      throw error;
    }
  },

  async getProductBySlug(slug: string): Promise<ProductDto> {
    const response = await api.get<ApiResponse<ProductDto>>(`/products/slug/${slug}`);
    if (!response.data.success || !response.data.data) throw new Error("Producto no encontrado");
    return mapSingleProduct(response.data.data);
  },

  async createProduct(data: CreateProductDto): Promise<ProductDto> {
    const response = await api.post<ApiResponse<ProductDto>>("/products", data);
    if (!response.data.success || !response.data.data) throw new Error(response.data.message);
    return mapSingleProduct(response.data.data);
  },

  async updateProduct(id: number, data: UpdateProductDto): Promise<ProductDto> {
    const response = await api.put<ApiResponse<ProductDto>>(`/products/${id}`, data);
    if (!response.data.success || !response.data.data) throw new Error(response.data.message);
    return mapSingleProduct(response.data.data);
  },

  async deleteProduct(id: number): Promise<void> {
    const response = await api.delete<ApiResponse<void>>(`/products/${id}`);
    if (!response.data.success) throw new Error(response.data.message);
  },

  // Búsqueda y Filtros
  async searchProducts(filter: Partial<ProductFilterDto> = {}): Promise<ProductSearchResultDto> {
    const params = new URLSearchParams();
    Object.entries(filter).forEach(([key, value]) => {
      if (value !== undefined && value !== null) params.append(key, value.toString());
    });

    const response = await api.get<ApiResponse<ProductSearchResultDto>>(`/products/search?${params}`);
    if (!response.data.success || !response.data.data) throw new Error("Error en búsqueda");
    return response.data.data;
  },

  async getFeaturedProducts(count: number = 8): Promise<ProductSummaryDto[]> {
    const response = await api.get<ApiResponse<ProductSummaryDto[]>>(`/products/featured?count=${count}`);
    return response.data.data || [];
  },

  async getProductsOnSale(count: number = 12): Promise<ProductSummaryDto[]> {
    const response = await api.get<ApiResponse<ProductSummaryDto[]>>(`/products/on-sale?count=${count}`);
    return response.data.data || [];
  },

  async getRelatedProducts(id: number, count: number = 6): Promise<ProductSummaryDto[]> {
    const response = await api.get<ApiResponse<ProductSummaryDto[]>>(`/products/${id}/related?count=${count}`);
    return response.data.data || [];
  },

  async getProductsByCategory(categoryId: number, count: number = 12): Promise<ProductSummaryDto[]> {
    const response = await api.get<ApiResponse<ProductSummaryDto[]>>(`/products/category/${categoryId}?count=${count}`);
    return response.data.data || [];
  },

  async getProductsByBrand(brandId: number, count: number = 12): Promise<ProductSummaryDto[]> {
    const response = await api.get<ApiResponse<ProductSummaryDto[]>>(`/products/brand/${brandId}?count=${count}`);
    return response.data.data || [];
  },

  async getNewestProducts(count: number = 8): Promise<ProductSummaryDto[]> {
    const response = await api.get<ApiResponse<ProductSummaryDto[]>>(`/products/newest?count=${count}`);
    return response.data.data || [];
  },

  async getAvailableFilters(filter?: Partial<ProductFilterDto>): Promise<ProductSearchFiltersDto> {
    const params = new URLSearchParams();
    if (filter) {
      Object.entries(filter).forEach(([key, value]) => {
        if (value !== undefined && value !== null) params.append(key, value.toString());
      });
    }

    const response = await api.get<ApiResponse<ProductSearchFiltersDto>>(`/products/filters?${params}`);
    return response.data.data!;
  },

  async getActiveProducts(): Promise<ProductSummaryDto[]> {
    const response = await api.get<ApiResponse<ProductSummaryDto[]>>("/products/active");
    return response.data.data || [];
  },

  // Gestión de Inventario
  async adjustStock(data: AdjustStockDto): Promise<void> {
    const response = await api.post<ApiResponse<void>>("/products/adjust-stock", data);
    if (!response.data.success) throw new Error(response.data.message);
  },

  async updateStock(id: number, data: UpdateStockRequest): Promise<void> {
    const response = await api.put<ApiResponse<void>>(`/products/${id}/stock`, data);
    if (!response.data.success) throw new Error(response.data.message);
  },

  async getLowStockProducts(): Promise<ProductSummaryDto[]> {
    const response = await api.get<ApiResponse<ProductSummaryDto[]>>("/products/low-stock");
    return response.data.data || [];
  },

  async getOutOfStockProducts(): Promise<ProductSummaryDto[]> {
    const response = await api.get<ApiResponse<ProductSummaryDto[]>>("/products/out-of-stock");
    return response.data.data || [];
  },

  // Operaciones Masivas
  async toggleProductStatus(id: number): Promise<void> {
    console.log(`🔄 === toggleProductStatus - ID: ${id} ===`);
    
    try {
      // Verificar token antes de la solicitud
      const token = localStorage.getItem("authToken");
      console.log('🔑 Token para toggle:', token ? 'Presente' : 'Ausente');
      
      if (!token) {
        throw new Error('No hay token de autenticación. Por favor, inicia sesión nuevamente.');
      }
      
      const response = await api.patch<ApiResponse<void>>(`/products/${id}/toggle-status`);
      
      console.log('✅ Toggle response:', {
        status: response.status,
        success: response.data?.success,
        message: response.data?.message
      });
      
      if (!response.data.success) {
        throw new Error(response.data.message || 'Error al cambiar el estado del producto');
      }
      
    } catch (error) {
      console.error('❌ Toggle error:', error);
      const axiosError = error as AxiosError<ApiResponse<void>>;
      
      if (axiosError.response?.status === 403) {
        throw new Error('No tienes permisos para cambiar el estado de productos. Verifica tus credenciales.');
      } else if (axiosError.response?.status === 401) {
        throw new Error('Tu sesión ha expirado. Por favor, inicia sesión nuevamente.');
      }
      
      throw new Error(axiosError.response?.data?.message || axiosError.message || 'Error al cambiar el estado');
    }
  },

  async bulkToggleStatus(data: BulkToggleStatusDto): Promise<{ count: number }> {
    const response = await api.patch<ApiResponse<{ count: number }>>("/products/bulk-toggle-status", data);
    if (!response.data.success || !response.data.data) throw new Error(response.data.message);
    return response.data.data;
  },

  async bulkUpdatePrices(data: BulkPriceUpdateDto): Promise<{ count: number }> {
    const response = await api.patch<ApiResponse<{ count: number }>>("/products/bulk-update-prices", data);
    if (!response.data.success || !response.data.data) throw new Error(response.data.message);
    return response.data.data;
  },

  async bulkDelete(data: BulkOperationDto): Promise<{ count: number }> {
    const response = await api.delete<ApiResponse<{ count: number }>>("/products/bulk-delete", { data });
    if (!response.data.success || !response.data.data) throw new Error(response.data.message);
    return response.data.data;
  },

  // Estadísticas y Reportes
  async getProductStats(): Promise<ProductStatsDto> {
    const response = await api.get<ApiResponse<ProductStatsDto>>("/products/stats");
    return response.data.data!;
  },

  async getBestSellingProducts(count: number = 10): Promise<ProductSummaryDto[]> {
    const response = await api.get<ApiResponse<ProductSummaryDto[]>>(`/products/best-selling?count=${count}`);
    return response.data.data || [];
  },

  // Validaciones
  async productExists(name: string, excludeId?: number): Promise<boolean> {
    const params = new URLSearchParams();
    params.append("name", name);
    if (excludeId) params.append("excludeId", excludeId.toString());

    const response = await api.get<ApiResponse<boolean>>(`/products/exists/name?${params}`);
    return response.data.data || false;
  },

  async skuExists(sku: string, excludeId?: number): Promise<boolean> {
    const params = new URLSearchParams();
    params.append("sku", sku);
    if (excludeId) params.append("excludeId", excludeId.toString());

    const response = await api.get<ApiResponse<boolean>>(`/products/exists/sku?${params}`);
    return response.data.data || false;
  },

  async slugExists(slug: string, excludeId?: number): Promise<boolean> {
    const params = new URLSearchParams();
    params.append("slug", slug);
    if (excludeId) params.append("excludeId", excludeId.toString());

    const response = await api.get<ApiResponse<boolean>>(`/products/exists/slug?${params}`);
    return response.data.data || false;
  },
};

// ✅ FUNCIONES GLOBALES DE DEBUG PARA LA CONSOLA DEL NAVEGADOR
(window as any).debugProducts = productsService.debugProducts.bind(productsService);
(window as any).debugBackendMapping = productsService.debugBackendMapping.bind(productsService);

// ✅ FUNCIÓN DE DEBUG RÁPIDO PARA PROBAR EN CONSOLA
(window as any).quickProductTest = async () => {
  console.log('🚀 === QUICK PRODUCT TEST ===');
  try {
    const result = await productsService.getProducts({
      Page: 1,
      PageSize: 2,
      Activo: undefined
    });
    
    console.log('📊 Test Result:', {
      totalItems: result.totalItems,
      itemsLoaded: result.items.length,
      firstProduct: result.items[0] || 'No products',
      allProducts: result.items
    });
    
    if (result.items.length > 0) {
      const product = result.items[0];
      console.log('🔍 First Product Analysis:', {
        hasValidId: product.Id > 0,
        hasValidName: product.Nombre && product.Nombre !== 'Sin nombre',
        hasValidSKU: product.SKU && product.SKU !== 'Sin SKU',
        hasValidPrice: product.Precio > 0,
        isActive: product.Activo,
        summary: `${product.Id > 0 ? '✅' : '❌'} ID | ${product.Nombre !== 'Sin nombre' ? '✅' : '❌'} Name | ${product.SKU !== 'Sin SKU' ? '✅' : '❌'} SKU | ${product.Precio > 0 ? '✅' : '❌'} Price`
      });
    }
    
    return result;
  } catch (error) {
    console.error('❌ Quick test failed:', error);
    return { error: error.message };
  }
};

// ✅ FUNCIÓN PARA PROBAR DIRECTAMENTE LA API SIN MAPEO
(window as any).testRawAPI = async () => {
  console.log('🔍 === TEST RAW API ===');
  try {
    const response = await api.get('/products?Page=1&PageSize=2');
    console.log('📦 Raw API Response:', response.data);
    
    if (response.data?.data?.items?.length > 0) {
      const firstItem = response.data.data.items[0];
      console.log('🔍 First Item Raw Properties:', Object.keys(firstItem));
      console.log('🔍 First Item Raw Values:', firstItem);
    }
    
    return response.data;
  } catch (error) {
    console.error('❌ Raw API test failed:', error);
    return { error: error.message };
  }
};

// ✅ FUNCIÓN PARA PROBAR PRODUCTO INDIVIDUAL
(window as any).testSingleProduct = async (id: number = 1) => {
  console.log(`🔍 === TEST SINGLE PRODUCT ${id} ===`);
  try {
    // 1. Test API directa
    const rawResponse = await api.get(`/products/${id}`);
    console.log('📦 Raw Single Product Response:', rawResponse.data);
    
    if (rawResponse.data?.data) {
      console.log('🔍 Single Product Raw Properties:', Object.keys(rawResponse.data.data));
      console.log('🔍 Single Product Raw Values:', rawResponse.data.data);
    }
    
    // 2. Test service method
    const serviceResult = await productsService.getProduct(id);
    console.log('✅ Service Single Product Result:', serviceResult);
    
    return {
      raw: rawResponse.data,
      service: serviceResult
    };
  } catch (error) {
    console.error('❌ Single product test failed:', error);
    return { error: error.message };
  }
};

// ✅ FUNCIÓN PARA COMPARAR BACKEND VS FRONTEND
(window as any).compareBackendFrontend = async () => {
  console.log('🔄 === COMPARE BACKEND VS FRONTEND ===');
  
  try {
    // 1. Test del backend mapping
    console.log('1️⃣ Testing backend mapping...');
    const backendTest = await productsService.debugBackendMapping();
    
    // 2. Test del frontend
    console.log('2️⃣ Testing frontend...');
    const frontendTest = await (window as any).testRawAPI();
    
    // 3. Comparación
    console.log('3️⃣ Comparison:', {
      backend: {
        hasEntityRaw: !!backendTest?.EntityRaw,
        hasServiceMapped: !!backendTest?.ServiceMapped,
        mappingWorking: backendTest?.MappingAnalysis?.MappingWorking
      },
      frontend: {
        hasData: !!frontendTest?.data,
        hasItems: frontendTest?.data?.items?.length > 0,
        firstItemKeys: frontendTest?.data?.items?.[0] ? Object.keys(frontendTest.data.items[0]) : []
      }
    });
    
    return { backend: backendTest, frontend: frontendTest };
  } catch (error) {
    console.error('❌ Comparison failed:', error);
    return { error: error.message };
  }
};

// ✅ FUNCIÓN DE DEBUG PARA PRODUCTDETAIL
(window as any).debugProductDetail = async (id: number) => {
  console.log(`🔍 === DEBUG PRODUCT DETAIL ${id} ===`);
  
  try {
    // 1. Test endpoint normal
    console.log('1️⃣ Testing normal endpoint...');
    const normalResponse = await api.get(`/products/${id}`);
    console.log('📦 Normal Response:', normalResponse.data);
    
    // 2. Test service method
    console.log('2️⃣ Testing service method...');
    const serviceResult = await productsService.getProduct(id);
    console.log('✅ Service Result:', serviceResult);
    
    // 3. Test mapeo manual
    console.log('3️⃣ Testing manual mapping...');
    if (normalResponse.data?.data) {
      const manualMapped = mapSingleProduct(normalResponse.data.data);
      console.log('🔄 Manual Mapped Result:', manualMapped);
    }
    
    return {
      normal: normalResponse.data,
      service: serviceResult,
      success: true
    };
  } catch (error) {
    console.error('❌ Debug failed:', error);
    return { error: error.message, success: false };
  }
};

export default productsService;