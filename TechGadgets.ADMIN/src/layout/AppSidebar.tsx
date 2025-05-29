import { useCallback, useEffect, useMemo } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";

// Assume these icons are imported from an icon library
import {
  BoxCubeIcon,
  CalenderIcon,
  GridIcon,
  HorizontaLDots,
  ListIcon,
  PageIcon,
  PieChartIcon,
  PlugInIcon,
  TableIcon,
  UserCircleIcon,
} from "../icons";
import { useSidebar } from "../context/SidebarContext";
import { useAuth } from "../context/AuthContext";
import { usePermissions } from "../hooks/usePermissions";
import { PERMISSIONS } from "../types/permissions";


type NavItem = {
  name: string;
  icon: React.ReactNode;
  path: string;
  permissions?: string[];
  roles?: string[];
  pro?: boolean;
  new?: boolean;
};

// Configuración del menú principal - todos los items como links directos
const navItems: NavItem[] = [
  {
    icon: <GridIcon />,
    name: "Dashboard",
    path: "/",
    permissions: [PERMISSIONS.DASHBOARD.VIEW]
  },
  {
    icon: <UserCircleIcon />,
    name: "Users List",
    path: "/users",
    permissions: [PERMISSIONS.USERS.LIST]
  },
  {
    icon: <UserCircleIcon />,
    name: "Create User",
    path: "/users/create",
    permissions: [PERMISSIONS.USERS.CREATE]
  },
  {
    icon: <UserCircleIcon />,
    name: "Roles & Permissions",
    path: "/users/roles",
    permissions: [PERMISSIONS.ROLES.LIST]
  },
  {
    icon: <BoxCubeIcon />,
    name: "Products List",
    path: "/products",
    permissions: [PERMISSIONS.PRODUCTS.LIST]
  },
  {
    icon: <BoxCubeIcon />,
    name: "Productos",
    path: "/products",
    permissions: [PERMISSIONS.PRODUCTS.VIEW]
  },
  {
    icon: <BoxCubeIcon />,
    name: "Categorias",
    path: "/categories",
    permissions: [PERMISSIONS.CATEGORIES.VIEW]
  },
  {
    icon: <BoxCubeIcon />,
    name: "Marcas",
    path: "/brands",
    permissions: [PERMISSIONS.BRANDS.VIEW]
  },
  {
    icon: <ListIcon />,
    name: "Orders List",
    path: "/orders",
    permissions: [PERMISSIONS.ORDERS.LIST]
  },
  {
    icon: <ListIcon />,
    name: "Create Order",
    path: "/orders/create",
    permissions: [PERMISSIONS.ORDERS.CREATE]
  },
  {
    icon: <ListIcon />,
    name: "Process Orders",
    path: "/orders/process",
    permissions: [PERMISSIONS.ORDERS.PROCESS]
  },
  {
    icon: <TableIcon />,
    name: "Stock Overview",
    path: "/inventory",
    permissions: [PERMISSIONS.INVENTORY.VIEW]
  },
  {
    icon: <TableIcon />,
    name: "Adjust Stock",
    path: "/inventory/adjust",
    permissions: [PERMISSIONS.INVENTORY.ADJUST]
  },
  {
    icon: <TableIcon />,
    name: "Stock Alerts",
    path: "/inventory/alerts",
    permissions: [PERMISSIONS.INVENTORY.ALERTS]
  },
  {
    icon: <CalenderIcon />,
    name: "Calendar",
    path: "/calendar",
    permissions: [PERMISSIONS.DASHBOARD.VIEW]
  },
];

const othersItems: NavItem[] = [
  {
    icon: <PieChartIcon />,
    name: "Sales Reports",
    path: "/reports/sales",
    permissions: [PERMISSIONS.REPORTS.SALES]
  },
  {
    icon: <PieChartIcon />,
    name: "Product Reports",
    path: "/reports/products",
    permissions: [PERMISSIONS.REPORTS.PRODUCTS]
  },
  {
    icon: <PieChartIcon />,
    name: "User Reports",
    path: "/reports/users",
    permissions: [PERMISSIONS.REPORTS.USERS]
  },
  {
    icon: <PieChartIcon />,
    name: "Financial Reports",
    path: "/reports/financial",
    permissions: [PERMISSIONS.REPORTS.FINANCIAL],
    pro: true
  },
  {
    icon: <PieChartIcon />,
    name: "Line Chart",
    path: "/line-chart",
    permissions: [PERMISSIONS.DASHBOARD.VIEW]
  },
  {
    icon: <PieChartIcon />,
    name: "Bar Chart",
    path: "/bar-chart",
    permissions: [PERMISSIONS.DASHBOARD.VIEW]
  },
  {
    icon: <BoxCubeIcon />,
    name: "Alerts",
    path: "/alerts",
    permissions: [PERMISSIONS.DASHBOARD.VIEW]
  },
  {
    icon: <BoxCubeIcon />,
    name: "Avatar",
    path: "/avatars",
    permissions: [PERMISSIONS.DASHBOARD.VIEW]
  },
  {
    icon: <BoxCubeIcon />,
    name: "Badge",
    path: "/badge",
    permissions: [PERMISSIONS.DASHBOARD.VIEW]
  },
  {
    icon: <BoxCubeIcon />,
    name: "Buttons",
    path: "/buttons",
    permissions: [PERMISSIONS.DASHBOARD.VIEW]
  },
  {
    icon: <BoxCubeIcon />,
    name: "Images",
    path: "/images",
    permissions: [PERMISSIONS.DASHBOARD.VIEW]
  },
  {
    icon: <BoxCubeIcon />,
    name: "Videos",
    path: "/videos",
    permissions: [PERMISSIONS.DASHBOARD.VIEW]
  },
  {
    icon: <PlugInIcon />,
    name: "General Settings",
    path: "/admin/settings",
    permissions: [PERMISSIONS.CONFIG.GENERAL]
  },
  {
    icon: <PlugInIcon />,
    name: "Payment Config",
    path: "/admin/payments",
    permissions: [PERMISSIONS.CONFIG.PAYMENTS]
  },
  {
    icon: <PlugInIcon />,
    name: "Shipping Config",
    path: "/admin/shipping",
    permissions: [PERMISSIONS.CONFIG.SHIPPING]
  },
  {
    icon: <PlugInIcon />,
    name: "System Config",
    path: "/admin/system",
    permissions: [PERMISSIONS.CONFIG.SYSTEM],
    roles: ['SuperAdmin']
  },
  // Dev Pages
  {
    icon: <PageIcon />,
    name: "Form Elements",
    path: "/form-elements",
    permissions: [PERMISSIONS.DASHBOARD.VIEW]
  },
  {
    icon: <PageIcon />,
    name: "Basic Tables",
    path: "/basic-tables",
    permissions: [PERMISSIONS.DASHBOARD.VIEW]
  },
  {
    icon: <PageIcon />,
    name: "Blank Page",
    path: "/blank",
    permissions: [PERMISSIONS.DASHBOARD.VIEW]
  },
  {
    icon: <PageIcon />,
    name: "404 Error",
    path: "/error-404",
    permissions: [PERMISSIONS.DASHBOARD.VIEW]
  },
];

const AppSidebar: React.FC = () => {
  const { isExpanded, isMobileOpen, isHovered, setIsHovered } = useSidebar();
  const { user, logout } = useAuth();
  const { hasAnyPermission, hasAnyRole } = usePermissions();
  const location = useLocation();
  const navigate = useNavigate();

  const isActive = useCallback(
    (path: string) => location.pathname === path,
    [location.pathname]
  );

  // Función para verificar si el usuario tiene acceso a un elemento del menú
  const hasAccess = useCallback((item: NavItem): boolean => {
    // Verificar permisos
    if (item.permissions && item.permissions.length > 0) {
      if (!hasAnyPermission(item.permissions)) return false;
    }

    // Verificar roles
    if (item.roles && item.roles.length > 0) {
      if (!hasAnyRole(item.roles)) return false;
    }

    return true;
  }, [hasAnyPermission, hasAnyRole]);

  // Filtrar elementos del menú basado en permisos
  const filteredNavItems = useMemo(() => {
    return navItems.filter(hasAccess);
  }, [hasAccess]);

  const filteredOthersItems = useMemo(() => {
    return othersItems.filter(hasAccess);
  }, [hasAccess]);

  // Debug: mostrar permisos del usuario
  useEffect(() => {
    if (user) {
      console.log('👤 Usuario actual:', user.nombreCompleto);
      console.log('🎭 Roles:', user.roles);
      console.log('🔑 Permisos:', user.permisos);
      console.log('📋 Items del menú filtrados:', {
        main: filteredNavItems.length,
        others: filteredOthersItems.length
      });
    }
  }, [user, filteredNavItems.length, filteredOthersItems.length]);

  const handleLogout = () => {
    console.log('🚪 Cerrando sesión...');
    logout();
    navigate('/signin');
  };

  const renderMenuItems = (items: NavItem[]) => (
    <ul className="flex flex-col gap-2">
      {items.map((nav, index) => (
        <li key={`${nav.name}-${index}`}>
          <Link
            to={nav.path}
            className={`menu-item group ${
              isActive(nav.path) ? "menu-item-active" : "menu-item-inactive"
            }`}
          >
            <span
              className={`menu-item-icon-size ${
                isActive(nav.path)
                  ? "menu-item-icon-active"
                  : "menu-item-icon-inactive"
              }`}
            >
              {nav.icon}
            </span>
            {(isExpanded || isHovered || isMobileOpen) && (
              <>
                <span className="menu-item-text">{nav.name}</span>
                <span className="flex items-center gap-1 ml-auto">
                  {nav.new && (
                    <span
                      className={`${
                        isActive(nav.path)
                          ? "menu-dropdown-badge-active"
                          : "menu-dropdown-badge-inactive"
                      } menu-dropdown-badge`}
                    >
                      new
                    </span>
                  )}
                  {nav.pro && (
                    <span
                      className={`${
                        isActive(nav.path)
                          ? "menu-dropdown-badge-active"
                          : "menu-dropdown-badge-inactive"
                      } menu-dropdown-badge`}
                    >
                      pro
                    </span>
                  )}
                </span>
              </>
            )}
          </Link>
        </li>
      ))}
    </ul>
  );

  // Si no hay elementos para mostrar, mostrar mensaje de debug
  if (filteredNavItems.length === 0 && filteredOthersItems.length === 0) {
    return (
      <aside className="fixed mt-16 flex flex-col lg:mt-0 top-0 px-5 left-0 bg-white dark:bg-gray-900 dark:border-gray-800 text-gray-900 h-screen w-[290px] border-r border-gray-200 dark:border-gray-700">
        <div className="p-6 text-center">
          <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-2">
            Sin elementos de menú
          </h3>
          <p className="text-sm text-gray-600 dark:text-gray-400 mb-4">
            El usuario no tiene permisos para ver ningún elemento del menú.
          </p>
          {user && (
            <div className="text-xs text-gray-500 dark:text-gray-400 space-y-1 mb-4">
              <p><strong>Usuario:</strong> {user.nombreCompleto}</p>
              <p><strong>Roles:</strong> {user.roles?.join(', ') || 'Ninguno'}</p>
              <p><strong>Permisos:</strong> {user.permisos?.length || 0}</p>
              <div className="mt-2 max-h-32 overflow-y-auto">
                <p><strong>Lista de permisos:</strong></p>
                <ul className="text-left space-y-1">
                  {user.permisos?.map((permiso, i) => (
                    <li key={i} className="text-xs">• {permiso}</li>
                  ))}
                </ul>
              </div>
            </div>
          )}
          <button
            onClick={handleLogout}
            className="mt-4 px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg text-sm transition-colors"
          >
            Logout
          </button>
        </div>
      </aside>
    );
  }

  return (
    <aside
      className={`fixed mt-16 flex flex-col lg:mt-0 top-0 px-5 left-0 bg-white dark:bg-gray-900 dark:border-gray-800 text-gray-900 h-screen transition-all duration-300 ease-in-out z-50 border-r border-gray-200 
        ${
          isExpanded || isMobileOpen
            ? "w-[290px]"
            : isHovered
            ? "w-[290px]"
            : "w-[90px]"
        }
        ${isMobileOpen ? "translate-x-0" : "-translate-x-full"}
        lg:translate-x-0`}
      onMouseEnter={() => !isExpanded && setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
    >
      {/* Logo */}
      <div
        className={`py-8 flex ${
          !isExpanded && !isHovered ? "lg:justify-center" : "justify-start"
        }`}
      >
        <Link to="/">
          {isExpanded || isHovered || isMobileOpen ? (
            <>
              <img
                className="dark:hidden"
                src="/images/logo/logo.svg"
                alt="Logo"
                width={150}
                height={40}
              />
              <img
                className="hidden dark:block"
                src="/images/logo/logo-dark.svg"
                alt="Logo"
                width={150}
                height={40}
              />
            </>
          ) : (
            <img
              src="/images/logo/logo-icon.svg"
              alt="Logo"
              width={32}
              height={32}
            />
          )}
        </Link>
      </div>

      {/* Contenido principal del sidebar */}
      <div className="flex flex-col overflow-y-auto duration-300 ease-linear no-scrollbar">
        <nav className="mb-6">
          <div className="flex flex-col gap-4">
            {/* Menú principal */}
            {filteredNavItems.length > 0 && (
              <div>
                <h2
                  className={`mb-4 text-xs uppercase flex leading-[20px] text-gray-400 ${
                    !isExpanded && !isHovered
                      ? "lg:justify-center"
                      : "justify-start"
                  }`}
                >
                  {isExpanded || isHovered || isMobileOpen ? (
                    "Menu"
                  ) : (
                    <HorizontaLDots className="size-6" />
                  )}
                </h2>
                {renderMenuItems(filteredNavItems)}
              </div>
            )}

            {/* Menú "Others" */}
            {filteredOthersItems.length > 0 && (
              <div className="">
                <h2
                  className={`mb-4 text-xs uppercase flex leading-[20px] text-gray-400 ${
                    !isExpanded && !isHovered
                      ? "lg:justify-center"
                      : "justify-start"
                  }`}
                >
                  {isExpanded || isHovered || isMobileOpen ? (
                    "Others"
                  ) : (
                    <HorizontaLDots />
                  )}
                </h2>
                {renderMenuItems(filteredOthersItems)}
              </div>
            )}
          </div>
        </nav>

        {/* Sección de usuario y logout */}
        <div className="mt-auto mb-4">
          {(isExpanded || isHovered || isMobileOpen) && (
            <div className="border-t border-gray-200 dark:border-gray-700 pt-4">
              {/* Información del usuario */}
              <div className="flex items-center gap-3 px-3 py-2 mb-3 bg-gray-50 dark:bg-gray-800 rounded-lg">
                <div className="w-10 h-10 bg-gradient-to-r from-blue-500 to-purple-500 rounded-full flex items-center justify-center text-white text-sm font-bold shadow-md">
                  {user?.nombreCompleto?.charAt(0)?.toUpperCase() || 'U'}
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-semibold text-gray-900 dark:text-white truncate">
                    {user?.nombreCompleto || 'Usuario'}
                  </p>
                  <p className="text-xs text-gray-500 dark:text-gray-400 truncate">
                    {user?.email}
                  </p>
                </div>
              </div>

              {/* Roles del usuario */}
              {user?.roles && user.roles.length > 0 && (
                <div className="px-3 py-2 mb-3">
                  <p className="text-xs text-gray-400 mb-2 font-medium">Roles:</p>
                  <div className="flex flex-wrap gap-1">
                    {user.roles.map((role, index) => (
                      <span
                        key={index}
                        className="px-2 py-1 text-xs font-medium bg-gradient-to-r from-blue-100 to-purple-100 text-blue-800 dark:from-blue-900 dark:to-purple-900 dark:text-blue-200 rounded-full border border-blue-200 dark:border-blue-700"
                      >
                        {role}
                      </span>
                    ))}
                  </div>
                </div>
              )}

              {/* Botón de logout */}
              <button
                onClick={handleLogout}
                className="w-full flex items-center gap-3 px-3 py-2 text-sm font-medium text-red-600 hover:text-red-700 hover:bg-red-50 dark:text-red-400 dark:hover:text-red-300 dark:hover:bg-red-900/20 rounded-lg transition-all duration-200 group"
              >
                <svg className="w-5 h-5 group-hover:scale-110 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                </svg>
                Logout
              </button>
            </div>
          )}
          
          {/* Botón de logout colapsado */}
          {!isExpanded && !isHovered && !isMobileOpen && (
            <div className="flex justify-center pb-4">
              <button
                onClick={handleLogout}
                className="w-10 h-10 flex items-center justify-center text-red-600 hover:text-red-700 hover:bg-red-50 dark:text-red-400 dark:hover:text-red-300 dark:hover:bg-red-900/20 rounded-lg transition-colors group"
                title="Logout"
              >
                <svg className="w-5 h-5 group-hover:scale-110 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3-3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                </svg>
              </button>
            </div>
          )}
        </div>

        {/* Widget del sidebar */}
        {isExpanded || isHovered || isMobileOpen}
      </div>
    </aside>
  );
};

export default AppSidebar;