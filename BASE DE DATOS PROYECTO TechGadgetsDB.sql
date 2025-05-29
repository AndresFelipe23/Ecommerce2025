-- =====================================================
-- BASE DE DATOS E-COMMERCE TECH GADGETS
-- =====================================================

-- Crear la base de datos
CREATE DATABASE TechGadgetsDB;
GO

USE TechGadgetsDB;
GO



-- =====================================================
-- TABLAS DE CONFIGURACIÓN Y SISTEMA
-- =====================================================

-- Tabla de Configuraciones del Sistema
CREATE TABLE Configuraciones (
    ConId INT IDENTITY(1,1) PRIMARY KEY,
    ConClave NVARCHAR(100) NOT NULL UNIQUE,
    ConValor NVARCHAR(500) NOT NULL,
    ConDescripcion NVARCHAR(255),
    ConTipo NVARCHAR(50) DEFAULT 'string', -- string, int, bool, decimal
    ConActivo BIT DEFAULT 1,
    ConFechaCreacion DATETIME2 DEFAULT GETDATE(),
    ConFechaModificacion DATETIME2 DEFAULT GETDATE()
);

-- Tabla de Países
CREATE TABLE Paises (
    PaiId INT IDENTITY(1,1) PRIMARY KEY,
    PaiNombre NVARCHAR(100) NOT NULL,
    PaiCodigo NVARCHAR(3) NOT NULL UNIQUE, -- ISO 3166-1 alpha-3
    PaiCodigoTelefono NVARCHAR(5),
    PaiMoneda NVARCHAR(3), -- USD, EUR, etc.
    PaiActivo BIT DEFAULT 1
);

-- Tabla de Estados/Provincias
CREATE TABLE Estados (
    EstId INT IDENTITY(1,1) PRIMARY KEY,
    EstPaisId INT NOT NULL,
    EstNombre NVARCHAR(100) NOT NULL,
    EstCodigo NVARCHAR(10),
    EstActivo BIT DEFAULT 1,
    FOREIGN KEY (EstPaisId) REFERENCES Paises(PaiId)
);

-- Tabla de Ciudades
CREATE TABLE Ciudades (
    CiuId INT IDENTITY(1,1) PRIMARY KEY,
    CiuEstadoId INT NOT NULL,
    CiuNombre NVARCHAR(100) NOT NULL,
    CiuCodigoPostal NVARCHAR(20),
    CiuActivo BIT DEFAULT 1,
    FOREIGN KEY (CiuEstadoId) REFERENCES Estados(EstId)
);

-- =====================================================
-- GESTIÓN DE USUARIOS Y AUTENTICACIÓN
-- =====================================================

-- Tabla de Roles
CREATE TABLE Roles (
    RolId INT IDENTITY(1,1) PRIMARY KEY,
    RolNombre NVARCHAR(50) NOT NULL UNIQUE,
    RolDescripcion NVARCHAR(255),
    RolActivo BIT DEFAULT 1,
    RolFechaCreacion DATETIME2 DEFAULT GETDATE()
);

-- Tabla de Permisos
CREATE TABLE Permisos (
    PerCodigo NVARCHAR(50) PRIMARY KEY,
    PerNombre NVARCHAR(100) NOT NULL,
    PerDescripcion NVARCHAR(255),
    PerModulo NVARCHAR(50), -- productos, pedidos, usuarios, etc.
    PerActivo BIT DEFAULT 1
);

-- Tabla de Permisos por Rol
CREATE TABLE RolesPermisos (
    RpeRolId INT NOT NULL,
    RpePermisoCodigo NVARCHAR(50) NOT NULL,
    RpeFechaAsignacion DATETIME2 DEFAULT GETDATE(),
    PRIMARY KEY (RpeRolId, RpePermisoCodigo),
    FOREIGN KEY (RpeRolId) REFERENCES Roles(RolId),
    FOREIGN KEY (RpePermisoCodigo) REFERENCES Permisos(PerCodigo)
);

-- Tabla de Usuarios
CREATE TABLE Usuarios (
    UsuId INT IDENTITY(1,1) PRIMARY KEY,
    UsuEmail NVARCHAR(255) NOT NULL UNIQUE,
    UsuPassword NVARCHAR(255) NOT NULL, -- Hash
    UsuNombre NVARCHAR(100) NOT NULL,
    UsuApellido NVARCHAR(100) NOT NULL,
    UsuTelefono NVARCHAR(20),
    UsuFechaNacimiento DATE,
    UsuGenero CHAR(1), -- M, F, O
    UsuActivo BIT DEFAULT 1,
    UsuEmailVerificado BIT DEFAULT 0,
    UsuTelefonoVerificado BIT DEFAULT 0,
    UsuFechaCreacion DATETIME2 DEFAULT GETDATE(),
    UsuFechaModificacion DATETIME2 DEFAULT GETDATE(),
    UsuUltimoAcceso DATETIME2,
    UsuIntentosFallidos INT DEFAULT 0,
    UsuBloqueadoHasta DATETIME2 NULL
);

-- Tabla de Usuarios Roles
CREATE TABLE UsuariosRoles (
    UsrUsuarioId INT NOT NULL,
    UsrRolId INT NOT NULL,
    UsrFechaAsignacion DATETIME2 DEFAULT GETDATE(),
    UsrActivo BIT DEFAULT 1,
    PRIMARY KEY (UsrUsuarioId, UsrRolId),
    FOREIGN KEY (UsrUsuarioId) REFERENCES Usuarios(UsuId),
    FOREIGN KEY (UsrRolId) REFERENCES Roles(RolId)
);

-- Tabla de Direcciones
CREATE TABLE Direcciones (
    DirId INT IDENTITY(1,1) PRIMARY KEY,
    DirUsuarioId INT NOT NULL,
    DirTipo NVARCHAR(20) DEFAULT 'envio', -- envio, facturacion, ambos
    DirNombre NVARCHAR(100), -- Casa, Oficina, etc.
    DirDireccionLinea1 NVARCHAR(255) NOT NULL,
    DirDireccionLinea2 NVARCHAR(255),
    DirCiudadId INT NOT NULL,
    DirCodigoPostal NVARCHAR(20),
    DirReferencias NVARCHAR(255),
    DirEsPrincipal BIT DEFAULT 0,
    DirActivo BIT DEFAULT 1,
    DirFechaCreacion DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (DirUsuarioId) REFERENCES Usuarios(UsuId),
    FOREIGN KEY (DirCiudadId) REFERENCES Ciudades(CiuId)
);

-- =====================================================
-- GESTIÓN DE PRODUCTOS Y CATÁLOGO
-- =====================================================

-- Tabla de Marcas
CREATE TABLE Marcas (
    MarId INT IDENTITY(1,1) PRIMARY KEY,
    MarNombre NVARCHAR(100) NOT NULL UNIQUE,
    MarDescripcion NVARCHAR(500),
    MarLogo NVARCHAR(255), -- URL del logo
    MarSitioWeb NVARCHAR(255),
    MarActivo BIT DEFAULT 1,
    MarFechaCreacion DATETIME2 DEFAULT GETDATE()
);

-- Tabla de Categorías
CREATE TABLE Categorias (
    CatId INT IDENTITY(1,1) PRIMARY KEY,
    CatNombre NVARCHAR(100) NOT NULL,
    CatDescripcion NVARCHAR(500),
    CatCategoriaPadreId INT NULL,
    CatImagen NVARCHAR(255), -- URL de imagen
    CatIcono NVARCHAR(50), -- Clase CSS del icono
    CatSlug NVARCHAR(100) NOT NULL UNIQUE, -- URL amigable
    CatOrden INT DEFAULT 0,
    CatActivo BIT DEFAULT 1,
    CatFechaCreacion DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (CatCategoriaPadreId) REFERENCES Categorias(CatId)
);

-- Tabla de Proveedores
CREATE TABLE Proveedores (
    ProId INT IDENTITY(1,1) PRIMARY KEY,
    ProNombre NVARCHAR(150) NOT NULL,
    ProContacto NVARCHAR(100),
    ProEmail NVARCHAR(255),
    ProTelefono NVARCHAR(20),
    ProDireccion NVARCHAR(255),
    ProCiudadId INT,
    ProSitioWeb NVARCHAR(255),
    ProTiempoEntrega INT DEFAULT 7, -- días
    ProTerminosPago NVARCHAR(100), -- 30 días, contado, etc.
    ProActivo BIT DEFAULT 1,
    ProFechaCreacion DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (ProCiudadId) REFERENCES Ciudades(CiuId)
);

-- Tabla de Productos
CREATE TABLE Productos (
    PrdId INT IDENTITY(1,1) PRIMARY KEY,
    PrdSku NVARCHAR(50) NOT NULL UNIQUE,
    PrdNombre NVARCHAR(200) NOT NULL,
    PrdDescripcionCorta NVARCHAR(500),
    PrdDescripcionLarga NTEXT,
    PrdPrecio DECIMAL(18,2) NOT NULL,
    PrdPrecioComparacion DECIMAL(18,2), -- Precio tachado
    PrdCosto DECIMAL(18,2), -- Para cálculo de margen
    PrdMarcaId INT NOT NULL,
    PrdCategoriaId INT NOT NULL,
    PrdTipo NVARCHAR(20) DEFAULT 'simple', -- simple, variable, digital
    PrdEstado NVARCHAR(20) DEFAULT 'borrador', -- borrador, publicado, agotado
    PrdDestacado BIT DEFAULT 0,
    PrdNuevo BIT DEFAULT 0,
    PrdEnOferta BIT DEFAULT 0,
    PrdPeso DECIMAL(8,2), -- en gramos
    PrdDimensiones NVARCHAR(50), -- LxWxH en cm
    PrdSlug NVARCHAR(200) NOT NULL UNIQUE,
    PrdMetaTitulo NVARCHAR(200), -- SEO
    PrdMetaDescripcion NVARCHAR(500), -- SEO
    PrdPalabrasClaves NVARCHAR(500), -- SEO
    PrdRequiereEnvio BIT DEFAULT 1,
    PrdPermiteReseñas BIT DEFAULT 1,
    PrdGarantia NVARCHAR(100), -- 1 año, 6 meses, etc.
    PrdOrden INT DEFAULT 0,
    PrdActivo BIT DEFAULT 1,
    PrdFechaCreacion DATETIME2 DEFAULT GETDATE(),
    PrdFechaModificacion DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (PrdMarcaId) REFERENCES Marcas(MarId),
    FOREIGN KEY (PrdCategoriaId) REFERENCES Categorias(CatId)
);

-- Tabla de Atributos (Color, Tamaño, etc.)
CREATE TABLE Atributos (
    AtrId INT IDENTITY(1,1) PRIMARY KEY,
    AtrNombre NVARCHAR(100) NOT NULL UNIQUE,
    AtrTipo NVARCHAR(20) DEFAULT 'texto', -- texto, color, numero, booleano
    AtrRequerido BIT DEFAULT 0,
    AtrFiltrable BIT DEFAULT 1, -- Aparece en filtros
    AtrOrden INT DEFAULT 0,
    AtrActivo BIT DEFAULT 1
);

-- Tabla de Valores de Atributos
CREATE TABLE ValoresAtributos (
    ValId INT IDENTITY(1,1) PRIMARY KEY,
    ValAtributoId INT NOT NULL,
    ValValor NVARCHAR(100) NOT NULL,
    ValCodigoColor NVARCHAR(7), -- Para colores: #FFFFFF
    ValOrden INT DEFAULT 0,
    ValActivo BIT DEFAULT 1,
    FOREIGN KEY (ValAtributoId) REFERENCES Atributos(AtrId)
);

-- Tabla de Variantes de Productos
CREATE TABLE ProductosVariantes (
    PvaId INT IDENTITY(1,1) PRIMARY KEY,
    PvaProductoId INT NOT NULL,
    PvaSku NVARCHAR(50) NOT NULL UNIQUE,
    PvaNombre NVARCHAR(200),
    PvaPrecio DECIMAL(18,2),
    PvaCosto DECIMAL(18,2),
    PvaPeso DECIMAL(8,2),
    PvaOrden INT DEFAULT 0,
    PvaActivo BIT DEFAULT 1,
    FOREIGN KEY (PvaProductoId) REFERENCES Productos(PrdId)
);

-- Tabla de Atributos por Variante
CREATE TABLE VariantesAtributos (
    VatVarianteId INT NOT NULL,
    VatValorAtributoId INT NOT NULL,
    PRIMARY KEY (VatVarianteId, VatValorAtributoId),
    FOREIGN KEY (VatVarianteId) REFERENCES ProductosVariantes(PvaId),
    FOREIGN KEY (VatValorAtributoId) REFERENCES ValoresAtributos(ValId)
);

-- Tabla de Imágenes de Productos
CREATE TABLE ProductosImagenes (
    PimId INT IDENTITY(1,1) PRIMARY KEY,
    PimProductoId INT NOT NULL,
    PimVarianteId INT NULL, -- Para imágenes específicas de variante
    PimUrl NVARCHAR(500) NOT NULL,
    PimTextoAlternativo NVARCHAR(255),
    PimEsPrincipal BIT DEFAULT 0,
    PimOrden INT DEFAULT 0,
    PimActivo BIT DEFAULT 1,
    FOREIGN KEY (PimProductoId) REFERENCES Productos(PrdId),
    FOREIGN KEY (PimVarianteId) REFERENCES ProductosVariantes(PvaId)
);

-- Tabla de Productos por Proveedor
CREATE TABLE ProductosProveedores (
    PprProductoId INT NOT NULL,
    PprProveedorId INT NOT NULL,
    PprSkuProveedor NVARCHAR(50),
    PprCosto DECIMAL(18,2) NOT NULL,
    PprTiempoEntrega INT DEFAULT 7,
    PprStockMinimo INT DEFAULT 0,
    PprEsPrincipal BIT DEFAULT 0,
    PprActivo BIT DEFAULT 1,
    PprFechaCreacion DATETIME2 DEFAULT GETDATE(),
    PRIMARY KEY (PprProductoId, PprProveedorId),
    FOREIGN KEY (PprProductoId) REFERENCES Productos(PrdId),
    FOREIGN KEY (PprProveedorId) REFERENCES Proveedores(ProId)
);

-- =====================================================
-- GESTIÓN DE INVENTARIO
-- =====================================================

-- Tabla de Inventario
CREATE TABLE Inventario (
    InvId INT IDENTITY(1,1) PRIMARY KEY,
    InvProductoId INT NOT NULL,
    InvVarianteId INT NULL,
    InvStock INT NOT NULL DEFAULT 0,
    InvStockMinimo INT DEFAULT 5,
    InvStockMaximo INT DEFAULT 100,
    InvStockReservado INT DEFAULT 0, -- Para pedidos pendientes
    InvUbicacion NVARCHAR(50), -- Almacén, estante, etc.
    InvFechaUltimaActualizacion DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (InvProductoId) REFERENCES Productos(PrdId),
    FOREIGN KEY (InvVarianteId) REFERENCES ProductosVariantes(PvaId)
);

-- Tabla de Movimientos de Inventario
CREATE TABLE MovimientosInventario (
    MovId INT IDENTITY(1,1) PRIMARY KEY,
    MovInventarioId INT NOT NULL,
    MovTipo NVARCHAR(20) NOT NULL, -- entrada, salida, ajuste, reserva
    MovCantidad INT NOT NULL,
    MovCantidadAnterior INT NOT NULL,
    MovReferencia NVARCHAR(100), -- ID de pedido, compra, etc.
    MovMotivo NVARCHAR(255),
    MovUsuarioId INT,
    MovFecha DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (MovInventarioId) REFERENCES Inventario(InvId),
    FOREIGN KEY (MovUsuarioId) REFERENCES Usuarios(UsuId)
);

-- =====================================================
-- GESTIÓN DE CARRITO Y WISHLIST
-- =====================================================

-- Tabla de Carrito
CREATE TABLE Carrito (
    CarId INT IDENTITY(1,1) PRIMARY KEY,
    CarUsuarioId INT NULL, -- NULL para invitados
    CarSesionId NVARCHAR(100) NULL, -- Para invitados
    CarProductoId INT NOT NULL,
    CarVarianteId INT NULL,
    CarCantidad INT NOT NULL DEFAULT 1,
    CarPrecio DECIMAL(18,2) NOT NULL, -- Precio al momento de agregar
    CarFechaCreacion DATETIME2 DEFAULT GETDATE(),
    CarFechaModificacion DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (CarUsuarioId) REFERENCES Usuarios(UsuId),
    FOREIGN KEY (CarProductoId) REFERENCES Productos(PrdId),
    FOREIGN KEY (CarVarianteId) REFERENCES ProductosVariantes(PvaId)
);

-- Tabla de Lista de Deseos
CREATE TABLE ListaDeseos (
    LisId INT IDENTITY(1,1) PRIMARY KEY,
    LisUsuarioId INT NOT NULL,
    LisProductoId INT NOT NULL,
    LisVarianteId INT NULL,
    LisFechaCreacion DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (LisUsuarioId) REFERENCES Usuarios(UsuId),
    FOREIGN KEY (LisProductoId) REFERENCES Productos(PrdId),
    FOREIGN KEY (LisVarianteId) REFERENCES ProductosVariantes(PvaId),
    UNIQUE (LisUsuarioId, LisProductoId, LisVarianteId)
);

-- =====================================================
-- GESTIÓN DE PEDIDOS Y VENTAS
-- =====================================================

-- Tabla de Estados de Pedidos
CREATE TABLE EstadosPedidos (
    EpeCodigo NVARCHAR(20) PRIMARY KEY,
    EpeNombre NVARCHAR(50) NOT NULL,
    EpeDescripcion NVARCHAR(255),
    EpeColor NVARCHAR(7) DEFAULT '#666666', -- Color hexadecimal
    EpeOrden INT DEFAULT 0,
    EpeActivo BIT DEFAULT 1
);

-- Tabla de Métodos de Pago
CREATE TABLE MetodosPago (
    MpaId INT IDENTITY(1,1) PRIMARY KEY,
    MpaNombre NVARCHAR(50) NOT NULL,
    MpaDescripcion NVARCHAR(255),
    MpaTipo NVARCHAR(20), -- tarjeta, transferencia, efectivo, paypal
    MpaComision DECIMAL(5,2) DEFAULT 0, -- Porcentaje
    MpaActivo BIT DEFAULT 1,
    MpaOrden INT DEFAULT 0
);

-- Tabla de Métodos de Envío
CREATE TABLE MetodosEnvio (
    MenId INT IDENTITY(1,1) PRIMARY KEY,
    MenNombre NVARCHAR(100) NOT NULL,
    MenDescripcion NVARCHAR(255),
    MenTipo NVARCHAR(20), -- fijo, por_peso, por_valor, gratis
    MenCosto DECIMAL(18,2) DEFAULT 0,
    MenCostoAdicional DECIMAL(18,2) DEFAULT 0, -- Por kg adicional
    MenPesoMaximo DECIMAL(8,2), -- kg
    MenValorMinimo DECIMAL(18,2), -- Para envío gratis
    MenTiempoEntrega NVARCHAR(50), -- 1-3 días, 24 horas
    MenActivo BIT DEFAULT 1,
    MenOrden INT DEFAULT 0
);

-- Tabla de Zonas de Envío
CREATE TABLE ZonasEnvio (
    ZenId INT IDENTITY(1,1) PRIMARY KEY,
    ZenNombre NVARCHAR(100) NOT NULL,
    ZenDescripcion NVARCHAR(255),
    ZenActivo BIT DEFAULT 1
);

-- Tabla de Estados por Zona de Envío
CREATE TABLE ZonasEnvioEstados (
    ZeeZonaId INT NOT NULL,
    ZeeEstadoId INT NOT NULL,
    PRIMARY KEY (ZeeZonaId, ZeeEstadoId),
    FOREIGN KEY (ZeeZonaId) REFERENCES ZonasEnvio(ZenId),
    FOREIGN KEY (ZeeEstadoId) REFERENCES Estados(EstId)
);

-- Tabla de Métodos de Envío por Zona
CREATE TABLE MetodosEnvioZonas (
    MezMetodoId INT NOT NULL,
    MezZonaId INT NOT NULL,
    MezCosto DECIMAL(18,2) DEFAULT 0,
    MezActivo BIT DEFAULT 1,
    PRIMARY KEY (MezMetodoId, MezZonaId),
    FOREIGN KEY (MezMetodoId) REFERENCES MetodosEnvio(MenId),
    FOREIGN KEY (MezZonaId) REFERENCES ZonasEnvio(ZenId)
);

-- Tabla de Cupones de Descuento
CREATE TABLE Cupones (
    CupId INT IDENTITY(1,1) PRIMARY KEY,
    CupCodigo NVARCHAR(50) NOT NULL UNIQUE,
    CupNombre NVARCHAR(100) NOT NULL,
    CupDescripcion NVARCHAR(255),
    CupTipo NVARCHAR(20) NOT NULL, -- porcentaje, fijo, envio_gratis
    CupValor DECIMAL(18,2) NOT NULL,
    CupValorMinimo DECIMAL(18,2), -- Compra mínima
    CupUsosMaximos INT, -- NULL = ilimitado
    CupUsosActuales INT DEFAULT 0,
    CupUsosPorUsuario INT DEFAULT 1,
    CupFechaInicio DATETIME2 NOT NULL,
    CupFechaFin DATETIME2,
    CupActivo BIT DEFAULT 1,
    CupFechaCreacion DATETIME2 DEFAULT GETDATE()
);

-- Tabla de Pedidos
CREATE TABLE Pedidos (
    PedId INT IDENTITY(1,1) PRIMARY KEY,
    PedNumero NVARCHAR(20) NOT NULL UNIQUE, -- PED-2024-000001
    PedUsuarioId INT NULL, -- NULL para invitados
    PedEmail NVARCHAR(255) NOT NULL, -- Para invitados
    PedNombre NVARCHAR(100) NOT NULL,
    PedApellido NVARCHAR(100) NOT NULL,
    PedTelefono NVARCHAR(20),
    
    -- Dirección de Envío
    PedDireccionEnvio NVARCHAR(500) NOT NULL,
    PedCiudadEnvio NVARCHAR(100) NOT NULL,
    PedEstadoEnvio NVARCHAR(100) NOT NULL,
    PedCodigoPostalEnvio NVARCHAR(20),
    
    -- Dirección de Facturación
    PedDireccionFacturacion NVARCHAR(500),
    PedCiudadFacturacion NVARCHAR(100),
    PedEstadoFacturacion NVARCHAR(100),
    PedCodigoPostalFacturacion NVARCHAR(20),
    
    -- Totales
    PedSubtotal DECIMAL(18,2) NOT NULL,
    PedDescuento DECIMAL(18,2) DEFAULT 0,
    PedCostoEnvio DECIMAL(18,2) DEFAULT 0,
    PedImpuestos DECIMAL(18,2) DEFAULT 0,
    PedTotal DECIMAL(18,2) NOT NULL,
    
    -- Referencias
    PedEstado NVARCHAR(20) NOT NULL DEFAULT 'pendiente',
    PedMetodoPagoId INT NOT NULL,
    PedMetodoEnvioId INT NOT NULL,
    PedCuponId INT NULL,
    PedCodigoDescuento NVARCHAR(50),
    
    -- Fechas y seguimiento
    PedFechaCreacion DATETIME2 DEFAULT GETDATE(),
    PedFechaActualizacion DATETIME2 DEFAULT GETDATE(),
    PedFechaEnviado DATETIME2,
    PedFechaEntregado DATETIME2,
    PedCodigoSeguimiento NVARCHAR(100),
    PedNotas NVARCHAR(500),
    
    FOREIGN KEY (PedUsuarioId) REFERENCES Usuarios(UsuId),
    FOREIGN KEY (PedEstado) REFERENCES EstadosPedidos(EpeCodigo),
    FOREIGN KEY (PedMetodoPagoId) REFERENCES MetodosPago(MpaId),
    FOREIGN KEY (PedMetodoEnvioId) REFERENCES MetodosEnvio(MenId),
    FOREIGN KEY (PedCuponId) REFERENCES Cupones(CupId)
);

-- Tabla de Items de Pedidos
CREATE TABLE PedidosItems (
    PitId INT IDENTITY(1,1) PRIMARY KEY,
    PitPedidoId INT NOT NULL,
    PitProductoId INT NOT NULL,
    PitVarianteId INT NULL,
    PitSku NVARCHAR(50) NOT NULL,
    PitNombre NVARCHAR(200) NOT NULL,
    PitPrecio DECIMAL(18,2) NOT NULL,
    PitCantidad INT NOT NULL,
    PitTotal DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (PitPedidoId) REFERENCES Pedidos(PedId),
    FOREIGN KEY (PitProductoId) REFERENCES Productos(PrdId),
    FOREIGN KEY (PitVarianteId) REFERENCES ProductosVariantes(PvaId)
);

-- Tabla de Histórico de Estados de Pedidos
CREATE TABLE PedidosHistorial (
    PhiId INT IDENTITY(1,1) PRIMARY KEY,
    PhiPedidoId INT NOT NULL,
    PhiEstadoAnterior NVARCHAR(20),
    PhiEstadoNuevo NVARCHAR(20) NOT NULL,
    PhiComentario NVARCHAR(500),
    PhiUsuarioId INT,
    PhiFecha DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (PhiPedidoId) REFERENCES Pedidos(PedId),
    FOREIGN KEY (PhiEstadoAnterior) REFERENCES EstadosPedidos(EpeCodigo),
    FOREIGN KEY (PhiEstadoNuevo) REFERENCES EstadosPedidos(EpeCodigo),
    FOREIGN KEY (PhiUsuarioId) REFERENCES Usuarios(UsuId)
);

-- =====================================================
-- GESTIÓN DE PAGOS
-- =====================================================

-- Tabla de Transacciones de Pago
CREATE TABLE Transacciones (
    TraId INT IDENTITY(1,1) PRIMARY KEY,
    TraPedidoId INT NOT NULL,
    TraMetodoPagoId INT NOT NULL,
    TraNumeroTransaccion NVARCHAR(100), -- ID del gateway
    TraEstado NVARCHAR(20) NOT NULL, -- pendiente, completado, fallido, cancelado
    TraMonto DECIMAL(18,2) NOT NULL,
    TraGateway NVARCHAR(50), -- stripe, paypal, etc.
    TraReferencia NVARCHAR(255), -- Referencia externa
    TraFecha DATETIME2 DEFAULT GETDATE(),
    TraRespuestaGateway NTEXT, -- JSON response
    FOREIGN KEY (TraPedidoId) REFERENCES Pedidos(PedId),
    FOREIGN KEY (TraMetodoPagoId) REFERENCES MetodosPago(MpaId)
);

-- =====================================================
-- GESTIÓN DE CONTENIDO Y MARKETING
-- =====================================================

-- Tabla de Banners
CREATE TABLE Banners (
    BanId INT IDENTITY(1,1) PRIMARY KEY,
    BanTitulo NVARCHAR(200),
    BanSubtitulo NVARCHAR(300),
    BanImagen NVARCHAR(500) NOT NULL,
    BanImagenMovil NVARCHAR(500), -- Imagen específica para móvil
    BanEnlace NVARCHAR(500),
    BanTextoBoton NVARCHAR(50),
    BanPosicion NVARCHAR(20) DEFAULT 'principal', -- principal, secundario, lateral
    BanOrden INT DEFAULT 0,
    BanFechaInicio DATETIME2,
    BanFechaFin DATETIME2,
    BanActivo BIT DEFAULT 1,
    BanFechaCreacion DATETIME2 DEFAULT GETDATE()
);

-- Tabla de Páginas de Contenido
CREATE TABLE Paginas (
    PagId INT IDENTITY(1,1) PRIMARY KEY,
    PagTitulo NVARCHAR(200) NOT NULL,
    PagSlug NVARCHAR(200) NOT NULL UNIQUE,
    PagContenido NTEXT,
    PagMetaTitulo NVARCHAR(200),
    PagMetaDescripcion NVARCHAR(500),
    PagActivo BIT DEFAULT 1,
    PagFechaCreacion DATETIME2 DEFAULT GETDATE(),
    PagFechaModificacion DATETIME2 DEFAULT GETDATE()
);

-- Tabla de Reseñas de Productos
CREATE TABLE Reseñas (
    ResId INT IDENTITY(1,1) PRIMARY KEY,
    ResProductoId INT NOT NULL,
    ResUsuarioId INT NOT NULL,
    ResPedidoId INT NULL, -- Para verificar compra
    ResCalificacion INT NOT NULL CHECK (ResCalificacion BETWEEN 1 AND 5),
    ResTitulo NVARCHAR(200),
    ResComentario NTEXT,
    ResAprobado BIT DEFAULT 0,
    ResUtil INT DEFAULT 0, -- Contador de "útil"
    ResFechaCreacion DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (ResProductoId) REFERENCES Productos(PrdId),
    FOREIGN KEY (ResUsuarioId) REFERENCES Usuarios(UsuId),
    FOREIGN KEY (ResPedidoId) REFERENCES Pedidos(PedId),
    UNIQUE (ResProductoId, ResUsuarioId) -- Un usuario, una reseña por producto
);

-- Tabla de Newsletter
CREATE TABLE Newsletter (
    NewId INT IDENTITY(1,1) PRIMARY KEY,
    NewEmail NVARCHAR(255) NOT NULL UNIQUE,
    NewNombre NVARCHAR(100),
    NewActivo BIT DEFAULT 1,
    NewFechaCreacion DATETIME2 DEFAULT GETDATE(),
    NewFechaBaja DATETIME2 NULL
);

-- =====================================================
-- AUDITORÍA Y LOGS
-- =====================================================

-- Tabla de Logs del Sistema
CREATE TABLE Logs (
    LogId BIGINT IDENTITY(1,1) PRIMARY KEY,
    LogNivel NVARCHAR(20) NOT NULL, -- INFO, WARNING, ERROR, DEBUG
    LogMensaje NVARCHAR(MAX) NOT NULL,
    LogExcepcion NVARCHAR(MAX),
    LogUsuarioId INT,
    LogDireccionIP NVARCHAR(45),
    LogUserAgent NVARCHAR(500),
    LogUrl NVARCHAR(500),
    LogFecha DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (LogUsuarioId) REFERENCES Usuarios(UsuId)
);

-- Tabla de Auditoría de Cambios
CREATE TABLE Auditoria (
    AudId BIGINT IDENTITY(1,1) PRIMARY KEY,
    AudTabla NVARCHAR(100) NOT NULL,
    AudOperacion NVARCHAR(10) NOT NULL, -- INSERT, UPDATE, DELETE
    AudIdRegistro INT NOT NULL,
    AudValoresAnteriores NVARCHAR(MAX), -- JSON
    AudValoresNuevos NVARCHAR(MAX), -- JSON
    AudUsuarioId INT,
    AudFecha DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (AudUsuarioId) REFERENCES Usuarios(UsuId)
);

-- =====================================================
-- ANALYTICS Y REPORTES
-- =====================================================

-- Tabla de Vistas de Productos
CREATE TABLE ProductosVistas (
    PviId BIGINT IDENTITY(1,1) PRIMARY KEY,
    PviProductoId INT NOT NULL,
    PviUsuarioId INT NULL,
    PviSesionId NVARCHAR(100),
    PviDireccionIP NVARCHAR(45),
    PviUserAgent NVARCHAR(500),
    PviReferrer NVARCHAR(500),
    PviFecha DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (PviProductoId) REFERENCES Productos(PrdId),
    FOREIGN KEY (PviUsuarioId) REFERENCES Usuarios(UsuId)
);

-- Tabla de Búsquedas
CREATE TABLE Busquedas (
    BusId BIGINT IDENTITY(1,1) PRIMARY KEY,
    BusTermino NVARCHAR(255) NOT NULL,
    BusResultados INT DEFAULT 0,
    BusUsuarioId INT NULL,
    BusSesionId NVARCHAR(100),
    BusFecha DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (BusUsuarioId) REFERENCES Usuarios(UsuId)
);

-- =====================================================
-- CONFIGURACIONES INICIALES Y DATOS MAESTROS
-- =====================================================

-- Insertar configuraciones básicas
-- Insertar configuraciones básicas
INSERT INTO Configuraciones (ConClave, ConValor, ConDescripcion, ConTipo, ConActivo, ConFechaCreacion, ConFechaModificacion) VALUES
('sitio_nombre', 'TechGadgets Store', 'Nombre del sitio web', 'string', 1, GETDATE(), GETDATE()),
('sitio_descripcion', 'Tu tienda de gadgets tecnologicos', 'Descripcion del sitio', 'string', 1, GETDATE(), GETDATE()),
('moneda_principal', 'COP', 'Moneda principal del sitio', 'string', 1, GETDATE(), GETDATE()),
('simbolo_moneda', '$', 'Simbolo de la moneda', 'string', 1, GETDATE(), GETDATE()),
('iva_porcentaje', '19', 'Porcentaje de IVA', 'decimal', 1, GETDATE(), GETDATE()),
('productos_por_pagina', '12', 'Productos por pagina en catalogo', 'int', 1, GETDATE(), GETDATE()),
('envio_gratis_minimo', '150000', 'Monto minimo para envio gratis', 'decimal', 1, GETDATE(), GETDATE()),
('email_admin', 'admin@techgadgets.com', 'Email del administrador', 'string', 1, GETDATE(), GETDATE()),
('telefono_soporte', '+57 300 123 4567', 'Telefono de soporte', 'string', 1, GETDATE(), GETDATE()),
('horario_atencion', 'Lunes a Viernes 8:00 AM - 6:00 PM', 'Horario de atencion', 'string', 1, GETDATE(), GETDATE());

-- Insertar roles básicos
INSERT INTO Roles (RolNombre, RolDescripcion) VALUES
('SuperAdmin', 'Administrador con acceso completo al sistema'),
('Admin', 'Administrador con acceso a gestión de productos y pedidos'),
('Vendedor', 'Usuario con acceso a gestión de pedidos y productos'),
('Cliente', 'Cliente registrado de la tienda');

-- Insertar permisos básicos
INSERT INTO Permisos (PerCodigo, PerNombre, PerDescripcion, PerModulo) VALUES
-- Productos
('productos.ver', 'Ver Productos', 'Visualizar listado de productos', 'productos'),
('productos.crear', 'Crear Productos', 'Crear nuevos productos', 'productos'),
('productos.editar', 'Editar Productos', 'Modificar productos existentes', 'productos'),
('productos.eliminar', 'Eliminar Productos', 'Eliminar productos', 'productos'),
('productos.importar', 'Importar Productos', 'Importar productos masivamente', 'productos'),

-- Categorías
('categorias.ver', 'Ver Categorías', 'Visualizar categorías', 'categorias'),
('categorias.crear', 'Crear Categorías', 'Crear nuevas categorías', 'categorias'),
('categorias.editar', 'Editar Categorías', 'Modificar categorías', 'categorias'),
('categorias.eliminar', 'Eliminar Categorías', 'Eliminar categorías', 'categorias'),

-- Pedidos
('pedidos.ver', 'Ver Pedidos', 'Visualizar pedidos', 'pedidos'),
('pedidos.editar', 'Editar Pedidos', 'Modificar estado de pedidos', 'pedidos'),
('pedidos.cancelar', 'Cancelar Pedidos', 'Cancelar pedidos', 'pedidos'),
('pedidos.reembolsar', 'Reembolsar Pedidos', 'Procesar reembolsos', 'pedidos'),

-- Usuarios
('usuarios.ver', 'Ver Usuarios', 'Visualizar usuarios', 'usuarios'),
('usuarios.crear', 'Crear Usuarios', 'Crear nuevos usuarios', 'usuarios'),
('usuarios.editar', 'Editar Usuarios', 'Modificar usuarios', 'usuarios'),
('usuarios.eliminar', 'Eliminar Usuarios', 'Eliminar usuarios', 'usuarios'),

-- Inventario
('inventario.ver', 'Ver Inventario', 'Visualizar inventario', 'inventario'),
('inventario.ajustar', 'Ajustar Inventario', 'Realizar ajustes de inventario', 'inventario'),

-- Reportes
('reportes.ventas', 'Reportes de Ventas', 'Ver reportes de ventas', 'reportes'),
('reportes.productos', 'Reportes de Productos', 'Ver reportes de productos', 'reportes'),
('reportes.usuarios', 'Reportes de Usuarios', 'Ver reportes de usuarios', 'reportes'),

-- Configuración
('config.ver', 'Ver Configuración', 'Ver configuración del sistema', 'configuracion'),
('config.editar', 'Editar Configuración', 'Modificar configuración', 'configuracion');

-- Asignar permisos a roles
INSERT INTO RolesPermisos (RpeRolId, RpePermisoCodigo) 
SELECT r.RolId, p.PerCodigo 
FROM Roles r, Permisos p 
WHERE r.RolNombre = 'SuperAdmin';

INSERT INTO RolesPermisos (RpeRolId, RpePermisoCodigo) 
SELECT r.RolId, p.PerCodigo 
FROM Roles r, Permisos p 
WHERE r.RolNombre = 'Admin' 
AND p.PerCodigo NOT IN ('usuarios.eliminar', 'config.editar');

-- Insertar estados de pedidos
INSERT INTO EstadosPedidos (EpeCodigo, EpeNombre, EpeDescripcion, EpeColor, EpeOrden) VALUES
('pendiente', 'Pendiente', 'Pedido recibido, pendiente de procesamiento', '#FFA500', 1),
('confirmado', 'Confirmado', 'Pedido confirmado y en proceso', '#2196F3', 2),
('preparando', 'Preparando', 'Pedido siendo preparado para envío', '#FF9800', 3),
('enviado', 'Enviado', 'Pedido enviado al cliente', '#9C27B0', 4),
('entregado', 'Entregado', 'Pedido entregado exitosamente', '#4CAF50', 5),
('cancelado', 'Cancelado', 'Pedido cancelado', '#F44336', 6),
('reembolsado', 'Reembolsado', 'Pedido reembolsado', '#607D8B', 7);

-- Insertar métodos de pago
INSERT INTO MetodosPago (MpaNombre, MpaDescripcion, MpaTipo, MpaComision, MpaActivo, MpaOrden) VALUES
('Tarjeta de Crédito', 'Pago con tarjeta de crédito', 'tarjeta', 2.9, 1, 1),
('Tarjeta Débito', 'Pago con tarjeta débito', 'tarjeta', 1.5, 1, 2),
('PSE', 'Pago Seguro en Línea', 'transferencia', 1.2, 1, 3),
('Nequi', 'Pago con Nequi', 'digital', 0.5, 1, 4),
('Daviplata', 'Pago con Daviplata', 'digital', 0.5, 1, 5),
('Efectivo Contraentrega', 'Pago en efectivo al recibir', 'efectivo', 0, 1, 6);

-- Insertar métodos de envío
INSERT INTO MetodosEnvio (MenNombre, MenDescripcion, MenTipo, MenCosto, MenTiempoEntrega, MenActivo, MenOrden) VALUES
('Envío Estándar', 'Entrega en 3-5 días hábiles', 'fijo', 15000, '3-5 días hábiles', 1, 1),
('Envío Express', 'Entrega en 1-2 días hábiles', 'fijo', 25000, '1-2 días hábiles', 1, 2),
('Envío Gratis', 'Envío gratuito en compras mayores a $150,000', 'gratis', 0, '5-7 días hábiles', 1, 3),
('Recogida en Tienda', 'Recoger en nuestras instalaciones', 'fijo', 0, 'Inmediato', 1, 4);

-- Insertar país base (Colombia)
INSERT INTO Paises (PaiNombre, PaiCodigo, PaiCodigoTelefono, PaiMoneda, PaiActivo) VALUES
('Colombia', 'COL', '+57', 'COP', 1);

-- Insertar algunos estados principales de Colombia
DECLARE @PaisId INT = (SELECT PaiId FROM Paises WHERE PaiCodigo = 'COL');

INSERT INTO Estados (EstPaisId, EstNombre, EstCodigo, EstActivo) VALUES
(@PaisId, 'Antioquia', 'ANT', 1),
(@PaisId, 'Bogotá D.C.', 'BOG', 1),
(@PaisId, 'Valle del Cauca', 'VAL', 1),
(@PaisId, 'Atlántico', 'ATL', 1),
(@PaisId, 'Cundinamarca', 'CUN', 1),
(@PaisId, 'Santander', 'SAN', 1),
(@PaisId, 'Córdoba', 'COR', 1);

-- Insertar algunas ciudades principales
DECLARE @AntioquiaId INT = (SELECT EstId FROM Estados WHERE EstCodigo = 'ANT');
DECLARE @BogotaId INT = (SELECT EstId FROM Estados WHERE EstCodigo = 'BOG');
DECLARE @ValleId INT = (SELECT EstId FROM Estados WHERE EstCodigo = 'VAL');
DECLARE @CordobaId INT = (SELECT EstId FROM Estados WHERE EstCodigo = 'COR');

INSERT INTO Ciudades (CiuEstadoId, CiuNombre, CiuCodigoPostal, CiuActivo) VALUES
(@AntioquiaId, 'Medellín', '050001', 1),
(@BogotaId, 'Bogotá', '110111', 1),
(@ValleId, 'Cali', '760001', 1),
(@ValleId, 'Palmira', '763001', 1),
(@CordobaId, 'Montería', '230001', 1),
(@CordobaId, 'Cereté', '230055', 1);

-- Crear zona de envío nacional
INSERT INTO ZonasEnvio (ZenNombre, ZenDescripcion, ZenActivo) VALUES
('Nacional', 'Cobertura nacional en Colombia', 1);

DECLARE @ZonaNacionalId INT = SCOPE_IDENTITY();

-- Asignar todos los estados a la zona nacional
INSERT INTO ZonasEnvioEstados (ZeeZonaId, ZeeEstadoId)
SELECT @ZonaNacionalId, EstId FROM Estados;

-- Asignar métodos de envío a la zona nacional
INSERT INTO MetodosEnvioZonas (MezMetodoId, MezZonaId, MezCosto, MezActivo)
SELECT MenId, @ZonaNacionalId, MenCosto, 1 FROM MetodosEnvio;

-- Insertar marcas principales de tech
INSERT INTO Marcas (MarNombre, MarDescripcion, MarActivo) VALUES
('Apple', 'Productos Apple - iPhone, iPad, MacBook', 1),
('Samsung', 'Dispositivos Samsung - Galaxy, televisores', 1),
('Xiaomi', 'Smartphones y gadgets Xiaomi', 1),
('Sony', 'Productos electrónicos Sony', 1),
('LG', 'Electrodomésticos y tecnología LG', 1),
('Huawei', 'Dispositivos móviles y tecnología Huawei', 1),
('JBL', 'Audio y sonido profesional', 1),
('Anker', 'Accesorios y cargadores', 1),
('Logitech', 'Periféricos para computadora', 1),
('HP', 'Computadoras y impresoras', 1);

-- Insertar categorías principales
INSERT INTO Categorias (CatNombre, CatDescripcion, CatSlug, CatOrden, CatActivo) VALUES
('Smartphones', 'Teléfonos inteligentes y accesorios', 'smartphones', 1, 1),
('Computadoras', 'Laptops, PCs y componentes', 'computadoras', 2, 1),
('Audio y Video', 'Audífonos, parlantes y dispositivos multimedia', 'audio-video', 3, 1),
('Gaming', 'Consolas, juegos y accesorios gaming', 'gaming', 4, 1),
('Accesorios Tech', 'Cables, cargadores y accesorios diversos', 'accesorios-tech', 5, 1),
('Smart Home', 'Dispositivos para hogar inteligente', 'smart-home', 6, 1),
('Wearables', 'Smartwatches y dispositivos vestibles', 'wearables', 7, 1);

-- Insertar subcategorías
DECLARE @SmartphonesId INT = (SELECT CatId FROM Categorias WHERE CatSlug = 'smartphones');
DECLARE @ComputadorasId INT = (SELECT CatId FROM Categorias WHERE CatSlug = 'computadoras');
DECLARE @AudioVideoId INT = (SELECT CatId FROM Categorias WHERE CatSlug = 'audio-video');
DECLARE @AccesoriosId INT = (SELECT CatId FROM Categorias WHERE CatSlug = 'accesorios-tech');

INSERT INTO Categorias (CatNombre, CatDescripcion, CatCategoriaPadreId, CatSlug, CatOrden, CatActivo) VALUES
('iPhone', 'Teléfonos iPhone de Apple', @SmartphonesId, 'iphone', 1, 1),
('Samsung Galaxy', 'Línea Galaxy de Samsung', @SmartphonesId, 'samsung-galaxy', 2, 1),
('Xiaomi', 'Smartphones Xiaomi', @SmartphonesId, 'xiaomi-phones', 3, 1),
('Laptops', 'Computadoras portátiles', @ComputadorasId, 'laptops', 1, 1),
('PCs de Escritorio', 'Computadoras de escritorio', @ComputadorasId, 'pcs-escritorio', 2, 1),
('Audífonos', 'Audífonos y auriculares', @AudioVideoId, 'audifonos', 1, 1),
('Parlantes', 'Altavoces y sistemas de sonido', @AudioVideoId, 'parlantes', 2, 1),
('Cargadores', 'Cargadores y cables de carga', @AccesoriosId, 'cargadores', 1, 1),
('Fundas y Protectores', 'Protección para dispositivos', @AccesoriosId, 'fundas-protectores', 2, 1);

-- Insertar atributos principales
INSERT INTO Atributos (AtrNombre, AtrTipo, AtrRequerido, AtrFiltrable, AtrOrden, AtrActivo) VALUES
('Color', 'color', 0, 1, 1, 1),
('Capacidad de Almacenamiento', 'texto', 0, 1, 2, 1),
('Tamaño de Pantalla', 'texto', 0, 1, 3, 1),
('RAM', 'texto', 0, 1, 4, 1),
('Sistema Operativo', 'texto', 0, 1, 5, 1),
('Conectividad', 'texto', 0, 1, 6, 1),
('Garantía', 'texto', 0, 0, 7, 1);

-- Insertar valores de atributos comunes
DECLARE @ColorId INT = (SELECT AtrId FROM Atributos WHERE AtrNombre = 'Color');
DECLARE @AlmacenamientoId INT = (SELECT AtrId FROM Atributos WHERE AtrNombre = 'Capacidad de Almacenamiento');
DECLARE @PantallaId INT = (SELECT AtrId FROM Atributos WHERE AtrNombre = 'Tamaño de Pantalla');

INSERT INTO ValoresAtributos (ValAtributoId, ValValor, ValCodigoColor, ValOrden, ValActivo) VALUES
-- Colores
(@ColorId, 'Negro', '#000000', 1, 1),
(@ColorId, 'Blanco', '#FFFFFF', 2, 1),
(@ColorId, 'Azul', '#0066CC', 3, 1),
(@ColorId, 'Rojo', '#CC0000', 4, 1),
(@ColorId, 'Rosa', '#FF69B4', 5, 1),
(@ColorId, 'Verde', '#00CC00', 6, 1),
(@ColorId, 'Dorado', '#FFD700', 7, 1),
(@ColorId, 'Plateado', '#C0C0C0', 8, 1),

-- Almacenamiento
(@AlmacenamientoId, '64GB', NULL, 1, 1),
(@AlmacenamientoId, '128GB', NULL, 2, 1),
(@AlmacenamientoId, '256GB', NULL, 3, 1),
(@AlmacenamientoId, '512GB', NULL, 4, 1),
(@AlmacenamientoId, '1TB', NULL, 5, 1),

-- Tamaños de pantalla
(@PantallaId, '6.1"', NULL, 1, 1),
(@PantallaId, '6.7"', NULL, 2, 1),
(@PantallaId, '13.3"', NULL, 3, 1),
(@PantallaId, '15.6"', NULL, 4, 1);

-- =====================================================
-- ÍNDICES PARA OPTIMIZACIÓN
-- =====================================================

-- Índices para tablas de usuarios
CREATE INDEX IX_Usuarios_Email ON Usuarios(UsuEmail);
CREATE INDEX IX_Usuarios_Activo ON Usuarios(UsuActivo);
CREATE INDEX IX_Usuarios_FechaCreacion ON Usuarios(UsuFechaCreacion);

-- Índices para productos
CREATE INDEX IX_Productos_SKU ON Productos(PrdSku);
CREATE INDEX IX_Productos_Slug ON Productos(PrdSlug);
CREATE INDEX IX_Productos_Categoria ON Productos(PrdCategoriaId);
CREATE INDEX IX_Productos_Marca ON Productos(PrdMarcaId);
CREATE INDEX IX_Productos_Estado ON Productos(PrdEstado);
CREATE INDEX IX_Productos_Activo ON Productos(PrdActivo);
CREATE INDEX IX_Productos_Precio ON Productos(PrdPrecio);

-- Índices para pedidos
CREATE INDEX IX_Pedidos_Numero ON Pedidos(PedNumero);
CREATE INDEX IX_Pedidos_Usuario ON Pedidos(PedUsuarioId);
CREATE INDEX IX_Pedidos_Estado ON Pedidos(PedEstado);
CREATE INDEX IX_Pedidos_Fecha ON Pedidos(PedFechaCreacion);
CREATE INDEX IX_Pedidos_Email ON Pedidos(PedEmail);

-- Índices para inventario
CREATE INDEX IX_Inventario_Producto ON Inventario(InvProductoId);
CREATE INDEX IX_Inventario_Variante ON Inventario(InvVarianteId);
CREATE INDEX IX_Inventario_Stock ON Inventario(InvStock);

-- Índices para carrito
CREATE INDEX IX_Carrito_Usuario ON Carrito(CarUsuarioId);
CREATE INDEX IX_Carrito_Sesion ON Carrito(CarSesionId);
CREATE INDEX IX_Carrito_Producto ON Carrito(CarProductoId);

-- Índices para analytics
CREATE INDEX IX_ProductosVistas_Producto ON ProductosVistas(PviProductoId);
CREATE INDEX IX_ProductosVistas_Fecha ON ProductosVistas(PviFecha);
CREATE INDEX IX_Busquedas_Termino ON Busquedas(BusTermino);
CREATE INDEX IX_Busquedas_Fecha ON Busquedas(BusFecha);

-- Índices para logs y auditoría
CREATE INDEX IX_Logs_Fecha ON Logs(LogFecha);
CREATE INDEX IX_Logs_Nivel ON Logs(LogNivel);
CREATE INDEX IX_Auditoria_Tabla ON Auditoria(AudTabla);
CREATE INDEX IX_Auditoria_Fecha ON Auditoria(AudFecha);

-- =====================================================
-- TRIGGERS PARA AUDITORÍA AUTOMÁTICA
-- =====================================================


GO

-- =====================================================
-- MENSAJE DE FINALIZACIÓN
-- =====================================================
PRINT 'Base de datos TechGadgetsDB creada exitosamente';
PRINT 'Tablas creadas: ' + CAST(@@ROWCOUNT AS VARCHAR(10));
PRINT 'Configuración inicial completada';
PRINT '==============================================';