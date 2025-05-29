using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

[Table("PedidosHistorial")]
public partial class PedidosHistorial
{
    [Key]
    public int PhiId { get; set; }

    public int PhiPedidoId { get; set; }

    [StringLength(20)]
    public string? PhiEstadoAnterior { get; set; }

    [StringLength(20)]
    public string PhiEstadoNuevo { get; set; } = null!;

    [StringLength(500)]
    public string? PhiComentario { get; set; }

    public int? PhiUsuarioId { get; set; }

    public DateTime? PhiFecha { get; set; }

    [ForeignKey("PhiEstadoAnterior")]
    [InverseProperty("PedidosHistorialPhiEstadoAnteriorNavigations")]
    public virtual EstadosPedido? PhiEstadoAnteriorNavigation { get; set; }

    [ForeignKey("PhiEstadoNuevo")]
    [InverseProperty("PedidosHistorialPhiEstadoNuevoNavigations")]
    public virtual EstadosPedido PhiEstadoNuevoNavigation { get; set; } = null!;

    [ForeignKey("PhiPedidoId")]
    [InverseProperty("PedidosHistorials")]
    public virtual Pedido PhiPedido { get; set; } = null!;

    [ForeignKey("PhiUsuarioId")]
    [InverseProperty("PedidosHistorials")]
    public virtual Usuario? PhiUsuario { get; set; }
}
