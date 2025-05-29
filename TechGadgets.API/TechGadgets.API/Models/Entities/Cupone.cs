using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

[Index("CupCodigo", Name = "UQ__Cupones__C60838A36DE84A73", IsUnique = true)]
public partial class Cupone
{
    [Key]
    public int CupId { get; set; }

    [StringLength(50)]
    public string CupCodigo { get; set; } = null!;

    [StringLength(100)]
    public string CupNombre { get; set; } = null!;

    [StringLength(255)]
    public string? CupDescripcion { get; set; }

    [StringLength(20)]
    public string CupTipo { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal CupValor { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? CupValorMinimo { get; set; }

    public int? CupUsosMaximos { get; set; }

    public int? CupUsosActuales { get; set; }

    public int? CupUsosPorUsuario { get; set; }

    public DateTime CupFechaInicio { get; set; }

    public DateTime? CupFechaFin { get; set; }

    public bool? CupActivo { get; set; }

    public DateTime? CupFechaCreacion { get; set; }

    [InverseProperty("PedCupon")]
    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
