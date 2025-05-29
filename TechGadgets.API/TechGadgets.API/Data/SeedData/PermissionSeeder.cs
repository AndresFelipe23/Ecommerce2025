// Data/SeedData/PermissionSeeder.cs
using Microsoft.EntityFrameworkCore;
using TechGadgets.API.Data.Context;
using TechGadgets.API.Models.Entities;

namespace TechGadgets.API.Data.SeedData
{
    public static class PermissionSeeder
    {
        public static async Task SeedPermissionsAsync(TechGadgetsDbContext context)
        {
            if (await context.Permisos.AnyAsync())
                return; // Ya hay permisos, no sembrar

            var permisos = new List<Permiso>
            {
                // Permisos de Usuarios
                new() { PerCodigo = "usuarios.listar", PerNombre = "Listar Usuarios", PerDescripcion = "Ver lista de usuarios", PerModulo = "usuarios", PerActivo = true },
                new() { PerCodigo = "usuarios.ver", PerNombre = "Ver Usuario", PerDescripcion = "Ver detalles de un usuario", PerModulo = "usuarios", PerActivo = true },
                new() { PerCodigo = "usuarios.crear", PerNombre = "Crear Usuario", PerDescripcion = "Crear nuevos usuarios", PerModulo = "usuarios", PerActivo = true },
                new() { PerCodigo = "usuarios.editar", PerNombre = "Editar Usuario", PerDescripcion = "Modificar usuarios existentes", PerModulo = "usuarios", PerActivo = true },
                new() { PerCodigo = "usuarios.eliminar", PerNombre = "Eliminar Usuario", PerDescripcion = "Eliminar usuarios del sistema", PerModulo = "usuarios", PerActivo = true },
                new() { PerCodigo = "usuarios.bloquear", PerNombre = "Bloquear Usuario", PerDescripcion = "Bloquear/desbloquear usuarios", PerModulo = "usuarios", PerActivo = true },

                // Permisos de Roles
                new() { PerCodigo = "roles.listar", PerNombre = "Listar Roles", PerDescripcion = "Ver lista de roles", PerModulo = "roles", PerActivo = true },
                new() { PerCodigo = "roles.ver", PerNombre = "Ver Rol", PerDescripcion = "Ver detalles de un rol", PerModulo = "roles", PerActivo = true },
                new() { PerCodigo = "roles.crear", PerNombre = "Crear Rol", PerDescripcion = "Crear nuevos roles", PerModulo = "roles", PerActivo = true },
                new() { PerCodigo = "roles.editar", PerNombre = "Editar Rol", PerDescripcion = "Modificar roles existentes", PerModulo = "roles", PerActivo = true },
                new() { PerCodigo = "roles.eliminar", PerNombre = "Eliminar Rol", PerDescripcion = "Eliminar roles del sistema", PerModulo = "roles", PerActivo = true },
                new() { PerCodigo = "roles.asignar", PerNombre = "Asignar Roles", PerDescripcion = "Asignar roles a usuarios", PerModulo = "roles", PerActivo = true },
                new() { PerCodigo = "roles.remover", PerNombre = "Remover Roles", PerDescripcion = "Remover roles de usuarios", PerModulo = "roles", PerActivo = true },
                new() { PerCodigo = "permisos.listar", PerNombre = "Listar Permisos", PerDescripcion = "Ver lista de permisos", PerModulo = "roles", PerActivo = true },

                // Permisos de Productos
                new() { PerCodigo = "productos.listar", PerNombre = "Listar Productos", PerDescripcion = "Ver catálogo de productos", PerModulo = "productos", PerActivo = true },
                new() { PerCodigo = "productos.ver", PerNombre = "Ver Producto", PerDescripcion = "Ver detalles de un producto", PerModulo = "productos", PerActivo = true },
                new() { PerCodigo = "productos.crear", PerNombre = "Crear Producto", PerDescripcion = "Añadir nuevos productos", PerModulo = "productos", PerActivo = true },
                new() { PerCodigo = "productos.editar", PerNombre = "Editar Producto", PerDescripcion = "Modificar productos existentes", PerModulo = "productos", PerActivo = true },
                new() { PerCodigo = "productos.eliminar", PerNombre = "Eliminar Producto", PerDescripcion = "Eliminar productos del catálogo", PerModulo = "productos", PerActivo = true },
                new() { PerCodigo = "productos.publicar", PerNombre = "Publicar Producto", PerDescripcion = "Publicar/despublicar productos", PerModulo = "productos", PerActivo = true },

                // Permisos de Categorías
                new() { PerCodigo = "categorias.listar", PerNombre = "Listar Categorías", PerDescripcion = "Ver lista de categorías", PerModulo = "productos", PerActivo = true },
                new() { PerCodigo = "categorias.crear", PerNombre = "Crear Categoría", PerDescripcion = "Crear nuevas categorías", PerModulo = "productos", PerActivo = true },
                new() { PerCodigo = "categorias.editar", PerNombre = "Editar Categoría", PerDescripcion = "Modificar categorías", PerModulo = "productos", PerActivo = true },
                new() { PerCodigo = "categorias.eliminar", PerNombre = "Eliminar Categoría", PerDescripcion = "Eliminar categorías", PerModulo = "productos", PerActivo = true },

                // Permisos de Pedidos
                new() { PerCodigo = "pedidos.listar", PerNombre = "Listar Pedidos", PerDescripcion = "Ver lista de pedidos", PerModulo = "pedidos", PerActivo = true },
                new() { PerCodigo = "pedidos.ver", PerNombre = "Ver Pedido", PerDescripcion = "Ver detalles de un pedido", PerModulo = "pedidos", PerActivo = true },
                new() { PerCodigo = "pedidos.crear", PerNombre = "Crear Pedido", PerDescripcion = "Realizar nuevos pedidos", PerModulo = "pedidos", PerActivo = true },
                new() { PerCodigo = "pedidos.editar", PerNombre = "Editar Pedido", PerDescripcion = "Modificar pedidos existentes", PerModulo = "pedidos", PerActivo = true },
                new() { PerCodigo = "pedidos.cancelar", PerNombre = "Cancelar Pedido", PerDescripcion = "Cancelar pedidos", PerModulo = "pedidos", PerActivo = true },
                new() { PerCodigo = "pedidos.procesar", PerNombre = "Procesar Pedido", PerDescripcion = "Cambiar estado de pedidos", PerModulo = "pedidos", PerActivo = true },

                // Permisos de Inventario
                new() { PerCodigo = "inventario.ver", PerNombre = "Ver Inventario", PerDescripcion = "Ver niveles de inventario", PerModulo = "inventario", PerActivo = true },
                new() { PerCodigo = "inventario.actualizar", PerNombre = "Actualizar Inventario", PerDescripcion = "Modificar cantidades de inventario", PerModulo = "inventario", PerActivo = true },
                new() { PerCodigo = "inventario.alertas", PerNombre = "Ver Alertas Inventario", PerDescripcion = "Ver alertas de stock bajo", PerModulo = "inventario", PerActivo = true },

                // Permisos de Reportes
                new() { PerCodigo = "reportes.ventas", PerNombre = "Reportes de Ventas", PerDescripcion = "Ver reportes de ventas", PerModulo = "reportes", PerActivo = true },
                new() { PerCodigo = "reportes.usuarios", PerNombre = "Reportes de Usuarios", PerDescripcion = "Ver reportes de usuarios", PerModulo = "reportes", PerActivo = true },
                new() { PerCodigo = "reportes.productos", PerNombre = "Reportes de Productos", PerDescripcion = "Ver reportes de productos", PerModulo = "reportes", PerActivo = true },
                new() { PerCodigo = "reportes.financieros", PerNombre = "Reportes Financieros", PerDescripcion = "Ver reportes financieros", PerModulo = "reportes", PerActivo = true },

                // Permisos de Configuración
                new() { PerCodigo = "config.general", PerNombre = "Configuración General", PerDescripcion = "Acceder a configuración general", PerModulo = "configuracion", PerActivo = true },
                new() { PerCodigo = "config.pagos", PerNombre = "Configuración de Pagos", PerDescripcion = "Configurar métodos de pago", PerModulo = "configuracion", PerActivo = true },
                new() { PerCodigo = "config.envios", PerNombre = "Configuración de Envíos", PerDescripcion = "Configurar métodos de envío", PerModulo = "configuracion", PerActivo = true },
                new() { PerCodigo = "config.sistema", PerNombre = "Configuración del Sistema", PerDescripcion = "Acceso a configuración del sistema", PerModulo = "configuracion", PerActivo = true }
            };

            await context.Permisos.AddRangeAsync(permisos);
            await context.SaveChangesAsync();

            Console.WriteLine($"✅ {permisos.Count} permisos creados exitosamente");
        }

        public static async Task SeedDefaultRolesAsync(TechGadgetsDbContext context)
        {
            if (await context.Roles.AnyAsync())
                return; // Ya hay roles

            var roles = new List<Role>
            {
                new() { 
                    RolNombre = "SuperAdmin", 
                    RolDescripcion = "Acceso completo al sistema", 
                    RolActivo = true, 
                    RolFechaCreacion = DateTime.UtcNow
                },
                new() { 
                    RolNombre = "Admin", 
                    RolDescripcion = "Administrador del sistema", 
                    RolActivo = true, 
                    RolFechaCreacion = DateTime.UtcNow
                },
                new() { 
                    RolNombre = "Vendedor", 
                    RolDescripcion = "Gestión de productos y pedidos", 
                    RolActivo = true, 
                    RolFechaCreacion = DateTime.UtcNow
                },
                new() { 
                    RolNombre = "Cliente", 
                    RolDescripcion = "Usuario cliente del e-commerce", 
                    RolActivo = true, 
                    RolFechaCreacion = DateTime.UtcNow
                }
            };

            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();

            // Asignar todos los permisos al SuperAdmin
            var superAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.RolNombre == "SuperAdmin");
            var allPermissions = await context.Permisos.Where(p => p.PerActivo == true).ToListAsync();

            if (superAdminRole != null)
            {
                var rolePermissions = allPermissions.Select(p => new RolesPermiso
                {
                    RpeRolId = superAdminRole.RolId,
                    RpePermisoCodigo = p.PerCodigo,
                    RpeFechaAsignacion = DateTime.UtcNow
                }).ToList();

                await context.RolesPermisos.AddRangeAsync(rolePermissions);
                await context.SaveChangesAsync();
            }

            Console.WriteLine($"✅ {roles.Count} roles predeterminados creados");
        }
    }
}