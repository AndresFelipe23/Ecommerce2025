using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

public partial class EstadosPedido
{
    [Key]
    [StringLength(20)]
    public string EpeCodigo { get; set; } = null!;

    [StringLength(50)]
    public string EpeNombre { get; set; } = null!;

    [StringLength(255)]
    public string? EpeDescripcion { get; set; }

    [StringLength(7)]
    public string? EpeColor { get; set; }

    public int? EpeOrden { get; set; }

    public bool? EpeActivo { get; set; }

    [InverseProperty("PedEstadoNavigation")]
    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();

    [InverseProperty("PhiEstadoAnteriorNavigation")]
    public virtual ICollection<PedidosHistorial> PedidosHistorialPhiEstadoAnteriorNavigations { get; set; } = new List<PedidosHistorial>();

    [InverseProperty("PhiEstadoNuevoNavigation")]
    public virtual ICollection<PedidosHistorial> PedidosHistorialPhiEstadoNuevoNavigations { get; set; } = new List<PedidosHistorial>();
}
