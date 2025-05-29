// hooks/usePermissions.ts
import { useMemo } from 'react';
import { useAuth } from '../context/AuthContext';
import PermissionService, { PERMISSIONS, ROLES } from '../types/permissions';

export const usePermissions = () => {
  const { user } = useAuth();

  // Memoizar los permisos del usuario para evitar recálculos
  const userPermissions = useMemo(() => {
    return user?.permisos || [];
  }, [user?.permisos]);

  const userRoles = useMemo(() => {
    return user?.roles || [];
  }, [user?.roles]);

  // Funciones helper
  const hasPermission = (permission: string): boolean => {
    return PermissionService.hasPermission(userPermissions, permission);
  };

  const hasAnyPermission = (permissions: string[]): boolean => {
    return PermissionService.hasAnyPermission(userPermissions, permissions);
  };

  const hasAllPermissions = (permissions: string[]): boolean => {
    return PermissionService.hasAllPermissions(userPermissions, permissions);
  };

  const hasRole = (role: string): boolean => {
    return PermissionService.hasRole(userRoles, role);
  };

  const hasAnyRole = (roles: string[]): boolean => {
    return PermissionService.hasAnyRole(userRoles, roles);
  };

  // Permisos específicos por módulo (basados en tu API)
  const can = useMemo(() => ({
    // Dashboard
    viewDashboard: hasPermission(PERMISSIONS.DASHBOARD.VIEW),

    // Usuarios
    viewUsers: hasPermission(PERMISSIONS.USERS.VIEW),
    listUsers: hasPermission(PERMISSIONS.USERS.LIST),
    createUsers: hasPermission(PERMISSIONS.USERS.CREATE),
    editUsers: hasPermission(PERMISSIONS.USERS.EDIT),
    deleteUsers: hasPermission(PERMISSIONS.USERS.DELETE),

    // Roles
    viewRoles: hasPermission(PERMISSIONS.ROLES.VIEW),
    listRoles: hasPermission(PERMISSIONS.ROLES.LIST),
    createRoles: hasPermission(PERMISSIONS.ROLES.CREATE),
    editRoles: hasPermission(PERMISSIONS.ROLES.EDIT),
    deleteRoles: hasPermission(PERMISSIONS.ROLES.DELETE),
    assignRoles: hasPermission(PERMISSIONS.ROLES.ASSIGN),
    removeRoles: hasPermission(PERMISSIONS.ROLES.REMOVE),

    // Permisos
    listPermissions: hasPermission(PERMISSIONS.PERMISSIONS.LIST),

    // Productos
    viewProducts: hasPermission(PERMISSIONS.PRODUCTS.VIEW),
    listProducts: hasPermission(PERMISSIONS.PRODUCTS.LIST),
    createProducts: hasPermission(PERMISSIONS.PRODUCTS.CREATE),
    editProducts: hasPermission(PERMISSIONS.PRODUCTS.EDIT),
    deleteProducts: hasPermission(PERMISSIONS.PRODUCTS.DELETE),
    importProducts: hasPermission(PERMISSIONS.PRODUCTS.IMPORT),

    // Categorías
    viewCategories: hasPermission(PERMISSIONS.CATEGORIES.VIEW),
    listCategories: hasPermission(PERMISSIONS.CATEGORIES.LIST),
    createCategories: hasPermission(PERMISSIONS.CATEGORIES.CREATE),
    editCategories: hasPermission(PERMISSIONS.CATEGORIES.EDIT),
    deleteCategories: hasPermission(PERMISSIONS.CATEGORIES.DELETE),

    // Marcas
    viewBrands: hasPermission(PERMISSIONS.BRANDS.VIEW),
    listBrands: hasPermission(PERMISSIONS.BRANDS.LIST),
    createBrands: hasPermission(PERMISSIONS.BRANDS.CREATE),
    editBrands: hasPermission(PERMISSIONS.BRANDS.EDIT),
    deleteBrands: hasPermission(PERMISSIONS.BRANDS.DELETE),

    // Pedidos
    viewOrders: hasPermission(PERMISSIONS.ORDERS.VIEW),
    listOrders: hasPermission(PERMISSIONS.ORDERS.LIST),
    createOrders: hasPermission(PERMISSIONS.ORDERS.CREATE),
    editOrders: hasPermission(PERMISSIONS.ORDERS.EDIT),
    deleteOrders: hasPermission(PERMISSIONS.ORDERS.DELETE),
    cancelOrders: hasPermission(PERMISSIONS.ORDERS.CANCEL),
    processOrders: hasPermission(PERMISSIONS.ORDERS.PROCESS),
    refundOrders: hasPermission(PERMISSIONS.ORDERS.REFUND),

    // Inventario
    viewInventory: hasPermission(PERMISSIONS.INVENTORY.VIEW),
    adjustInventory: hasPermission(PERMISSIONS.INVENTORY.ADJUST),
    viewInventoryAlerts: hasPermission(PERMISSIONS.INVENTORY.ALERTS),

    // Reportes
    viewSalesReports: hasPermission(PERMISSIONS.REPORTS.SALES),
    viewUserReports: hasPermission(PERMISSIONS.REPORTS.USERS),
    viewProductReports: hasPermission(PERMISSIONS.REPORTS.PRODUCTS),
    viewFinancialReports: hasPermission(PERMISSIONS.REPORTS.FINANCIAL),

    // Configuración
    accessGeneralConfig: hasPermission(PERMISSIONS.CONFIG.GENERAL),
    accessPaymentConfig: hasPermission(PERMISSIONS.CONFIG.PAYMENTS),
    accessShippingConfig: hasPermission(PERMISSIONS.CONFIG.SHIPPING),
    accessSystemConfig: hasPermission(PERMISSIONS.CONFIG.SYSTEM),
  }), [userPermissions]);

  // Roles específicos
  const is = useMemo(() => ({
    superAdmin: hasRole(ROLES.SUPER_ADMIN.id),
    admin: hasRole(ROLES.ADMIN.id),
    vendedor: hasRole(ROLES.VENDEDOR.id),
    cliente: hasRole(ROLES.CLIENTE.id),
  }), [userRoles]);

  // Acceso a módulos completos
  const canAccess = useMemo(() => ({
    dashboard: hasPermission(PERMISSIONS.DASHBOARD.VIEW),
    users: hasAnyPermission([PERMISSIONS.USERS.VIEW, PERMISSIONS.USERS.LIST]),
    roles: hasAnyPermission([PERMISSIONS.ROLES.VIEW, PERMISSIONS.ROLES.LIST]),
    products: hasAnyPermission([PERMISSIONS.PRODUCTS.VIEW, PERMISSIONS.PRODUCTS.LIST]),
    categories: hasAnyPermission([PERMISSIONS.CATEGORIES.VIEW, PERMISSIONS.CATEGORIES.LIST]),
    brands: hasAnyPermission([PERMISSIONS.BRANDS.VIEW, PERMISSIONS.BRANDS.LIST]),
    orders: hasAnyPermission([PERMISSIONS.ORDERS.VIEW, PERMISSIONS.ORDERS.LIST]),
    inventory: hasPermission(PERMISSIONS.INVENTORY.VIEW),
    reports: hasAnyPermission([
      PERMISSIONS.REPORTS.SALES,
      PERMISSIONS.REPORTS.USERS,
      PERMISSIONS.REPORTS.PRODUCTS,
      PERMISSIONS.REPORTS.FINANCIAL
    ]),
    config: hasAnyPermission([
      PERMISSIONS.CONFIG.GENERAL,
      PERMISSIONS.CONFIG.PAYMENTS,
      PERMISSIONS.CONFIG.SHIPPING,
      PERMISSIONS.CONFIG.SYSTEM
    ]),
  }), [userPermissions]);

  return {
    // Datos del usuario
    user,
    userPermissions,
    userRoles,

    // Funciones de verificación
    hasPermission,
    hasAnyPermission,
    hasAllPermissions,
    hasRole,
    hasAnyRole,

    // Helpers específicos
    can,
    is,
    canAccess,

    // Información útil
    isAuthenticated: !!user,
    hasAnyPermissions: userPermissions.length > 0,
    hasAnyRoles: userRoles.length > 0,
  };
};

export default usePermissions;