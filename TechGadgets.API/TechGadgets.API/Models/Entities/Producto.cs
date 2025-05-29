using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

[Index("PrdActivo", Name = "IX_Productos_Activo")]
[Index("PrdCategoriaId", Name = "IX_Productos_Categoria")]
[Index("PrdEstado", Name = "IX_Productos_Estado")]
[Index("PrdMarcaId", Name = "IX_Productos_Marca")]
[Index("PrdPrecio", Name = "IX_Productos_Precio")]
[Index("PrdSku", Name = "IX_Productos_SKU")]
[Index("PrdSlug", Name = "IX_Productos_Slug")]
[Index("PrdSlug", Name = "UQ__Producto__1CF70D438A8E3024", IsUnique = true)]
[Index("PrdSku", Name = "UQ__Producto__D2A8ACFE644BBFCA", IsUnique = true)]
public partial class Producto
{
    [Key]
    public int PrdId { get; set; }

    [StringLength(50)]
    public string PrdSku { get; set; } = null!;

    [StringLength(200)]
    public string PrdNombre { get; set; } = null!;

    [StringLength(500)]
    public string? PrdDescripcionCorta { get; set; }

    [Column(TypeName = "ntext")]
    public string? PrdDescripcionLarga { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal PrdPrecio { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? PrdPrecioComparacion { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? PrdCosto { get; set; }

    public int PrdMarcaId { get; set; }

    public int PrdCategoriaId { get; set; }

    [StringLength(20)]
    public string? PrdTipo { get; set; }

    [StringLength(20)]
    public string? PrdEstado { get; set; }

    public bool? PrdDestacado { get; set; }

    public bool? PrdNuevo { get; set; }

    public bool? PrdEnOferta { get; set; }

    [Column(TypeName = "decimal(8, 2)")]
    public decimal? PrdPeso { get; set; }

    [StringLength(50)]
    public string? PrdDimensiones { get; set; }

    [StringLength(200)]
    public string PrdSlug { get; set; } = null!;

    [StringLength(200)]
    public string? PrdMetaTitulo { get; set; }

    [StringLength(500)]
    public string? PrdMetaDescripcion { get; set; }

    [StringLength(500)]
    public string? PrdPalabrasClaves { get; set; }

    public bool? PrdRequiereEnvio { get; set; }

    public bool? PrdPermiteReseñas { get; set; }

    [StringLength(100)]
    public string? PrdGarantia { get; set; }

    public int? PrdOrden { get; set; }

    public bool? PrdActivo { get; set; }

    public DateTime? PrdFechaCreacion { get; set; }

    public DateTime? PrdFechaModificacion { get; set; }

    [InverseProperty("CarProducto")]
    public virtual ICollection<Carrito> Carritos { get; set; } = new List<Carrito>();

    [InverseProperty("InvProducto")]
    public virtual ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();

    [InverseProperty("LisProducto")]
    public virtual ICollection<ListaDeseo> ListaDeseos { get; set; } = new List<ListaDeseo>();

    [InverseProperty("PitProducto")]
    public virtual ICollection<PedidosItem> PedidosItems { get; set; } = new List<PedidosItem>();

    [ForeignKey("PrdCategoriaId")]
    [InverseProperty("Productos")]
    public virtual Categoria PrdCategoria { get; set; } = null!;

    [ForeignKey("PrdMarcaId")]
    [InverseProperty("Productos")]
    public virtual Marca PrdMarca { get; set; } = null!;

    [InverseProperty("PimProducto")]
    public virtual ICollection<ProductosImagene> ProductosImagenes { get; set; } = new List<ProductosImagene>();

    [InverseProperty("PprProducto")]
    public virtual ICollection<ProductosProveedore> ProductosProveedores { get; set; } = new List<ProductosProveedore>();

    [InverseProperty("PvaProducto")]
    public virtual ICollection<ProductosVariante> ProductosVariantes { get; set; } = new List<ProductosVariante>();

    [InverseProperty("PviProducto")]
    public virtual ICollection<ProductosVista> ProductosVista { get; set; } = new List<ProductosVista>();

    [InverseProperty("ResProducto")]
    public virtual ICollection<Reseña> Reseñas { get; set; } = new List<Reseña>();
}
