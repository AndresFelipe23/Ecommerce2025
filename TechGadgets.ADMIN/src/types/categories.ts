export interface CategoryDto {
    id: number;
    nombre: string;
    descripcion?: string;
    categoriaPadreId?: number | null;
    categoriaPadreNombre?: string | null;
    imagen?: string;
    icono?: string;
    slug: string;
    orden: number;
    activo: boolean;
    fechaCreacion: string;
    totalProductos: number;
    totalSubcategorias: number;
    rutaCompleta: string;
    subcategorias?: CategoryDto[];
    metaTitle?: string;
    metaDescription?: string;
    keywords?: string;
  }
  
  export interface CategorySummaryDto {
    id: number;
    nombre: string;
    icono?: string;
    slug: string;
    activo: boolean;
    rutaCompleta: string;
  }
  
  export interface CategoryTreeDto {
    id: number;
    nombre: string;
    descripcion?: string;
    imagen?: string;
    icono?: string;
    slug: string;
    orden: number;
    activo: boolean;
    totalProductos: number;
    nivel: number;
    hijos?: CategoryTreeDto[];
  }
  
  export interface CategoryBreadcrumbDto {
    id: number;
    nombre: string;
    slug: string;
  }
  
  export interface CategoryProductCountDto {
    id: number;
    nombre: string;
    productosDirectos: number;
    productosDeSubcategorias?: number;
    totalProductos: number;
  }
  
  export interface CategoryStatsDto {
    totalCategorias: number;
    categoriasActivas: number;
    categoriasInactivas: number;
    categoriasRaiz: number;
    categoriasConHijos: number;
    categoriasConProductos: number;
    categoriasSinProductos: number;
    ultimaCategoriaCreada?: string;
    nivelesMaximos: number;
    topCategoriasPorProductos: CategoryProductCountDto[];
  }
  
  export interface CategoryFilterDto {
    page: number;
    pageSize: number;
    search?: string;
    nombre?: string;
    categoriaPadreId?: number;
    soloRaiz?: boolean;
    activo?: boolean;
    isActive?: boolean;
    hasProducts?: boolean;
    fechaDesde?: string;
    fechaHasta?: string;
    sortBy?: string;
    sortDirection?: 'asc' | 'desc';
    sortDescending?: boolean;
    incluirSubcategorias?: boolean;
  }
  
  export interface CreateCategoryDto {
    nombre: string;
    descripcion?: string;
    categoriaPadreId?: number | null;
    imagen?: string;
    icono?: string;
    slug?: string;
    orden?: number;
  }
  

  export interface UpdateCategoryDto {
    nombre: string;
    descripcion?: string;
    categoriaPadreId?: number | null;
    imagen?: string;
    icono?: string;
    slug?: string;
    orden?: number;
    activo: boolean;
  }
  
  export interface BulkToggleCategoryStatusDto {
    categoryIds: number[];
    active: boolean;
  }
  
  export interface BulkCategoryOperationDto {
    categoryIds: number[];
  }
  
  export interface MoveCategoryDto {
    categoryId: number;
    nuevoPadreId?: number;
    nuevoOrden: number;
  }
  