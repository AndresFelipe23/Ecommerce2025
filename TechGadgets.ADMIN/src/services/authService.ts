// services/authService.ts
import axios, { AxiosResponse, AxiosError, InternalAxiosRequestConfig } from 'axios';

// Types
interface CustomAxiosRequestConfig extends InternalAxiosRequestConfig {
  _retry?: boolean;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  nombre: string;
  apellido: string;
  telefono?: string;
  fechaNacimiento?: string;
  genero?: 'M' | 'F' | 'O';
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface UserInfo {
  id: number;
  email: string;
  nombreCompleto: string;
  roles: string[];
  permisos: string[];
}

export interface AuthResponse {
  success: boolean;
  message: string;
  token?: string;
  refreshToken?: string;
  tokenExpiration?: string;
  user?: UserInfo;
  errors?: Record<string, string[]>;
}

// API Configuration
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:5260/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor to add auth token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('authToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor to handle token refresh
api.interceptors.response.use(
  (response) => response,
  async (error: unknown) => {
    const axiosError = error as AxiosError;
    if (!axiosError.config) {
      return Promise.reject(error);
    }

    const originalRequest = axiosError.config as CustomAxiosRequestConfig;
    if (axiosError.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      const refreshToken = localStorage.getItem('refreshToken');
      if (refreshToken) {
        try {
          const response = await refreshTokenRequest({ refreshToken });
          if (response.data.success && response.data.token) {
            localStorage.setItem('authToken', response.data.token);
            if (response.data.refreshToken) {
              localStorage.setItem('refreshToken', response.data.refreshToken);
            }
            
            // Retry original request with new token
            originalRequest.headers.Authorization = `Bearer ${response.data.token}`;
            return api(originalRequest);
          }
        } catch (error) {
          console.error('Error al refrescar el token:', error);
          // Refresh failed, redirect to login
          authService.logout();
          window.location.href = '/signin';
        }
      } else {
        // No refresh token, redirect to login
        authService.logout();
        window.location.href = '/signin';
      }
    }

    return Promise.reject(error);
  }
);

// Auth Service Methods
const authService = {
  // Login
  async login(credentials: LoginRequest): Promise<AuthResponse> {
    try {
      console.log('📤 Enviando request de login a:', `${API_BASE_URL}/auth/login`);
      console.log('📋 Credenciales enviadas:', { email: credentials.email, password: '***' });
      
      const response: AxiosResponse<AuthResponse> = await api.post('/auth/login', credentials);
      console.log('📨 Status de respuesta:', response.status);
      console.log('📦 Datos de respuesta completos:', response.data);
      if (response.data.success && response.data.token) {
        console.log('💾 Guardando token en localStorage...');
        localStorage.setItem('authToken', response.data.token);
        if (response.data.refreshToken) {
          console.log('💾 Guardando refresh token en localStorage...');
          localStorage.setItem('refreshToken', response.data.refreshToken);
        }
        if (response.data.user) {
          console.log('💾 Guardando datos de usuario en localStorage...');
          console.log('👤 Usuario completo:', JSON.stringify(response.data.user, null, 2));
          localStorage.setItem('user', JSON.stringify(response.data.user));
        }
        console.log('✅ Tokens y usuario guardados exitosamente');
      }
      
      return response.data;
    } catch (error: unknown) {
      
      const axiosError = error as AxiosError<AuthResponse>;
      return {
        success: false,
        message: axiosError.response?.data?.message || 'Error de conexión',
        errors: axiosError.response?.data?.errors
      };
    }
  },

  // Register
  async register(userData: RegisterRequest): Promise<AuthResponse> {
    try {
      const response: AxiosResponse<AuthResponse> = await api.post('/auth/register', userData);
      return response.data;
    } catch (error: unknown) {
      const axiosError = error as AxiosError<AuthResponse>;
      return {
        success: false,
        message: axiosError.response?.data?.message || 'Error de conexión',
        errors: axiosError.response?.data?.errors
      };
    }
  },

  // Logout
  logout(): void {
    localStorage.removeItem('authToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
  },

  // Get current user
  async getCurrentUser(): Promise<UserInfo | null> {
    try {
      const response: AxiosResponse<UserInfo> = await api.get('/auth/me');
      localStorage.setItem('user', JSON.stringify(response.data));
      return response.data;
    } catch (error: unknown) {
      console.error('Error al obtener usuario actual:', error);
      return null;
    }
  },

  // Change password
  async changePassword(passwordData: ChangePasswordRequest): Promise<AuthResponse> {
    try {
      const response: AxiosResponse<{ message: string }> = await api.post('/auth/change-password', passwordData);
      return {
        success: true,
        message: response.data.message
      };
    } catch (error: unknown) {
      const axiosError = error as AxiosError<AuthResponse>;
      return {
        success: false,
        message: axiosError.response?.data?.message || 'Error al cambiar contraseña'
      };
    }
  },

  // Check if email exists
  async checkEmail(email: string): Promise<boolean> {
    try {
      const response: AxiosResponse<{ exists: boolean }> = await api.get(`/auth/check-email/${email}`);
      return response.data.exists;
    } catch (error: unknown) {
      console.error('Error al verificar email:', error);
      return false;
    }
  },

  // Refresh token
  async refreshToken(): Promise<AuthResponse> {
    const refreshToken = localStorage.getItem('refreshToken');
    if (!refreshToken) {
      return { success: false, message: 'No refresh token available' };
    }

    try {
      const response = await refreshTokenRequest({ refreshToken });
      
      if (response.data.success && response.data.token) {
        localStorage.setItem('authToken', response.data.token);
        if (response.data.refreshToken) {
          localStorage.setItem('refreshToken', response.data.refreshToken);
        }
      }
      
      return response.data;
    } catch (error: unknown) {
      const axiosError = error as AxiosError<AuthResponse>;
      return {
        success: false,
        message: axiosError.response?.data?.message || 'Error refreshing token'
      };
    }
  },

  // Utility methods
  isAuthenticated(): boolean {
    const token = localStorage.getItem('authToken');
    return !!token;
  },

  getToken(): string | null {
    return localStorage.getItem('authToken');
  },

  getUser(): UserInfo | null {
    const userStr = localStorage.getItem('user');
    if (userStr) {
      try {
        return JSON.parse(userStr);
      } catch {
        return null;
      }
    }
    return null;
  },

  hasRole(role: string): boolean {
    const user = this.getUser();
    return user?.roles?.includes(role) || false;
  },

  hasPermission(permission: string): boolean {
    const user = this.getUser();
    return user?.permisos?.includes(permission) || false;
  },

  hasAnyRole(roles: string[]): boolean {
    const user = this.getUser();
    if (!user?.roles) return false;
    return roles.some(role => user.roles.includes(role));
  },

  hasAnyPermission(permissions: string[]): boolean {
    const user = this.getUser();
    if (!user?.permisos) return false;
    return permissions.some(permission => user.permisos.includes(permission));
  }
};

// Helper function for refresh token request (to avoid circular dependency)
async function refreshTokenRequest(data: RefreshTokenRequest): Promise<AxiosResponse<AuthResponse>> {
  return axios.post(`${API_BASE_URL}/auth/refresh-token`, data, {
    headers: { 'Content-Type': 'application/json' }
  });
}

export default authService;