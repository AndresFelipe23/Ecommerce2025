// services/brandsService.ts
import axios, { AxiosError, AxiosResponse } from "axios";
import {
  BrandDto,
  BrandFilterDto,
  CreateBrandDto,
  UpdateBrandDto,
  BrandSummaryDto,
  BrandStatsDto,
  BrandProductCountDto
} from "../types/brands";

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
}

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "http://localhost:5260/api";

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json"
  }
});

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

const brandsService = {
  
  async getAll(): Promise<BrandSummaryDto[]> {
    try {
      const response: AxiosResponse<ApiResponse<BrandSummaryDto[]>> = await api.get("/brands/all");
      return response.data.data || [];
    } catch (error) {
      const axiosError = error as AxiosError<ApiResponse<BrandSummaryDto[]>>;
      throw new Error(axiosError.response?.data?.message || "Error al obtener todas las marcas");
    }
  },
  
  async getBrands(filter: Partial<BrandFilterDto> = {}): Promise<PagedResult<BrandDto>> {
    const params = new URLSearchParams();
    Object.entries(filter).forEach(([key, value]) => {
      if (value !== undefined && value !== null) params.append(key, value.toString());
    });

    try {
      const response: AxiosResponse<ApiResponse<PagedResult<BrandDto>>> = await api.get(`/brands?${params}`);
      if (!response.data.success || !response.data.data) {
        throw new Error("Error al obtener las marcas");
      }
      return response.data.data;
    } catch (error) {
      const axiosError = error as AxiosError<ApiResponse<PagedResult<BrandDto>>>;
      throw new Error(axiosError.response?.data?.message || "Error al obtener marcas");
    }
  },

  async getBrand(id: number): Promise<BrandDto> {
    const response = await api.get<ApiResponse<BrandDto>>(`/brands/${id}`);
    if (!response.data.success || !response.data.data) throw new Error("Marca no encontrada");
    return response.data.data;
  },

  async createBrand(data: CreateBrandDto): Promise<BrandDto> {
    const response = await api.post<ApiResponse<BrandDto>>("/brands", data);
    if (!response.data.success || !response.data.data) throw new Error(response.data.message);
    return response.data.data;
  },

  async updateBrand(id: number, data: UpdateBrandDto): Promise<BrandDto> {
    const response = await api.put<ApiResponse<BrandDto>>(`/brands/${id}`, data);
    if (!response.data.success || !response.data.data) throw new Error(response.data.message);
    return response.data.data;
  },

  async deleteBrand(id: number): Promise<void> {
    const response = await api.delete<ApiResponse<void>>(`/brands/${id}`);
    if (!response.data.success) throw new Error(response.data.message);
  },

  async getActiveBrands(): Promise<BrandSummaryDto[]> {
    const response = await api.get<ApiResponse<BrandSummaryDto[]>>("/brands/active");
    return response.data.data || [];
  },

  async toggleBrandStatus(id: number): Promise<void> {
    const response = await api.patch<ApiResponse<void>>(`/brands/${id}/toggle-status`);
    if (!response.data.success) throw new Error(response.data.message);
  },

  async getBrandStats(): Promise<BrandStatsDto> {
    const response = await api.get<ApiResponse<BrandStatsDto>>("/brands/stats");
    return response.data.data!;
  },

  async getBrandsWithProductCount(): Promise<BrandProductCountDto[]> {
    const response = await api.get<ApiResponse<BrandProductCountDto[]>>("/brands/with-product-count");
    return response.data.data || [];
  }
};

export default brandsService;