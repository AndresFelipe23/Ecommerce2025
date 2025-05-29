using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

[Table("MovimientosInventario")]
public partial class MovimientosInventario
{
    [Key]
    public int MovId { get; set; }

    public int MovInventarioId { get; set; }

    [StringLength(20)]
    public string MovTipo { get; set; } = null!;

    public int MovCantidad { get; set; }

    public int MovCantidadAnterior { get; set; }

    [StringLength(100)]
    public string? MovReferencia { get; set; }

    [StringLength(255)]
    public string? MovMotivo { get; set; }

    public int? MovUsuarioId { get; set; }

    public DateTime? MovFecha { get; set; }

    [ForeignKey("MovInventarioId")]
    [InverseProperty("MovimientosInventarios")]
    public virtual Inventario MovInventario { get; set; } = null!;

    [ForeignKey("MovUsuarioId")]
    [InverseProperty("MovimientosInventarios")]
    public virtual Usuario? MovUsuario { get; set; }
}
