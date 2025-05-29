using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

public partial class PedidosItem
{
    [Key]
    public int PitId { get; set; }

    public int PitPedidoId { get; set; }

    public int PitProductoId { get; set; }

    public int? PitVarianteId { get; set; }

    [StringLength(50)]
    public string PitSku { get; set; } = null!;

    [StringLength(200)]
    public string PitNombre { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal PitPrecio { get; set; }

    public int PitCantidad { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal PitTotal { get; set; }

    [ForeignKey("PitPedidoId")]
    [InverseProperty("PedidosItems")]
    public virtual Pedido PitPedido { get; set; } = null!;

    [ForeignKey("PitProductoId")]
    [InverseProperty("PedidosItems")]
    public virtual Producto PitProducto { get; set; } = null!;

    [ForeignKey("PitVarianteId")]
    [InverseProperty("PedidosItems")]
    public virtual ProductosVariante? PitVariante { get; set; }
}
