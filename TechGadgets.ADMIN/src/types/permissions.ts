// types/permissions.ts
export interface Permission {
  id: string;
  name: string;
  description: string;
  module: string;
}

export interface Role {
  id: string;
  name: string;
  description: string;
  permissions: string[];
}

// Definición de permisos según tu API
export const PERMISSIONS = {
  // Dashboard
  DASHBOARD: {
    VIEW: 'dashboard.view',
  },
  
  // Usuarios
  USERS: {
    VIEW: 'usuarios.ver',
    CREATE: 'usuarios.crear',
    EDIT: 'usuarios.editar',
    DELETE: 'usuarios.eliminar',
    LIST: 'usuarios.listar',
  },
  
  // Roles
  ROLES: {
    VIEW: 'roles.ver',
    CREATE: 'roles.crear',
    EDIT: 'roles.editar',
    DELETE: 'roles.eliminar',
    LIST: 'roles.listar',
    ASSIGN: 'roles.asignar',
    REMOVE: 'roles.remover',
  },
  
  // Permisos
  PERMISSIONS: {
    LIST: 'permisos.listar',
  },
  
  // Productos
  PRODUCTS: {
    VIEW: 'productos.ver',
    CREATE: 'productos.crear',
    EDIT: 'productos.editar',
    DELETE: 'productos.eliminar',
    LIST: 'productos.listar',
    IMPORT: 'productos.importar',
    PUBLISH: 'productos.publicar',
    INVENTORY: 'inventario.actualizar',
    INVENTORY_ALERTS: 'inventario.alertas',
    REPORTS: 'reportes.productos',
    SALES_REPORTS: 'reportes.ventas'
  },
  
  // Categorías
  CATEGORIES: {
    VIEW: 'categorias.ver',
    CREATE: 'categorias.crear',
    EDIT: 'categorias.editar',
    DELETE: 'categorias.eliminar',
    LIST: 'categorias.listar',
  },
  
  // Marcas
  BRANDS: {
    VIEW: 'marcas.ver',
    CREATE: 'marcas.crear',
    EDIT: 'marcas.editar',
    DELETE: 'marcas.eliminar',
    LIST: 'marcas.listar',
  },
  
  // Pedidos
  ORDERS: {
    VIEW: 'pedidos.ver',
    CREATE: 'pedidos.crear',
    EDIT: 'pedidos.editar',
    DELETE: 'pedidos.eliminar',
    LIST: 'pedidos.listar',
    CANCEL: 'pedidos.cancelar',
    PROCESS: 'pedidos.procesar',
    REFUND: 'pedidos.reembolsar',
  },
  
  // Inventario
  INVENTORY: {
    VIEW: 'inventario.ver',
    ADJUST: 'inventario.ajustar',
    ALERTS: 'inventario.alertas',
  },
  
  // Reportes
  REPORTS: {
    SALES: 'reportes.ventas',
    USERS: 'reportes.usuarios',
    PRODUCTS: 'reportes.productos',
    FINANCIAL: 'reportes.financieros',
  },
  
  // Configuración
  CONFIG: {
    GENERAL: 'config.general',
    PAYMENTS: 'config.pagos',
    SHIPPING: 'config.envios',
    SYSTEM: 'config.sistema',
  }
} as const;

// Definición de roles predefinidos basados en tu API
export const ROLES = {
  SUPER_ADMIN: {
    id: 'SuperAdmin',
    name: 'Super Admin',
    description: 'Acceso total al sistema',
    permissions: [
      // Todos los permisos según tu lista de la API
      'categorias.crear', 'categorias.editar', 'categorias.eliminar', 'categorias.ver',
      'config.editar', 'config.ver', 'inventario.ajustar', 'inventario.ver',
      'marcas.crear', 'marcas.editar', 'marcas.eliminar', 'marcas.ver',
      'pedidos.cancelar', 'pedidos.editar', 'pedidos.reembolsar', 'pedidos.ver',
      'permisos.listar', 'productos.crear', 'productos.editar', 'productos.eliminar',
      'productos.importar', 'productos.ver', 'reportes.productos', 'reportes.usuarios',
      'reportes.ventas', 'roles.asignar', 'roles.crear', 'roles.editar',
      'roles.eliminar', 'roles.listar', 'roles.remover', 'roles.ver',
      'usuarios.crear', 'usuarios.editar', 'usuarios.eliminar', 'usuarios.listar', 'usuarios.ver'
    ]
  },
  
  ADMIN: {
    id: 'Admin',
    name: 'Admin',
    description: 'Administrador con acceso limitado',
    permissions: [
      'categorias.crear', 'categorias.editar', 'categorias.ver',
      'inventario.ver',
      'marcas.crear', 'marcas.editar', 'marcas.ver',
      'pedidos.editar', 'pedidos.ver',
      'productos.crear', 'productos.editar', 'productos.ver',
      'reportes.productos', 'reportes.ventas',
      'usuarios.ver'
    ]
  },
  
  VENDEDOR: {
    id: 'Vendedor',
    name: 'Vendedor',
    description: 'Empleado con permisos básicos',
    permissions: [
      'productos.ver',
      'pedidos.crear', 'pedidos.editar', 'pedidos.ver',
      'inventario.ver',
    ]
  },
  
  CLIENTE: {
    id: 'Cliente',
    name: 'Cliente',
    description: 'Cliente con acceso limitado',
    permissions: [
      // Los clientes normalmente no tienen acceso al panel admin
    ]
  }
} as const;

// Helper functions para trabajar con permisos
export class PermissionService {
  /**
   * Verifica si un usuario tiene un permiso específico
   */
  static hasPermission(userPermissions: string[], permission: string): boolean {
    return userPermissions.includes(permission);
  }

  /**
   * Verifica si un usuario tiene alguno de los permisos especificados
   */
  static hasAnyPermission(userPermissions: string[], permissions: string[]): boolean {
    return permissions.some(permission => userPermissions.includes(permission));
  }

  /**
   * Verifica si un usuario tiene todos los permisos especificados
   */
  static hasAllPermissions(userPermissions: string[], permissions: string[]): boolean {
    return permissions.every(permission => userPermissions.includes(permission));
  }

  /**
   * Obtiene los permisos de un rol específico
   */
  static getRolePermissions(roleId: string): string[] {
    const role = Object.values(ROLES).find(r => r.id === roleId);
    return role ? [...role.permissions] : [];
  }

  /**
   * Verifica si un usuario tiene un rol específico
   */
  static hasRole(userRoles: string[], role: string): boolean {
    return userRoles.includes(role);
  }

  /**
   * Verifica si un usuario tiene alguno de los roles especificados
   */
  static hasAnyRole(userRoles: string[], roles: string[]): boolean {
    return roles.some(role => userRoles.includes(role));
  }

  /**
   * Combina permisos de múltiples roles
   */
  static combineRolePermissions(roles: string[]): string[] {
    const allPermissions = new Set<string>();
    
    roles.forEach(roleId => {
      const permissions = this.getRolePermissions(roleId);
      permissions.forEach(permission => allPermissions.add(permission));
    });
    
    return Array.from(allPermissions);
  }
}

export default PermissionService;