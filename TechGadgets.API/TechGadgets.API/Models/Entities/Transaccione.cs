using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

public partial class Transaccione
{
    [Key]
    public int TraId { get; set; }

    public int TraPedidoId { get; set; }

    public int TraMetodoPagoId { get; set; }

    [StringLength(100)]
    public string? TraNumeroTransaccion { get; set; }

    [StringLength(20)]
    public string TraEstado { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TraMonto { get; set; }

    [StringLength(50)]
    public string? TraGateway { get; set; }

    [StringLength(255)]
    public string? TraReferencia { get; set; }

    public DateTime? TraFecha { get; set; }

    [Column(TypeName = "ntext")]
    public string? TraRespuestaGateway { get; set; }

    [ForeignKey("TraMetodoPagoId")]
    [InverseProperty("Transacciones")]
    public virtual MetodosPago TraMetodoPago { get; set; } = null!;

    [ForeignKey("TraPedidoId")]
    [InverseProperty("Transacciones")]
    public virtual Pedido TraPedido { get; set; } = null!;
}
