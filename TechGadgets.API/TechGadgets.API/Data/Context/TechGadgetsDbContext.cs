using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TechGadgets.API.Models.Entities;

namespace TechGadgets.API.Data.Context;

public partial class TechGadgetsDbContext : DbContext
{
    public TechGadgetsDbContext()
    {
    }

    public TechGadgetsDbContext(DbContextOptions<TechGadgetsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Atributo> Atributos { get; set; }

    public virtual DbSet<Auditorium> Auditoria { get; set; }

    public virtual DbSet<Banner> Banners { get; set; }

    public virtual DbSet<Busqueda> Busquedas { get; set; }

    public virtual DbSet<Carrito> Carritos { get; set; }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<Ciudade> Ciudades { get; set; }

    public virtual DbSet<Configuracione> Configuraciones { get; set; }

    public virtual DbSet<Cupone> Cupones { get; set; }

    public virtual DbSet<Direccione> Direcciones { get; set; }

    public virtual DbSet<Estado> Estados { get; set; }

    public virtual DbSet<EstadosPedido> EstadosPedidos { get; set; }

    public virtual DbSet<Inventario> Inventarios { get; set; }

    public virtual DbSet<ListaDeseo> ListaDeseos { get; set; }

    public virtual DbSet<Log> Logs { get; set; }

    public virtual DbSet<Marca> Marcas { get; set; }

    public virtual DbSet<MetodosEnvio> MetodosEnvios { get; set; }

    public virtual DbSet<MetodosEnvioZona> MetodosEnvioZonas { get; set; }

    public virtual DbSet<MetodosPago> MetodosPagos { get; set; }

    public virtual DbSet<MovimientosInventario> MovimientosInventarios { get; set; }

    public virtual DbSet<Newsletter> Newsletters { get; set; }

    public virtual DbSet<Pagina> Paginas { get; set; }

    public virtual DbSet<Paise> Paises { get; set; }

    public virtual DbSet<Pedido> Pedidos { get; set; }

    public virtual DbSet<PedidosHistorial> PedidosHistorials { get; set; }

    public virtual DbSet<PedidosItem> PedidosItems { get; set; }

    public virtual DbSet<Permiso> Permisos { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<ProductosImagene> ProductosImagenes { get; set; }

    public virtual DbSet<ProductosProveedore> ProductosProveedores { get; set; }

    public virtual DbSet<ProductosVariante> ProductosVariantes { get; set; }

    public virtual DbSet<ProductosVista> ProductosVistas { get; set; }

    public virtual DbSet<Proveedore> Proveedores { get; set; }

    public virtual DbSet<Reseña> Reseñas { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RolesPermiso> RolesPermisos { get; set; }

    public virtual DbSet<Transaccione> Transacciones { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<UsuariosRole> UsuariosRoles { get; set; }

    public virtual DbSet<ValoresAtributo> ValoresAtributos { get; set; }

    public virtual DbSet<ZonasEnvio> ZonasEnvios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=mssql-188335-0.cloudclusters.net,13026;Initial Catalog=TechGadgetsDB;Persist Security Info=False;User ID=andres;Password=Soypipe23@;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Atributo>(entity =>
        {
            entity.HasKey(e => e.AtrId).HasName("PK__Atributo__2B6457DC2304F5CB");

            entity.Property(e => e.AtrActivo).HasDefaultValue(true);
            entity.Property(e => e.AtrFiltrable).HasDefaultValue(true);
            entity.Property(e => e.AtrOrden).HasDefaultValue(0);
            entity.Property(e => e.AtrRequerido).HasDefaultValue(false);
            entity.Property(e => e.AtrTipo).HasDefaultValue("texto");
        });

        modelBuilder.Entity<Auditorium>(entity =>
        {
            entity.HasKey(e => e.AudId).HasName("PK__Auditori__D2F73E1510D848A6");

            entity.Property(e => e.AudFecha).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.AudUsuario).WithMany(p => p.Auditoria).HasConstraintName("FK__Auditoria__AudUs__3EDC53F0");
        });

        modelBuilder.Entity<Banner>(entity =>
        {
            entity.HasKey(e => e.BanId).HasName("PK__Banners__991CE745A4470BAE");

            entity.Property(e => e.BanActivo).HasDefaultValue(true);
            entity.Property(e => e.BanFechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.BanOrden).HasDefaultValue(0);
            entity.Property(e => e.BanPosicion).HasDefaultValue("principal");
        });

        modelBuilder.Entity<Busqueda>(entity =>
        {
            entity.HasKey(e => e.BusId).HasName("PK__Busqueda__6A0F60B53FB5D4B4");

            entity.Property(e => e.BusFecha).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.BusResultados).HasDefaultValue(0);

            entity.HasOne(d => d.BusUsuario).WithMany(p => p.Busqueda).HasConstraintName("FK__Busquedas__BusUs__4865BE2A");
        });

        modelBuilder.Entity<Carrito>(entity =>
        {
            entity.HasKey(e => e.CarId).HasName("PK__Carrito__68A0342EFA31889C");

            entity.Property(e => e.CarCantidad).HasDefaultValue(1);
            entity.Property(e => e.CarFechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CarFechaModificacion).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.CarProducto).WithMany(p => p.Carritos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Carrito__CarProd__55F4C372");

            entity.HasOne(d => d.CarUsuario).WithMany(p => p.Carritos).HasConstraintName("FK__Carrito__CarUsua__55009F39");

            entity.HasOne(d => d.CarVariante).WithMany(p => p.Carritos).HasConstraintName("FK__Carrito__CarVari__56E8E7AB");
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.CatId).HasName("PK__Categori__6A1C8AFA51A84697");

            entity.Property(e => e.CatActivo).HasDefaultValue(true);
            entity.Property(e => e.CatFechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CatOrden).HasDefaultValue(0);

            entity.HasOne(d => d.CatCategoriaPadre).WithMany(p => p.InverseCatCategoriaPadre).HasConstraintName("FK__Categoria__CatCa__06CD04F7");
        });

        modelBuilder.Entity<Ciudade>(entity =>
        {
            entity.HasKey(e => e.CiuId).HasName("PK__Ciudades__89C45E7E589B57CD");

            entity.Property(e => e.CiuActivo).HasDefaultValue(true);

            entity.HasOne(d => d.CiuEstado).WithMany(p => p.Ciudades)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ciudades__CiuEst__59063A47");
        });

        modelBuilder.Entity<Configuracione>(entity =>
        {
            entity.HasKey(e => e.ConId).HasName("PK__Configur__E19F47C9710ACBC1");

            entity.Property(e => e.ConActivo).HasDefaultValue(true);
            entity.Property(e => e.ConFechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.ConFechaModificacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.ConTipo).HasDefaultValue("string");
        });

        modelBuilder.Entity<Cupone>(entity =>
        {
            entity.HasKey(e => e.CupId).HasName("PK__Cupones__2C2806B438945013");

            entity.Property(e => e.CupActivo).HasDefaultValue(true);
            entity.Property(e => e.CupFechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CupUsosActuales).HasDefaultValue(0);
            entity.Property(e => e.CupUsosPorUsuario).HasDefaultValue(1);
        });

        modelBuilder.Entity<Direccione>(entity =>
        {
            entity.HasKey(e => e.DirId).HasName("PK__Direccio__E364B44D535C1760");

            entity.Property(e => e.DirActivo).HasDefaultValue(true);
            entity.Property(e => e.DirEsPrincipal).HasDefaultValue(false);
            entity.Property(e => e.DirFechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.DirTipo).HasDefaultValue("envio");

            entity.HasOne(d => d.DirCiudad).WithMany(p => p.Direcciones)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Direccion__DirCi__7B5B524B");

            entity.HasOne(d => d.DirUsuario).WithMany(p => p.Direcciones)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Direccion__DirUs__7A672E12");
        });

        modelBuilder.Entity<Estado>(entity =>
        {
            entity.HasKey(e => e.EstId).HasName("PK__Estados__665CAD5EAD0DAF99");

            entity.Property(e => e.EstActivo).HasDefaultValue(true);

            entity.HasOne(d => d.EstPais).WithMany(p => p.Estados)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Estados__EstPais__5535A963");
        });

        modelBuilder.Entity<EstadosPedido>(entity =>
        {
            entity.HasKey(e => e.EpeCodigo).HasName("PK__EstadosP__E00876024F1A3EF2");

            entity.Property(e => e.EpeActivo).HasDefaultValue(true);
            entity.Property(e => e.EpeColor).HasDefaultValue("#666666");
            entity.Property(e => e.EpeOrden).HasDefaultValue(0);
        });

        modelBuilder.Entity<Inventario>(entity =>
        {
            entity.HasKey(e => e.InvId).HasName("PK__Inventar__9DC82C6A1C851AEB");

            entity.Property(e => e.InvFechaUltimaActualizacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.InvStockMaximo).HasDefaultValue(100);
            entity.Property(e => e.InvStockMinimo).HasDefaultValue(5);
            entity.Property(e => e.InvStockReservado).HasDefaultValue(0);

            entity.HasOne(d => d.InvProducto).WithMany(p => p.Inventarios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventari__InvPr__498EEC8D");

            entity.HasOne(d => d.InvVariante).WithMany(p => p.Inventarios).HasConstraintName("FK__Inventari__InvVa__4A8310C6");
        });

        modelBuilder.Entity<ListaDeseo>(entity =>
        {
            entity.HasKey(e => e.LisId).HasName("PK__ListaDes__D7F1C04FC116C900");

            entity.Property(e => e.LisFechaCreacion).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.LisProducto).WithMany(p => p.ListaDeseos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ListaDese__LisPr__5CA1C101");

            entity.HasOne(d => d.LisUsuario).WithMany(p => p.ListaDeseos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ListaDese__LisUs__5BAD9CC8");

            entity.HasOne(d => d.LisVariante).WithMany(p => p.ListaDeseos).HasConstraintName("FK__ListaDese__LisVa__5D95E53A");
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__Logs__5E5486483AFBE88F");

            entity.Property(e => e.LogFecha).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.LogUsuario).WithMany(p => p.Logs).HasConstraintName("FK__Logs__LogUsuario__3B0BC30C");
        });

        modelBuilder.Entity<Marca>(entity =>
        {
            entity.HasKey(e => e.MarId).HasName("PK__Marcas__32E17527B22AA73F");

            entity.Property(e => e.MarActivo).HasDefaultValue(true);
            entity.Property(e => e.MarFechaCreacion).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<MetodosEnvio>(entity =>
        {
            entity.HasKey(e => e.MenId).HasName("PK__MetodosE__EB8800C945653E04");

            entity.Property(e => e.MenActivo).HasDefaultValue(true);
            entity.Property(e => e.MenCosto).HasDefaultValue(0m);
            entity.Property(e => e.MenCostoAdicional).HasDefaultValue(0m);
            entity.Property(e => e.MenOrden).HasDefaultValue(0);
        });

        modelBuilder.Entity<MetodosEnvioZona>(entity =>
        {
            entity.HasKey(e => new { e.MezMetodoId, e.MezZonaId }).HasName("PK__MetodosE__D4EF1CEA5A29137D");

            entity.Property(e => e.MezActivo).HasDefaultValue(true);
            entity.Property(e => e.MezCosto).HasDefaultValue(0m);

            entity.HasOne(d => d.MezMetodo).WithMany(p => p.MetodosEnvioZonas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MetodosEn__MezMe__7849DB76");

            entity.HasOne(d => d.MezZona).WithMany(p => p.MetodosEnvioZonas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MetodosEn__MezZo__793DFFAF");
        });

        modelBuilder.Entity<MetodosPago>(entity =>
        {
            entity.HasKey(e => e.MpaId).HasName("PK__MetodosP__8B47B19F0588B6D0");

            entity.Property(e => e.MpaActivo).HasDefaultValue(true);
            entity.Property(e => e.MpaComision).HasDefaultValue(0m);
            entity.Property(e => e.MpaOrden).HasDefaultValue(0);
        });

        modelBuilder.Entity<MovimientosInventario>(entity =>
        {
            entity.HasKey(e => e.MovId).HasName("PK__Movimien__C4941F47504FECF7");

            entity.Property(e => e.MovFecha).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.MovInventario).WithMany(p => p.MovimientosInventarios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Movimient__MovIn__4E53A1AA");

            entity.HasOne(d => d.MovUsuario).WithMany(p => p.MovimientosInventarios).HasConstraintName("FK__Movimient__MovUs__4F47C5E3");
        });

        modelBuilder.Entity<Newsletter>(entity =>
        {
            entity.HasKey(e => e.NewId).HasName("PK__Newslett__7CC3777E6991E500");

            entity.Property(e => e.NewActivo).HasDefaultValue(true);
            entity.Property(e => e.NewFechaCreacion).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Pagina>(entity =>
        {
            entity.HasKey(e => e.PagId).HasName("PK__Paginas__ED68D0A760E24A55");

            entity.Property(e => e.PagActivo).HasDefaultValue(true);
            entity.Property(e => e.PagFechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.PagFechaModificacion).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Paise>(entity =>
        {
            entity.HasKey(e => e.PaiId).HasName("PK__Paises__F50DFFD635FBE28E");

            entity.Property(e => e.PaiActivo).HasDefaultValue(true);
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.HasKey(e => e.PedId).HasName("PK__Pedidos__50CF4A45E381AEE4");

            entity.Property(e => e.PedCostoEnvio).HasDefaultValue(0m);
            entity.Property(e => e.PedDescuento).HasDefaultValue(0m);
            entity.Property(e => e.PedEstado).HasDefaultValue("pendiente");
            entity.Property(e => e.PedFechaActualizacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.PedFechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.PedImpuestos).HasDefaultValue(0m);

            entity.HasOne(d => d.PedCupon).WithMany(p => p.Pedidos).HasConstraintName("FK__Pedidos__PedCupo__0D44F85C");

            entity.HasOne(d => d.PedEstadoNavigation).WithMany(p => p.Pedidos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Pedidos__PedEsta__0A688BB1");

            entity.HasOne(d => d.PedMetodoEnvio).WithMany(p => p.Pedidos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Pedidos__PedMeto__0C50D423");

            entity.HasOne(d => d.PedMetodoPago).WithMany(p => p.Pedidos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Pedidos__PedMeto__0B5CAFEA");

            entity.HasOne(d => d.PedUsuario).WithMany(p => p.Pedidos).HasConstraintName("FK__Pedidos__PedUsua__09746778");
        });

        modelBuilder.Entity<PedidosHistorial>(entity =>
        {
            entity.HasKey(e => e.PhiId).HasName("PK__PedidosH__5BECFF58A60ACFAA");

            entity.Property(e => e.PhiFecha).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.PhiEstadoAnteriorNavigation).WithMany(p => p.PedidosHistorialPhiEstadoAnteriorNavigations).HasConstraintName("FK__PedidosHi__PhiEs__16CE6296");

            entity.HasOne(d => d.PhiEstadoNuevoNavigation).WithMany(p => p.PedidosHistorialPhiEstadoNuevoNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PedidosHi__PhiEs__17C286CF");

            entity.HasOne(d => d.PhiPedido).WithMany(p => p.PedidosHistorials)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PedidosHi__PhiPe__15DA3E5D");

            entity.HasOne(d => d.PhiUsuario).WithMany(p => p.PedidosHistorials).HasConstraintName("FK__PedidosHi__PhiUs__18B6AB08");
        });

        modelBuilder.Entity<PedidosItem>(entity =>
        {
            entity.HasKey(e => e.PitId).HasName("PK__PedidosI__A9AE88EF7297AB15");

            entity.HasOne(d => d.PitPedido).WithMany(p => p.PedidosItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PedidosIt__PitPe__10216507");

            entity.HasOne(d => d.PitProducto).WithMany(p => p.PedidosItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PedidosIt__PitPr__11158940");

            entity.HasOne(d => d.PitVariante).WithMany(p => p.PedidosItems).HasConstraintName("FK__PedidosIt__PitVa__1209AD79");
        });

        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.HasKey(e => e.PerCodigo).HasName("PK__Permisos__FC98D07E5D8090A3");

            entity.Property(e => e.PerActivo).HasDefaultValue(true);
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.PrdId).HasName("PK__Producto__7168B164696FFA61");

            entity.Property(e => e.PrdActivo).HasDefaultValue(true);
            entity.Property(e => e.PrdDestacado).HasDefaultValue(false);
            entity.Property(e => e.PrdEnOferta).HasDefaultValue(false);
            entity.Property(e => e.PrdEstado).HasDefaultValue("borrador");
            entity.Property(e => e.PrdFechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.PrdFechaModificacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.PrdNuevo).HasDefaultValue(false);
            entity.Property(e => e.PrdOrden).HasDefaultValue(0);
            entity.Property(e => e.PrdPermiteReseñas).HasDefaultValue(true);
            entity.Property(e => e.PrdRequiereEnvio).HasDefaultValue(true);
            entity.Property(e => e.PrdTipo).HasDefaultValue("simple");

            entity.HasOne(d => d.PrdCategoria).WithMany(p => p.Productos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Productos__PrdCa__1CBC4616");

            entity.HasOne(d => d.PrdMarca).WithMany(p => p.Productos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Productos__PrdMa__1BC821DD");
        });

        modelBuilder.Entity<ProductosImagene>(entity =>
        {
            entity.HasKey(e => e.PimId).HasName("PK__Producto__A9E5067C18B7A513");

            entity.Property(e => e.PimActivo).HasDefaultValue(true);
            entity.Property(e => e.PimEsPrincipal).HasDefaultValue(false);
            entity.Property(e => e.PimOrden).HasDefaultValue(0);

            entity.HasOne(d => d.PimProducto).WithMany(p => p.ProductosImagenes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Productos__PimPr__3864608B");

            entity.HasOne(d => d.PimVariante).WithMany(p => p.ProductosImagenes).HasConstraintName("FK__Productos__PimVa__395884C4");
        });

        modelBuilder.Entity<ProductosProveedore>(entity =>
        {
            entity.HasKey(e => new { e.PprProductoId, e.PprProveedorId }).HasName("PK__Producto__18F5699F0F6E56E6");

            entity.Property(e => e.PprActivo).HasDefaultValue(true);
            entity.Property(e => e.PprEsPrincipal).HasDefaultValue(false);
            entity.Property(e => e.PprFechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.PprStockMinimo).HasDefaultValue(0);
            entity.Property(e => e.PprTiempoEntrega).HasDefaultValue(7);

            entity.HasOne(d => d.PprProducto).WithMany(p => p.ProductosProveedores)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Productos__PprPr__40F9A68C");

            entity.HasOne(d => d.PprProveedor).WithMany(p => p.ProductosProveedores)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Productos__PprPr__41EDCAC5");
        });

        modelBuilder.Entity<ProductosVariante>(entity =>
        {
            entity.HasKey(e => e.PvaId).HasName("PK__Producto__EED800B13B228AF9");

            entity.Property(e => e.PvaActivo).HasDefaultValue(true);
            entity.Property(e => e.PvaOrden).HasDefaultValue(0);

            entity.HasOne(d => d.PvaProducto).WithMany(p => p.ProductosVariantes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Productos__PvaPr__2EDAF651");

            entity.HasMany(d => d.VatValorAtributos).WithMany(p => p.VatVariantes)
                .UsingEntity<Dictionary<string, object>>(
                    "VariantesAtributo",
                    r => r.HasOne<ValoresAtributo>().WithMany()
                        .HasForeignKey("VatValorAtributoId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Variantes__VatVa__32AB8735"),
                    l => l.HasOne<ProductosVariante>().WithMany()
                        .HasForeignKey("VatVarianteId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Variantes__VatVa__31B762FC"),
                    j =>
                    {
                        j.HasKey("VatVarianteId", "VatValorAtributoId").HasName("PK__Variante__A7EED96E8C9D9532");
                        j.ToTable("VariantesAtributos");
                    });
        });

        modelBuilder.Entity<ProductosVista>(entity =>
        {
            entity.HasKey(e => e.PviId).HasName("PK__Producto__E8E863BC883BCAC9");

            entity.Property(e => e.PviFecha).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.PviProducto).WithMany(p => p.ProductosVista)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Productos__PviPr__42ACE4D4");

            entity.HasOne(d => d.PviUsuario).WithMany(p => p.ProductosVista).HasConstraintName("FK__Productos__PviUs__43A1090D");
        });

        modelBuilder.Entity<Proveedore>(entity =>
        {
            entity.HasKey(e => e.ProId).HasName("PK__Proveedo__620295904EC18D7D");

            entity.Property(e => e.ProActivo).HasDefaultValue(true);
            entity.Property(e => e.ProFechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.ProTiempoEntrega).HasDefaultValue(7);

            entity.HasOne(d => d.ProCiudad).WithMany(p => p.Proveedores).HasConstraintName("FK__Proveedor__ProCi__0C85DE4D");
        });

        modelBuilder.Entity<Reseña>(entity =>
        {
            entity.HasKey(e => e.ResId).HasName("PK__Reseñas__297882F6B1843C1B");

            entity.Property(e => e.ResAprobado).HasDefaultValue(false);
            entity.Property(e => e.ResFechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.ResUtil).HasDefaultValue(0);

            entity.HasOne(d => d.ResPedido).WithMany(p => p.Reseñas).HasConstraintName("FK__Reseñas__ResPedi__32767D0B");

            entity.HasOne(d => d.ResProducto).WithMany(p => p.Reseñas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reseñas__ResProd__308E3499");

            entity.HasOne(d => d.ResUsuario).WithMany(p => p.Reseñas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reseñas__ResUsua__318258D2");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RolId).HasName("PK__Roles__F92302F1938B5A94");

            entity.Property(e => e.RolActivo).HasDefaultValue(true);
            entity.Property(e => e.RolFechaCreacion).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<RolesPermiso>(entity =>
        {
            entity.HasKey(e => new { e.RpeRolId, e.RpePermisoCodigo }).HasName("PK__RolesPer__BA6D2B625C09F960");

            entity.Property(e => e.RpeFechaAsignacion).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.RpePermisoCodigoNavigation).WithMany(p => p.RolesPermisos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RolesPerm__RpePe__656C112C");

            entity.HasOne(d => d.RpeRol).WithMany(p => p.RolesPermisos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RolesPerm__RpeRo__6477ECF3");
        });

        modelBuilder.Entity<Transaccione>(entity =>
        {
            entity.HasKey(e => e.TraId).HasName("PK__Transacc__E6FDEF50270067E9");

            entity.Property(e => e.TraFecha).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.TraMetodoPago).WithMany(p => p.Transacciones)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Transacci__TraMe__1D7B6025");

            entity.HasOne(d => d.TraPedido).WithMany(p => p.Transacciones)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Transacci__TraPe__1C873BEC");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.UsuId).HasName("PK__Usuarios__6852638391F623D4");

            entity.Property(e => e.UsuActivo).HasDefaultValue(true);
            entity.Property(e => e.UsuEmailVerificado).HasDefaultValue(false);
            entity.Property(e => e.UsuFechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.UsuFechaModificacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.UsuGenero).IsFixedLength();
            entity.Property(e => e.UsuIntentosFallidos).HasDefaultValue(0);
            entity.Property(e => e.UsuTelefonoVerificado).HasDefaultValue(false);
        });

        modelBuilder.Entity<UsuariosRole>(entity =>
        {
            entity.HasKey(e => new { e.UsrUsuarioId, e.UsrRolId }).HasName("PK__Usuarios__0196237D5D22E9FC");

            entity.Property(e => e.UsrActivo).HasDefaultValue(true);
            entity.Property(e => e.UsrFechaAsignacion).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.UsrRol).WithMany(p => p.UsuariosRoles)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UsuariosR__UsrRo__73BA3083");

            entity.HasOne(d => d.UsrUsuario).WithMany(p => p.UsuariosRoles)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UsuariosR__UsrUs__72C60C4A");
        });

        modelBuilder.Entity<ValoresAtributo>(entity =>
        {
            entity.HasKey(e => e.ValId).HasName("PK__ValoresA__07DB42097BFDF2AF");

            entity.Property(e => e.ValActivo).HasDefaultValue(true);
            entity.Property(e => e.ValOrden).HasDefaultValue(0);

            entity.HasOne(d => d.ValAtributo).WithMany(p => p.ValoresAtributos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ValoresAt__ValAt__29221CFB");
        });

        modelBuilder.Entity<ZonasEnvio>(entity =>
        {
            entity.HasKey(e => e.ZenId).HasName("PK__ZonasEnv__718D8815B9E5F40C");

            entity.Property(e => e.ZenActivo).HasDefaultValue(true);

            entity.HasMany(d => d.ZeeEstados).WithMany(p => p.ZeeZonas)
                .UsingEntity<Dictionary<string, object>>(
                    "ZonasEnvioEstado",
                    r => r.HasOne<Estado>().WithMany()
                        .HasForeignKey("ZeeEstadoId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__ZonasEnvi__ZeeEs__73852659"),
                    l => l.HasOne<ZonasEnvio>().WithMany()
                        .HasForeignKey("ZeeZonaId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__ZonasEnvi__ZeeZo__72910220"),
                    j =>
                    {
                        j.HasKey("ZeeZonaId", "ZeeEstadoId").HasName("PK__ZonasEnv__A646AB6636E699EF");
                        j.ToTable("ZonasEnvioEstados");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
