using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

[Table("MetodosEnvio")]
public partial class MetodosEnvio
{
    [Key]
    public int MenId { get; set; }

    [StringLength(100)]
    public string MenNombre { get; set; } = null!;

    [StringLength(255)]
    public string? MenDescripcion { get; set; }

    [StringLength(20)]
    public string? MenTipo { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? MenCosto { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? MenCostoAdicional { get; set; }

    [Column(TypeName = "decimal(8, 2)")]
    public decimal? MenPesoMaximo { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? MenValorMinimo { get; set; }

    [StringLength(50)]
    public string? MenTiempoEntrega { get; set; }

    public bool? MenActivo { get; set; }

    public int? MenOrden { get; set; }

    [InverseProperty("MezMetodo")]
    public virtual ICollection<MetodosEnvioZona> MetodosEnvioZonas { get; set; } = new List<MetodosEnvioZona>();

    [InverseProperty("PedMetodoEnvio")]
    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
