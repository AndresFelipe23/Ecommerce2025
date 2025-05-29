SELECT
    *
FROM
    Roles;

SELECT
    *
FROM
    Usuarios;

SELECT * FROM Roles
SELECT
    *
FROM
    UsuariosRoles;

SELECT * FROM Permisos;

SELECT * FROM Marcas;


SELECT * FROM Productos;
SELECT COUNT(*) FROM Productos WHERE PrdActivo = 1;
-- 1. CONSULTAR USUARIOS Y SUS ROLES ACTUALES
-- =====================================================
SELECT 
    u.UsuId,
    u.UsuEmail,
    u.UsuNombre + ' ' + u.UsuApellido AS NombreCompleto,
    r.RolNombre,
    ur.UsrFechaAsignacion,
    ur.UsrActivo
FROM Usuarios u
LEFT JOIN UsuariosRoles ur ON u.UsuId = ur.UsrUsuarioId AND ur.UsrActivo = 1
LEFT JOIN Roles r ON ur.UsrRolId = r.RolId
WHERE u.UsuActivo = 1
ORDER BY u.UsuEmail;

-- 2. ASIGNAR ROL "SuperAdmin" A UN USUARIO ESPECÍFICO (por email)
-- =====================================================
DECLARE @UsuarioId INT;
DECLARE @RolId INT;
DECLARE @Email NVARCHAR(255) = 'administrador@admin.com'; -- Cambiar por el email del usuario

-- Obtener IDs
SELECT @UsuarioId = UsuId FROM Usuarios WHERE UsuEmail = @Email AND UsuActivo = 1;
SELECT @RolId = RolId FROM Roles WHERE RolNombre = 'SuperAdmin' AND RolActivo = 1;

-- Verificar que existan
IF @UsuarioId IS NOT NULL AND @RolId IS NOT NULL
BEGIN
    -- Desactivar roles existentes del usuario
    UPDATE UsuariosRoles 
    SET UsrActivo = 0 
    WHERE UsrUsuarioId = @UsuarioId;

    -- Asignar nuevo rol
    IF NOT EXISTS (SELECT 1 FROM UsuariosRoles WHERE UsrUsuarioId = @UsuarioId AND UsrRolId = @RolId)
    BEGIN
        INSERT INTO UsuariosRoles (UsrUsuarioId, UsrRolId, UsrFechaAsignacion, UsrActivo)
        VALUES (@UsuarioId, @RolId, GETDATE(), 1);
        PRINT 'Rol SuperAdmin asignado exitosamente al usuario: ' + @Email;
    END
    ELSE
    BEGIN
        UPDATE UsuariosRoles 
        SET UsrActivo = 1, UsrFechaAsignacion = GETDATE()
        WHERE UsrUsuarioId = @UsuarioId AND UsrRolId = @RolId;
        PRINT 'Rol SuperAdmin reactivado para el usuario: ' + @Email;
    END
END
ELSE
BEGIN
    PRINT 'Error: Usuario o rol no encontrado';
END


SELECT 
    u.UsuEmail,
    r.RolNombre,
    ur.UsrActivo,
    ur.UsrFechaAsignacion
FROM Usuarios u
JOIN UsuariosRoles ur ON u.UsuId = ur.UsrUsuarioId
JOIN Roles r ON ur.UsrRolId = r.RolId
WHERE u.UsuEmail = 'Administrador@admin.com' -- Cambiar por tu email
AND ur.UsrActivo = 1;

SELECT 
    r.RolNombre,
    p.PerCodigo,
    p.PerNombre,
    rp.RpeFechaAsignacion
FROM Roles r
JOIN RolesPermisos rp ON r.RolId = rp.RpeRolId
JOIN Permisos p ON rp.RpePermisoCodigo = p.PerCodigo
WHERE r.RolNombre = 'SuperAdmin'
ORDER BY p.PerCodigo;







-- =====================================================
-- SCRIPT PARA CORREGIR PERMISOS DEL SUPERADMIN
-- =====================================================

-- 1. VERIFICAR ESTADO ACTUAL
-- =====================================================
PRINT '=== DIAGNÓSTICO INICIAL ===';

-- Ver usuarios con email admin@gmail.com
SELECT 
    'USUARIOS' AS Tabla,
    u.UsuId,
    u.UsuEmail,
    u.UsuNombre + ' ' + u.UsuApellido AS NombreCompleto,
    u.UsuActivo
FROM Usuarios u
WHERE u.UsuEmail = 'Administrador@admin.com';

-- Ver roles del usuario
SELECT 
    'ROLES_USUARIO' AS Tabla,
    u.UsuEmail,
    r.RolNombre,
    ur.UsrActivo AS RolActivo,
    ur.UsrFechaAsignacion
FROM Usuarios u
JOIN UsuariosRoles ur ON u.UsuId = ur.UsrUsuarioId
JOIN Roles r ON ur.UsrRolId = r.RolId
WHERE u.UsuEmail = 'Administrador@admin.com';

-- Ver permisos del rol SuperAdmin
SELECT 
    'PERMISOS_SUPERADMIN' AS Tabla,
    COUNT(*) AS TotalPermisos
FROM Roles r
LEFT JOIN RolesPermisos rp ON r.RolId = rp.RpeRolId
WHERE r.RolNombre = 'SuperAdmin';

-- 2. CORREGIR ASIGNACIÓN DE PERMISOS AL SUPERADMIN
-- =====================================================
PRINT '=== CORRIGIENDO PERMISOS ===';

DECLARE @SuperAdminRolId INT;
SELECT @SuperAdminRolId = RolId FROM Roles WHERE RolNombre = 'SuperAdmin';

IF @SuperAdminRolId IS NOT NULL
BEGIN
    -- Eliminar permisos existentes del SuperAdmin
    DELETE FROM RolesPermisos WHERE RpeRolId = @SuperAdminRolId;
    PRINT 'Permisos anteriores eliminados del SuperAdmin';

    -- Asignar TODOS los permisos activos al SuperAdmin
    INSERT INTO RolesPermisos (RpeRolId, RpePermisoCodigo, RpeFechaAsignacion)
    SELECT 
        @SuperAdminRolId,
        p.PerCodigo,
        GETDATE()
    FROM Permisos p
    WHERE p.PerActivo = 1;

    DECLARE @PermisosAsignados INT;
    SELECT @PermisosAsignados = COUNT(*) FROM RolesPermisos WHERE RpeRolId = @SuperAdminRolId;
    PRINT 'SuperAdmin: ' + CAST(@PermisosAsignados AS VARCHAR) + ' permisos asignados';
END
ELSE
BEGIN
    PRINT 'ERROR: Rol SuperAdmin no encontrado';
END

-- 3. VERIFICAR QUE EL USUARIO TENGA ROL SUPERADMIN
-- =====================================================
PRINT '=== VERIFICANDO ASIGNACIÓN DE ROL ===';

DECLARE @UsuarioId INT;
DECLARE @Email NVARCHAR(255) = 'admin@gmail.com'; -- CAMBIAR POR TU EMAIL

SELECT @UsuarioId = UsuId FROM Usuarios WHERE UsuEmail = @Email AND UsuActivo = 1;

IF @UsuarioId IS NOT NULL AND @SuperAdminRolId IS NOT NULL
BEGIN
    -- Verificar si ya tiene el rol
    IF NOT EXISTS (
        SELECT 1 FROM UsuariosRoles 
        WHERE UsrUsuarioId = @UsuarioId 
        AND UsrRolId = @SuperAdminRolId 
        AND UsrActivo = 1
    )
    BEGIN
        -- Desactivar otros roles
        UPDATE UsuariosRoles SET UsrActivo = 0 WHERE UsrUsuarioId = @UsuarioId;

        -- Asignar rol SuperAdmin
        INSERT INTO UsuariosRoles (UsrUsuarioId, UsrRolId, UsrFechaAsignacion, UsrActivo)
        VALUES (@UsuarioId, @SuperAdminRolId, GETDATE(), 1);
        
        PRINT 'Rol SuperAdmin asignado al usuario: ' + @Email;
    END
    ELSE
    BEGIN
        PRINT 'El usuario ya tiene rol SuperAdmin activo';
    END
END
ELSE
BEGIN
    PRINT 'ERROR: Usuario no encontrado o rol SuperAdmin no existe';
END

-- 4. VERIFICACIÓN FINAL
-- =====================================================
PRINT '=== VERIFICACIÓN FINAL ===';

-- Contar permisos del usuario
SELECT 
    u.UsuEmail,
    r.RolNombre,
    COUNT(DISTINCT rp.RpePermisoCodigo) AS TotalPermisos
FROM Usuarios u
JOIN UsuariosRoles ur ON u.UsuId = ur.UsrUsuarioId AND ur.UsrActivo = 1
JOIN Roles r ON ur.UsrRolId = r.RolId AND r.RolActivo = 1
JOIN RolesPermisos rp ON r.RolId = rp.RpeRolId
WHERE u.UsuEmail = @Email
GROUP BY u.UsuEmail, r.RolNombre;

-- Verificar permiso específico
SELECT 
    CASE 
        WHEN EXISTS (
            SELECT 1 FROM Usuarios u
            JOIN UsuariosRoles ur ON u.UsuId = ur.UsrUsuarioId AND ur.UsrActivo = 1
            JOIN RolesPermisos rp ON ur.UsrRolId = rp.RpeRolId
            WHERE u.UsuEmail = @Email AND rp.RpePermisoCodigo = 'roles.listar'
        ) 
        THEN 'SÍ TIENE PERMISO roles.listar'
        ELSE 'NO TIENE PERMISO roles.listar'
    END AS EstadoPermiso;

PRINT '=== SCRIPT COMPLETADO ===';

-- 5. CREAR USUARIO SUPERADMIN DE EMERGENCIA (OPCIONAL)
-- =====================================================
-- Solo descomenta si necesitas crear un nuevo usuario admin

/*
DECLARE @EmergencyEmail NVARCHAR(255) = 'superadmin@emergency.com';
DECLARE @EmergencyPassword NVARCHAR(255) = '$2a$11$YmD8gZ5qF9rJ3Lm8hK7vLOQx9wR5tA6bC3dE2fG1hI4jK5lM6nO7pQ'; -- Hash de "Admin123!"

IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE UsuEmail = @EmergencyEmail)
BEGIN
    INSERT INTO Usuarios (UsuNombre, UsuApellido, UsuEmail, UsuPassword, UsuActivo, UsuFechaCreacion)
    VALUES ('Super', 'Admin', @EmergencyEmail, @EmergencyPassword, 1, GETDATE());
    
    DECLARE @NewAdminId INT = SCOPE_IDENTITY();
    
    -- Asignar rol SuperAdmin
    INSERT INTO UsuariosRoles (UsrUsuarioId, UsrRolId, UsrFechaAsignacion, UsrActivo)
    VALUES (@NewAdminId, @SuperAdminRolId, GETDATE(), 1);
    
    PRINT 'Usuario SuperAdmin de emergencia creado: ' + @EmergencyEmail;
    PRINT 'Password: Admin123!';
END
*/


-- Método rápido: Asignar TODOS los permisos al SuperAdmin

SELECT @SuperAdminId = RolId FROM Roles WHERE RolNombre = 'SuperAdmin';

DELETE FROM RolesPermisos WHERE RpeRolId = @SuperAdminId;

INSERT INTO RolesPermisos (RpeRolId, RpePermisoCodigo, RpeFechaAsignacion)
SELECT @SuperAdminId, PerCodigo, GETDATE()
FROM Permisos WHERE PerActivo = 1;







-- =====================================================
-- CREAR PERMISOS DE ROLES FALTANTES
-- =====================================================

-- 1. Verificar permisos existentes
PRINT '=== PERMISOS EXISTENTES ===';
SELECT 
    PerModulo,
    COUNT(*) as Total
FROM Permisos 
WHERE PerActivo = 1
GROUP BY PerModulo
ORDER BY PerModulo;

-- 2. Crear permisos de roles si no existen
PRINT '=== CREANDO PERMISOS DE ROLES ===';

-- Lista de permisos de roles necesarios
IF NOT EXISTS (SELECT 1 FROM Permisos WHERE PerCodigo = 'roles.listar')
BEGIN
    INSERT INTO Permisos (PerCodigo, PerNombre, PerDescripcion, PerModulo, PerActivo)
    VALUES ('roles.listar', 'Listar Roles', 'Ver lista de roles del sistema', 'roles', 1);
    PRINT '✅ Creado: roles.listar';
END

IF NOT EXISTS (SELECT 1 FROM Permisos WHERE PerCodigo = 'roles.ver')
BEGIN
    INSERT INTO Permisos (PerCodigo, PerNombre, PerDescripcion, PerModulo, PerActivo)
    VALUES ('roles.ver', 'Ver Rol', 'Ver detalles de un rol específico', 'roles', 1);
    PRINT '✅ Creado: roles.ver';
END

IF NOT EXISTS (SELECT 1 FROM Permisos WHERE PerCodigo = 'roles.crear')
BEGIN
    INSERT INTO Permisos (PerCodigo, PerNombre, PerDescripcion, PerModulo, PerActivo)
    VALUES ('roles.crear', 'Crear Rol', 'Crear nuevos roles en el sistema', 'roles', 1);
    PRINT '✅ Creado: roles.crear';
END

IF NOT EXISTS (SELECT 1 FROM Permisos WHERE PerCodigo = 'roles.editar')
BEGIN
    INSERT INTO Permisos (PerCodigo, PerNombre, PerDescripcion, PerModulo, PerActivo)
    VALUES ('roles.editar', 'Editar Rol', 'Modificar roles existentes', 'roles', 1);
    PRINT '✅ Creado: roles.editar';
END

IF NOT EXISTS (SELECT 1 FROM Permisos WHERE PerCodigo = 'roles.eliminar')
BEGIN
    INSERT INTO Permisos (PerCodigo, PerNombre, PerDescripcion, PerModulo, PerActivo)
    VALUES ('roles.eliminar', 'Eliminar Rol', 'Eliminar roles del sistema', 'roles', 1);
    PRINT '✅ Creado: roles.eliminar';
END

IF NOT EXISTS (SELECT 1 FROM Permisos WHERE PerCodigo = 'roles.asignar')
BEGIN
    INSERT INTO Permisos (PerCodigo, PerNombre, PerDescripcion, PerModulo, PerActivo)
    VALUES ('roles.asignar', 'Asignar Roles', 'Asignar roles a usuarios', 'roles', 1);
    PRINT '✅ Creado: roles.asignar';
END

IF NOT EXISTS (SELECT 1 FROM Permisos WHERE PerCodigo = 'roles.remover')
BEGIN
    INSERT INTO Permisos (PerCodigo, PerNombre, PerDescripcion, PerModulo, PerActivo)
    VALUES ('roles.remover', 'Remover Roles', 'Remover roles de usuarios', 'roles', 1);
    PRINT '✅ Creado: roles.remover';
END

IF NOT EXISTS (SELECT 1 FROM Permisos WHERE PerCodigo = 'permisos.listar')
BEGIN
    INSERT INTO Permisos (PerCodigo, PerNombre, PerDescripcion, PerModulo, PerActivo)
    VALUES ('permisos.listar', 'Listar Permisos', 'Ver lista de permisos disponibles', 'roles', 1);
    PRINT '✅ Creado: permisos.listar';
END

-- 3. Crear permisos adicionales que podrían faltar
IF NOT EXISTS (SELECT 1 FROM Permisos WHERE PerCodigo = 'usuarios.listar')
BEGIN
    INSERT INTO Permisos (PerCodigo, PerNombre, PerDescripcion, PerModulo, PerActivo)
    VALUES ('usuarios.listar', 'Listar Usuarios', 'Ver lista de usuarios del sistema', 'usuarios', 1);
    PRINT '✅ Creado: usuarios.listar';
END

-- 4. Asignar TODOS los permisos de roles al SuperAdmin
PRINT '=== ASIGNANDO PERMISOS AL SUPERADMIN ===';

DECLARE @SuperAdminId INT;
SELECT @SuperAdminId = RolId FROM Roles WHERE RolNombre = 'SuperAdmin';

IF @SuperAdminId IS NOT NULL
BEGIN
    -- Asignar permisos de roles
    INSERT INTO RolesPermisos (RpeRolId, RpePermisoCodigo, RpeFechaAsignacion)
    SELECT 
        @SuperAdminId,
        p.PerCodigo,
        GETDATE()
    FROM Permisos p
    WHERE p.PerModulo = 'roles' 
    AND p.PerActivo = 1
    AND NOT EXISTS (
        SELECT 1 FROM RolesPermisos rp 
        WHERE rp.RpeRolId = @SuperAdminId 
        AND rp.RpePermisoCodigo = p.PerCodigo
    );

    -- También asignar usuarios.listar si no lo tiene
    IF NOT EXISTS (
        SELECT 1 FROM RolesPermisos 
        WHERE RpeRolId = @SuperAdminId AND RpePermisoCodigo = 'usuarios.listar'
    )
    BEGIN
        INSERT INTO RolesPermisos (RpeRolId, RpePermisoCodigo, RpeFechaAsignacion)
        VALUES (@SuperAdminId, 'usuarios.listar', GETDATE());
        PRINT '✅ Agregado usuarios.listar al SuperAdmin';
    END

    PRINT '✅ Permisos de roles asignados al SuperAdmin';
END
ELSE
BEGIN
    PRINT '❌ ERROR: Rol SuperAdmin no encontrado';
END

-- 5. Verificación final
PRINT '=== VERIFICACIÓN FINAL ===';

-- Contar permisos de roles del SuperAdmin
SELECT 
    r.RolNombre,
    COUNT(*) as PermisosDeRoles
FROM Roles r
JOIN RolesPermisos rp ON r.RolId = rp.RpeRolId
JOIN Permisos p ON rp.RpePermisoCodigo = p.PerCodigo
WHERE r.RolNombre = 'SuperAdmin'
AND p.PerModulo = 'roles'
GROUP BY r.RolNombre;

-- Mostrar todos los permisos de roles del SuperAdmin
SELECT 
    'PERMISOS_ROLES_SUPERADMIN' as Categoria,
    p.PerCodigo,
    p.PerNombre
FROM Roles r
JOIN RolesPermisos rp ON r.RolId = rp.RpeRolId
JOIN Permisos p ON rp.RpePermisoCodigo = p.PerCodigo
WHERE r.RolNombre = 'SuperAdmin'
AND p.PerModulo = 'roles'
ORDER BY p.PerCodigo;

-- Verificar permiso específico roles.listar
SELECT 
    CASE 
        WHEN EXISTS (
            SELECT 1 FROM Usuarios u
            JOIN UsuariosRoles ur ON u.UsuId = ur.UsrUsuarioId AND ur.UsrActivo = 1
            JOIN RolesPermisos rp ON ur.UsrRolId = rp.RpeRolId
            WHERE u.UsuEmail = 'admin@gmail.com' -- CAMBIAR POR TU EMAIL
            AND rp.RpePermisoCodigo = 'roles.listar'
        ) 
        THEN '✅ USUARIO TIENE roles.listar'
        ELSE '❌ USUARIO NO TIENE roles.listar'
    END AS EstadoFinal;

PRINT '=== SCRIPT COMPLETADO ===';
PRINT 'Ahora haz LOGOUT y LOGIN nuevamente para obtener JWT actualizado';


-- Agregar permisos específicos de marcas
INSERT INTO Permisos (PerCodigo, PerNombre, PerDescripcion, PerModulo, PerActivo)
VALUES 
    ('marcas.crear', 'Crear Marcas', 'Crear nuevas marcas', 'productos', 1),
    ('marcas.editar', 'Editar Marcas', 'Modificar marcas existentes', 'productos', 1),
    ('marcas.eliminar', 'Eliminar Marcas', 'Eliminar marcas', 'productos', 1),
    ('marcas.ver', 'Ver Marcas', 'Ver lista de marcas', 'productos', 1);

-- Asignar al SuperAdmin
DECLARE @SuperAdminId INT;
SELECT @SuperAdminId = RolId FROM Roles WHERE RolNombre = 'SuperAdmin';

INSERT INTO RolesPermisos (RpeRolId, RpePermisoCodigo, RpeFechaAsignacion)
VALUES 
    (@SuperAdminId, 'marcas.crear', GETDATE()),
    (@SuperAdminId, 'marcas.editar', GETDATE()),
    (@SuperAdminId, 'marcas.eliminar', GETDATE()),
    (@SuperAdminId, 'marcas.ver', GETDATE());


    -- Ejecutar en tu base de datos:
SELECT * FROM Permisos WHERE PerCodigo IN ('productos.listar', 'productos.ver');

-- Verificar si existe la categoría ID 7
SELECT * FROM Categorias WHERE CatId = 7;

-- Verificar si existe la marca ID 8
SELECT * FROM Marcas WHERE MarId = 8;

-- Verificar el producto completo
SELECT 
    p.PrdId, p.PrdNombre, p.PrdSku, p.PrdPrecio, p.PrdActivo,
    p.PrdCategoriaId, p.PrdMarcaId,
    c.CatNombre, m.MarNombre,
    i.InvStock
FROM Productos p
LEFT JOIN Categorias c ON p.PrdCategoriaId = c.CatId
LEFT JOIN Marcas m ON p.PrdMarcaId = m.MarId
LEFT JOIN Inventario i ON p.PrdId = i.InvProductoId
WHERE p.PrdId = 1;