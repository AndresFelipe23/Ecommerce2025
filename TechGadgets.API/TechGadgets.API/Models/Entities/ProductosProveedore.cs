using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

[PrimaryKey("PprProductoId", "PprProveedorId")]
public partial class ProductosProveedore
{
    [Key]
    public int PprProductoId { get; set; }

    [Key]
    public int PprProveedorId { get; set; }

    [StringLength(50)]
    public string? PprSkuProveedor { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal PprCosto { get; set; }

    public int? PprTiempoEntrega { get; set; }

    public int? PprStockMinimo { get; set; }

    public bool? PprEsPrincipal { get; set; }

    public bool? PprActivo { get; set; }

    public DateTime? PprFechaCreacion { get; set; }

    [ForeignKey("PprProductoId")]
    [InverseProperty("ProductosProveedores")]
    public virtual Producto PprProducto { get; set; } = null!;

    [ForeignKey("PprProveedorId")]
    [InverseProperty("ProductosProveedores")]
    public virtual Proveedore PprProveedor { get; set; } = null!;
}
