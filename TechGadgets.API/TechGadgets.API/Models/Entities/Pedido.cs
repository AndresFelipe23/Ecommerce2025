using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

[Index("PedEmail", Name = "IX_Pedidos_Email")]
[Index("PedEstado", Name = "IX_Pedidos_Estado")]
[Index("PedFechaCreacion", Name = "IX_Pedidos_Fecha")]
[Index("PedNumero", Name = "IX_Pedidos_Numero")]
[Index("PedUsuarioId", Name = "IX_Pedidos_Usuario")]
[Index("PedNumero", Name = "UQ__Pedidos__FADE10F57923ACAB", IsUnique = true)]
public partial class Pedido
{
    [Key]
    public int PedId { get; set; }

    [StringLength(20)]
    public string PedNumero { get; set; } = null!;

    public int? PedUsuarioId { get; set; }

    [StringLength(255)]
    public string PedEmail { get; set; } = null!;

    [StringLength(100)]
    public string PedNombre { get; set; } = null!;

    [StringLength(100)]
    public string PedApellido { get; set; } = null!;

    [StringLength(20)]
    public string? PedTelefono { get; set; }

    [StringLength(500)]
    public string PedDireccionEnvio { get; set; } = null!;

    [StringLength(100)]
    public string PedCiudadEnvio { get; set; } = null!;

    [StringLength(100)]
    public string PedEstadoEnvio { get; set; } = null!;

    [StringLength(20)]
    public string? PedCodigoPostalEnvio { get; set; }

    [StringLength(500)]
    public string? PedDireccionFacturacion { get; set; }

    [StringLength(100)]
    public string? PedCiudadFacturacion { get; set; }

    [StringLength(100)]
    public string? PedEstadoFacturacion { get; set; }

    [StringLength(20)]
    public string? PedCodigoPostalFacturacion { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal PedSubtotal { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? PedDescuento { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? PedCostoEnvio { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? PedImpuestos { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal PedTotal { get; set; }

    [StringLength(20)]
    public string PedEstado { get; set; } = null!;

    public int PedMetodoPagoId { get; set; }

    public int PedMetodoEnvioId { get; set; }

    public int? PedCuponId { get; set; }

    [StringLength(50)]
    public string? PedCodigoDescuento { get; set; }

    public DateTime? PedFechaCreacion { get; set; }

    public DateTime? PedFechaActualizacion { get; set; }

    public DateTime? PedFechaEnviado { get; set; }

    public DateTime? PedFechaEntregado { get; set; }

    [StringLength(100)]
    public string? PedCodigoSeguimiento { get; set; }

    [StringLength(500)]
    public string? PedNotas { get; set; }

    [ForeignKey("PedCuponId")]
    [InverseProperty("Pedidos")]
    public virtual Cupone? PedCupon { get; set; }

    [ForeignKey("PedEstado")]
    [InverseProperty("Pedidos")]
    public virtual EstadosPedido PedEstadoNavigation { get; set; } = null!;

    [ForeignKey("PedMetodoEnvioId")]
    [InverseProperty("Pedidos")]
    public virtual MetodosEnvio PedMetodoEnvio { get; set; } = null!;

    [ForeignKey("PedMetodoPagoId")]
    [InverseProperty("Pedidos")]
    public virtual MetodosPago PedMetodoPago { get; set; } = null!;

    [ForeignKey("PedUsuarioId")]
    [InverseProperty("Pedidos")]
    public virtual Usuario? PedUsuario { get; set; }

    [InverseProperty("PhiPedido")]
    public virtual ICollection<PedidosHistorial> PedidosHistorials { get; set; } = new List<PedidosHistorial>();

    [InverseProperty("PitPedido")]
    public virtual ICollection<PedidosItem> PedidosItems { get; set; } = new List<PedidosItem>();

    [InverseProperty("ResPedido")]
    public virtual ICollection<Reseña> Reseñas { get; set; } = new List<Reseña>();

    [InverseProperty("TraPedido")]
    public virtual ICollection<Transaccione> Transacciones { get; set; } = new List<Transaccione>();
}
