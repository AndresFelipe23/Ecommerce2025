import axios, { AxiosError, AxiosResponse } from "axios";
import {
  CategoryDto,
  CategoryFilterDto,
  CreateCategoryDto,
  UpdateCategoryDto,
  CategorySummaryDto,
  CategoryStatsDto,
  CategoryProductCountDto,
  CategoryTreeDto,
  CategoryBreadcrumbDto
} from "../types/categories"; // Ajusta si tienes una carpeta distinta

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
  const token = localStorage.getItem("authToken");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Service
const categoriesService = {
  async getCategories(filter: Partial<CategoryFilterDto> = {}): Promise<PagedResult<CategoryDto>> {
    const params = new URLSearchParams();
    Object.entries(filter).forEach(([key, value]) => {
      if (value !== undefined && value !== null) params.append(key, value.toString());
    });

    try {
      const response: AxiosResponse<ApiResponse<PagedResult<CategoryDto>>> = await api.get(`/categories?${params}`);
      if (!response.data.success || !response.data.data) {
        throw new Error("Error al obtener las categorías");
      }
      return response.data.data;
    } catch (error) {
      const axiosError = error as AxiosError<ApiResponse<PagedResult<CategoryDto>>>;
      throw new Error(axiosError.response?.data?.message || "Error al obtener categorías");
    }
  },

  async getCategory(id: number): Promise<CategoryDto> {
    const response = await api.get<ApiResponse<CategoryDto>>(`/categories/${id}`);
    if (!response.data.success || !response.data.data) throw new Error("Categoría no encontrada");
    return response.data.data;
  },

  async createCategory(data: CreateCategoryDto): Promise<CategoryDto> {
    const response = await api.post<ApiResponse<CategoryDto>>("/categories", data);
    if (!response.data.success || !response.data.data) throw new Error(response.data.message);
    return response.data.data;
  },

  async updateCategory(id: number, data: UpdateCategoryDto): Promise<CategoryDto> {
    const response = await api.put<ApiResponse<CategoryDto>>(`/categories/${id}`, data);
    if (!response.data.success || !response.data.data) throw new Error(response.data.message);
    return response.data.data;
  },

  async deleteCategory(id: number): Promise<void> {
    const response = await api.delete<ApiResponse<void>>(`/categories/${id}`);
    if (!response.data.success) throw new Error(response.data.message);
  },

  async getCategoryTree(): Promise<CategoryTreeDto[]> {
    const response = await api.get<ApiResponse<CategoryTreeDto[]>>("/categories/tree");
    return response.data.data || [];
  },

  async getRootCategories(): Promise<CategorySummaryDto[]> {
    const response = await api.get<ApiResponse<CategorySummaryDto[]>>("/categories/root");
    return response.data.data || [];
  },

  async toggleCategoryStatus(id: number): Promise<void> {
    const response = await api.patch<ApiResponse<void>>(`/categories/${id}/toggle-status`);
    if (!response.data.success) throw new Error(response.data.message);
  },

  async getBreadcrumb(id: number): Promise<CategoryBreadcrumbDto[]> {
    const response = await api.get<ApiResponse<CategoryBreadcrumbDto[]>>(`/categories/${id}/breadcrumb`);
    return response.data.data || [];
  },

  async getCategoryStats(): Promise<CategoryStatsDto> {
    const response = await api.get<ApiResponse<CategoryStatsDto>>("/categories/stats");
    return response.data.data!;
  },

  async getCategoriesWithProductCount(): Promise<CategoryProductCountDto[]> {
    const response = await api.get<ApiResponse<CategoryProductCountDto[]>>("/categories/with-product-count");
    return response.data.data || [];
  }
};

export default categoriesService;
