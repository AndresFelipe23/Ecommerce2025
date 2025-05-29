using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

[Table("MetodosPago")]
public partial class MetodosPago
{
    [Key]
    public int MpaId { get; set; }

    [StringLength(50)]
    public string MpaNombre { get; set; } = null!;

    [StringLength(255)]
    public string? MpaDescripcion { get; set; }

    [StringLength(20)]
    public string? MpaTipo { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? MpaComision { get; set; }

    public bool? MpaActivo { get; set; }

    public int? MpaOrden { get; set; }

    [InverseProperty("PedMetodoPago")]
    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();

    [InverseProperty("TraMetodoPago")]
    public virtual ICollection<Transaccione> Transacciones { get; set; } = new List<Transaccione>();
}
