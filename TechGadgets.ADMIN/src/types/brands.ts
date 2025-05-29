// types/brands.ts

export interface BrandDto {
    id: number;
    nombre: string;
    descripcion?: string;
    logo?: string;
    sitioWeb?: string;
    slug: string;
    activo: boolean;
    fechaCreacion: string;
    fechaModificacion?: string;
    totalProductos: number;
    orden: number;
    metaTitle?: string;
    metaDescription?: string;
    keywords?: string;
  }
  
  export interface BrandSummaryDto {
    id: number;
    nombre: string;
    logo?: string;
    slug: string;
    activo: boolean;
    totalProductos: number;
  }
  
  export interface BrandStatsDto {
    totalMarcas: number;
    marcasActivas: number;
    marcasInactivas: number;
    marcasConProductos: number;
    marcasSinProductos: number;
    ultimaMarcaCreada?: string;
    topMarcasPorProductos: BrandProductCountDto[];
  }
  
  export interface BrandProductCountDto {
    id: number;
    nombre: string;
    totalProductos: number;
  }
  
  export interface BrandFilterDto {
    page: number;
    pageSize: number;
    search?: string;
    nombre?: string;
    activo?: boolean;
    hasProducts?: boolean;
    fechaDesde?: string;
    fechaHasta?: string;
    sortBy?: string;
    sortDirection?: 'asc' | 'desc';
    sortDescending?: boolean;
  }
  
  export interface CreateBrandDto {
    nombre: string;
    descripcion?: string;
    logo?: string;
    sitioWeb?: string;
    slug?: string;
    orden?: number;
    metaTitle?: string;
    metaDescription?: string;
    keywords?: string;
  }
  
  export interface UpdateBrandDto {
    nombre: string;
    descripcion?: string;
    logo?: string;
    sitioWeb?: string;
    slug?: string;
    activo: boolean;
    orden?: number;
    metaTitle?: string;
    metaDescription?: string;
    keywords?: string;
  }
  
  export interface BulkToggleBrandStatusDto {
    brandIds: number[];
    active: boolean;
  }
  
  export interface BulkBrandOperationDto {
    brandIds: number[];
  }