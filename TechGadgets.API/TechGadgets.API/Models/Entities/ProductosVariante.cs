using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

[Index("PvaSku", Name = "UQ__Producto__3251A14B55420976", IsUnique = true)]
public partial class ProductosVariante
{
    [Key]
    public int PvaId { get; set; }

    public int PvaProductoId { get; set; }

    [StringLength(50)]
    public string PvaSku { get; set; } = null!;

    [StringLength(200)]
    public string? PvaNombre { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? PvaPrecio { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? PvaCosto { get; set; }

    [Column(TypeName = "decimal(8, 2)")]
    public decimal? PvaPeso { get; set; }

    public int? PvaOrden { get; set; }

    public bool? PvaActivo { get; set; }

    [InverseProperty("CarVariante")]
    public virtual ICollection<Carrito> Carritos { get; set; } = new List<Carrito>();

    [InverseProperty("InvVariante")]
    public virtual ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();

    [InverseProperty("LisVariante")]
    public virtual ICollection<ListaDeseo> ListaDeseos { get; set; } = new List<ListaDeseo>();

    [InverseProperty("PitVariante")]
    public virtual ICollection<PedidosItem> PedidosItems { get; set; } = new List<PedidosItem>();

    [InverseProperty("PimVariante")]
    public virtual ICollection<ProductosImagene> ProductosImagenes { get; set; } = new List<ProductosImagene>();

    [ForeignKey("PvaProductoId")]
    [InverseProperty("ProductosVariantes")]
    public virtual Producto PvaProducto { get; set; } = null!;

    [ForeignKey("VatVarianteId")]
    [InverseProperty("VatVariantes")]
    public virtual ICollection<ValoresAtributo> VatValorAtributos { get; set; } = new List<ValoresAtributo>();
}
