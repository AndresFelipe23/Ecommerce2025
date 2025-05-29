using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

[Table("Inventario")]
[Index("InvProductoId", Name = "IX_Inventario_Producto")]
[Index("InvStock", Name = "IX_Inventario_Stock")]
[Index("InvVarianteId", Name = "IX_Inventario_Variante")]
public partial class Inventario
{
    [Key]
    public int InvId { get; set; }

    public int InvProductoId { get; set; }

    public int? InvVarianteId { get; set; }

    public int InvStock { get; set; }

    public int? InvStockMinimo { get; set; }

    public int? InvStockMaximo { get; set; }

    public int? InvStockReservado { get; set; }

    [StringLength(50)]
    public string? InvUbicacion { get; set; }

    public DateTime? InvFechaUltimaActualizacion { get; set; }

    [ForeignKey("InvProductoId")]
    [InverseProperty("Inventarios")]
    public virtual Producto InvProducto { get; set; } = null!;

    [ForeignKey("InvVarianteId")]
    [InverseProperty("Inventarios")]
    public virtual ProductosVariante? InvVariante { get; set; }

    [InverseProperty("MovInventario")]
    public virtual ICollection<MovimientosInventario> MovimientosInventarios { get; set; } = new List<MovimientosInventario>();
}
