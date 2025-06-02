// types/products.ts

export interface ProductDto {
    Id: number;
    SKU: string;
    Nombre: string;
    DescripcionCorta?: string;
    DescripcionLarga?: string;
    Slug: string;
    Precio: number;
    PrecioComparacion?: number;
    Costo?: number;
    Tipo?: string;
    Estado?: string;
    Destacado: boolean;
    Nuevo: boolean;
    EnOferta: boolean;
    Peso?: number;
    Dimensiones?: string;
    MetaTitulo?: string;
    MetaDescripcion?: string;
    PalabrasClaves?: string;
    RequiereEnvio: boolean;
    PermiteReseñas: boolean;
    Garantia?: string;
    Orden: number;
    Activo: boolean;
    FechaCreacion: string;
    FechaModificacion?: string;
  
    // Relaciones
    CategoriaId: number;
    CategoriaNombre: string;
    CategoriaRuta: string;
    MarcaId: number;
    MarcaNombre: string;
    MarcaLogo?: string;
  
    // Imágenes
    Imagenes: ProductImageDto[];
    ImagenPrincipal?: string;
  
    // Inventario
    StockActual: number;
    StockReservado: number;
  }

  export interface ProductSearchResponse {
    productos: ProductSummaryDto[];
    totalResultados: number;
    pagina: number;
    totalPaginas: number;
    filtrosDisponibles: ProductSearchFiltersDto;
  }
  
  
  export interface ProductSummaryDto {
    id: number;
    nombre: string;
    slug: string;
    precio: number;
    precioOferta?: number;
    imagenPrincipal?: string;
    activo: boolean;
    destacado: boolean;
    stock: number;
    marcaNombre: string;
    categoriaNombre: string;
  }
  
  
  export interface ProductImageDto {
    Id: number;
    Url: string;
    AltText?: string;
    EsPrincipal: boolean;
    Orden: number;
  }
  
  export interface ProductSearchResultDto {
    Productos: ProductSummaryDto[];
    FiltrosDisponibles: ProductSearchFiltersDto;
    TotalResultados: number;
    Pagina: number;
    TotalPaginas: number;
  }
  
  export interface ProductSearchFiltersDto {
    Categorias: CategoryFilterOption[];
    Marcas: BrandFilterOption[];
    RangoPrecios: PriceRangeDto;
  }
  
  export interface CategoryFilterOption {
    Id: number;
    Nombre: string;
    Count: number;
  }
  
  export interface BrandFilterOption {
    Id: number;
    Nombre: string;
    Count: number;
  }
  
  export interface PriceRangeDto {
    Min: number;
    Max: number;
  }
  
  export interface ProductStatsDto {
    TotalProductos: number;
    ProductosActivos: number;
    ProductosInactivos: number;
    ProductosDestacados: number;
    ProductosEnOferta: number;
    ProductosBajoStock: number;
    ProductosSinStock: number;
    ValorTotalInventario: number;
    PrecioPromedio: number;
    PrecioMin?: number;
    PrecioMax?: number;
    UltimoProductoCreado?: string;
  }
  
  export interface ProductFilterDto {
    Page: number;
    PageSize: number;
    Busqueda?: string;
    SKU?: string;
    CategoriaId?: number;
    MarcaId?: number;
    PrecioMin?: number;
    PrecioMax?: number;
    EnOferta?: boolean;
    Destacado?: boolean;
    Activo?: boolean;
    FechaDesde?: string;
    FechaHasta?: string;
    BajoStock?: boolean;
    SinStock?: boolean;
    SortBy?: string;
    SortDescending?: boolean;
    IncluirInactivos?: boolean;
    IncluirImagenes?: boolean;
  }
  
  export interface CreateProductDto {
    SKU: string;
    Nombre: string;
    DescripcionCorta?: string;
    DescripcionLarga?: string;
    Slug?: string;
    Precio: number;
    PrecioComparacion?: number;
    Costo?: number;
    CategoriaId: number;
    MarcaId: number;
    Tipo?: string;
    Estado?: string;
    Destacado?: boolean;
    Nuevo?: boolean;
    EnOferta?: boolean;
    Peso?: number;
    Dimensiones?: string;
    MetaTitulo?: string;
    MetaDescripcion?: string;
    PalabrasClaves?: string;
    RequiereEnvio?: boolean;
    PermiteReseñas?: boolean;
    Garantia?: string;
    Orden?: number;
    StockInicial?: number;
    Imagenes: CreateProductImageDto[];
  }
  
  export interface UpdateProductDto {
    SKU: string;
    Nombre: string;
    DescripcionCorta?: string;
    DescripcionLarga?: string;
    Slug?: string;
    Precio: number;
    PrecioComparacion?: number;
    Costo?: number;
    CategoriaId: number;
    MarcaId: number;
    Tipo?: string;
    Estado?: string;
    Destacado?: boolean;
    Nuevo?: boolean;
    EnOferta?: boolean;
    Peso?: number;
    Dimensiones?: string;
    MetaTitulo?: string;
    MetaDescripcion?: string;
    PalabrasClaves?: string;
    RequiereEnvio?: boolean;
    PermiteReseñas?: boolean;
    Garantia?: string;
    Orden?: number;
    Activo: boolean;
    Imagenes: UpdateProductImageDto[];
  }
  
  export interface CreateProductImageDto {
    Url: string;
    AltText?: string;
    EsPrincipal: boolean;
    Orden: number;
  }
  
  export interface UpdateProductImageDto {
    Id?: number;
    Url: string;
    AltText?: string;
    EsPrincipal: boolean;
    Orden: number;
    Eliminar: boolean;
  }
  
  export interface AdjustStockDto {
    ProductoId: number;
    Cantidad: number;
    Motivo: string;
  }
  
  export interface UpdateStockRequest {
    NuevoStock: number;
  }
  
  export interface BulkToggleStatusDto {
    ProductIds: number[];
    Activo: boolean;
  }
  
  export interface BulkPriceUpdateDto {
    ProductIds: number[];
    TipoOperacion: string; // "precio", "comparacion", "incremento", "descuento"
    NuevoPrecio?: number;
    NuevoPrecioComparacion?: number;
    PorcentajeIncremento?: number;
    PorcentajeDescuento?: number;
  }
  
  export interface BulkOperationDto {
    ProductIds: number[];
  }