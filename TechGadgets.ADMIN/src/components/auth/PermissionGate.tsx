// components/auth/PermissionGate.tsx
import React from 'react';
import { usePermissions } from '../../hooks/usePermissions';

interface PermissionGateProps {
  children: React.ReactNode;
  
  // Permisos requeridos
  permissions?: string[];
  requireAllPermissions?: boolean; // true = requiere TODOS, false = requiere CUALQUIERA
  
  // Roles requeridos
  roles?: string[];
  requireAllRoles?: boolean; // true = requiere TODOS, false = requiere CUALQUIERA
  
  // Componente a mostrar si no tiene permisos
  fallback?: React.ReactNode;
  
  // Mostrar/ocultar completamente vs mostrar fallback
  hideIfNoAccess?: boolean;
}

const PermissionGate: React.FC<PermissionGateProps> = ({
  children,
  permissions = [],
  requireAllPermissions = false,
  roles = [],
  requireAllRoles = false,
  fallback = null,
  hideIfNoAccess = true,
}) => {
  const { hasAnyPermission, hasAllPermissions, hasRole, hasAnyRole } = usePermissions();

  // Verificar permisos
  let hasRequiredPermissions = true;
  if (permissions.length > 0) {
    if (requireAllPermissions) {
      hasRequiredPermissions = hasAllPermissions(permissions);
    } else {
      hasRequiredPermissions = hasAnyPermission(permissions);
    }
  }

  // Verificar roles
  let hasRequiredRoles = true;
  if (roles.length > 0) {
    if (requireAllRoles) {
      hasRequiredRoles = roles.every(role => hasRole(role));
    } else {
      hasRequiredRoles = hasAnyRole(roles);
    }
  }

  // Determinar si tiene acceso
  const hasAccess = hasRequiredPermissions && hasRequiredRoles;

  if (hasAccess) {
    return <>{children}</>;
  }

  if (hideIfNoAccess) {
    return null;
  }

  return <>{fallback}</>;
};

// Componentes específicos para casos comunes
export const AdminOnly: React.FC<{ children: React.ReactNode; fallback?: React.ReactNode }> = ({ 
  children, 
  fallback = null 
}) => (
  <PermissionGate roles={['super_admin', 'admin']} fallback={fallback}>
    {children}
  </PermissionGate>
);

export const SuperAdminOnly: React.FC<{ children: React.ReactNode; fallback?: React.ReactNode }> = ({ 
  children, 
  fallback = null 
}) => (
  <PermissionGate roles={['super_admin']} fallback={fallback}>
    {children}
  </PermissionGate>
);

export const ManagerAndUp: React.FC<{ children: React.ReactNode; fallback?: React.ReactNode }> = ({ 
  children, 
  fallback = null 
}) => (
  <PermissionGate roles={['super_admin', 'admin', 'manager']} fallback={fallback}>
    {children}
  </PermissionGate>
);

export const StaffOnly: React.FC<{ children: React.ReactNode; fallback?: React.ReactNode }> = ({ 
  children, 
  fallback = null 
}) => (
  <PermissionGate roles={['super_admin', 'admin', 'manager', 'employee']} fallback={fallback}>
    {children}
  </PermissionGate>
);

// Ejemplos de uso específicos por módulo
export const ProductsModule: React.FC<{ children: React.ReactNode }> = ({ children }) => (
  <PermissionGate permissions={['products.view']}>
    {children}
  </PermissionGate>
);

export const ProductManagement: React.FC<{ children: React.ReactNode }> = ({ children }) => (
  <PermissionGate permissions={['products.create', 'products.edit', 'products.delete']}>
    {children}
  </PermissionGate>
);

export const OrdersModule: React.FC<{ children: React.ReactNode }> = ({ children }) => (
  <PermissionGate permissions={['orders.view']}>
    {children}
  </PermissionGate>
);

export const ReportsModule: React.FC<{ children: React.ReactNode }> = ({ children }) => (
  <PermissionGate permissions={['reports.sales.view', 'reports.inventory.view', 'reports.users.view', 'reports.financial.view']}>
    {children}
  </PermissionGate>
);

export default PermissionGate;

/*
EJEMPLOS DE USO:

// Básico - mostrar solo si tiene el permiso
<PermissionGate permissions={['products.create']}>
  <button>Crear Producto</button>
</PermissionGate>

// Con múltiples permisos (cualquiera)
<PermissionGate permissions={['products.create', 'products.edit']}>
  <button>Gestionar Productos</button>
</PermissionGate>

// Requiere TODOS los permisos
<PermissionGate 
  permissions={['products.create', 'products.edit']} 
  requireAllPermissions={true}
>
  <button>Editor Completo</button>
</PermissionGate>

// Por roles
<PermissionGate roles={['admin', 'super_admin']}>
  <AdminPanel />
</PermissionGate>

// Con fallback
<PermissionGate 
  permissions={['products.create']} 
  fallback={<span>No tienes permisos para crear productos</span>}
  hideIfNoAccess={false}
>
  <button>Crear Producto</button>
</PermissionGate>

// Combinando roles y permisos
<PermissionGate 
  roles={['manager']} 
  permissions={['products.view']}
>
  <ProductManager />
</PermissionGate>

// Usando componentes específicos
<AdminOnly>
  <AdminSettings />
</AdminOnly>

<SuperAdminOnly fallback={<span>Solo Super Admin</span>}>
  <SuperAdminPanel />
</SuperAdminOnly>

<StaffOnly>
  <StaffDashboard />
</StaffOnly>
*/