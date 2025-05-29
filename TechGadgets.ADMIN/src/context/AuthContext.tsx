// context/AuthContext.tsx
import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import authService, { UserInfo, LoginRequest, RegisterRequest } from '../services/authService';

interface AuthContextType {
  user: UserInfo | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (credentials: LoginRequest) => Promise<{ success: boolean; message: string; errors?: Record<string, string[]> }>;
  register: (userData: RegisterRequest) => Promise<{ success: boolean; message: string; errors?: Record<string, string[]> }>;
  logout: () => void;
  hasRole: (role: string) => boolean;
  hasPermission: (permission: string) => boolean;
  hasAnyRole: (roles: string[]) => boolean;
  hasAnyPermission: (permissions: string[]) => boolean;
  refreshUser: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<UserInfo | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Initialize auth state on mount
  useEffect(() => {
    initializeAuth();
  }, []);

  const initializeAuth = async () => {
    console.log('🔄 Inicializando autenticación...');
    setIsLoading(true);
    
    try {
      // Check if user is stored in localStorage
      const storedUser = authService.getUser();
      const token = authService.getToken();

      console.log('💾 Usuario almacenado:', storedUser);
      console.log('🪙 Token almacenado:', token ? 'Sí existe' : 'No existe');

      if (storedUser && token) {
        console.log('✅ Datos de usuario encontrados en localStorage');
        console.log('👤 Usuario recuperado:', storedUser);
        setUser(storedUser);
        
        // Optionally verify token with server
        try {
          console.log('🔍 Verificando token con el servidor...');
          const currentUser = await authService.getCurrentUser();
          if (currentUser) {
            console.log('✅ Token válido, usuario actualizado:', currentUser);
            setUser(currentUser);
          }
        } catch (error) {
          console.log('⚠️ Token expirado, intentando renovar...', error);
          // Token might be expired, try to refresh
          const refreshResult = await authService.refreshToken();
          if (!refreshResult.success) {
            console.log('❌ Renovación falló, limpiando sesión');
            // Refresh failed, clear auth
            authService.logout();
            setUser(null);
          } else {
            console.log('✅ Token renovado exitosamente');
          }
        }
      } else {
        console.log('ℹ️ No hay sesión previa, usuario debe autenticarse');
      }
    } catch (error) {
      console.error('💥 Error inicializando auth:', error);
      authService.logout();
      setUser(null);
    } finally {
      setIsLoading(false);
      console.log('🏁 Inicialización de autenticación completada');
    }
  };

  const login = async (credentials: LoginRequest) => {
    try {
      console.log('🔐 Iniciando sesión con:', credentials);
      const response = await authService.login(credentials);
      
      console.log('📡 Respuesta del servidor:', response);
      
      if (response.success && response.user) {
        console.log('✅ Login exitoso - Datos del usuario:', response.user);
        console.log('🎭 Roles del usuario:', response.user.roles);
        console.log('🔑 Permisos del usuario:', response.user.permisos);
        console.log('🪙 Token recibido:', response.token ? 'Sí' : 'No');
        console.log('🔄 Refresh token recibido:', response.refreshToken ? 'Sí' : 'No');
        
        setUser(response.user);
        return { 
          success: true, 
          message: response.message 
        };
      } else {
        console.log('❌ Login fallido:', response.message);
        console.log('📝 Errores:', response.errors);
        return { 
          success: false, 
          message: response.message,
          errors: response.errors 
        };
      }
    } catch (error) {
      console.error('💥 Error en login:', error);
      return { 
        success: false, 
        message: 'Error de conexión con el servidor' 
      };
    }
  };

  const register = async (userData: RegisterRequest) => {
    try {
      const response = await authService.register(userData);
      
      return {
        success: response.success,
        message: response.message,
        errors: response.errors
      };
    } catch (error) {
      console.error('💥 Error durante el registro:', error);
      return {
        success: false,
        message: 'Error de conexión con el servidor'
      };
    }
  };

  const logout = () => {
    authService.logout();
    setUser(null);
  };

  const refreshUser = async () => {
    try {
      const currentUser = await authService.getCurrentUser();
      if (currentUser) {
        setUser(currentUser);
      }
    } catch (error) {
      console.error('Error refreshing user:', error);
    }
  };

  // Permission methods
  const hasRole = (role: string): boolean => {
    return user?.roles?.includes(role) || false;
  };

  const hasPermission = (permission: string): boolean => {
    return user?.permisos?.includes(permission) || false;
  };

  const hasAnyRole = (roles: string[]): boolean => {
    if (!user?.roles) return false;
    return roles.some(role => user.roles.includes(role));
  };

  const hasAnyPermission = (permissions: string[]): boolean => {
    if (!user?.permisos) return false;
    return permissions.some(permission => user.permisos.includes(permission));
  };

  const value: AuthContextType = {
    user,
    isAuthenticated: !!user,
    isLoading,
    login,
    register,
    logout,
    hasRole,
    hasPermission,
    hasAnyRole,
    hasAnyPermission,
    refreshUser,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};