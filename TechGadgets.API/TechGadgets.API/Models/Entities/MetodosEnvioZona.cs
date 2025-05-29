using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

[PrimaryKey("MezMetodoId", "MezZonaId")]
public partial class MetodosEnvioZona
{
    [Key]
    public int MezMetodoId { get; set; }

    [Key]
    public int MezZonaId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? MezCosto { get; set; }

    public bool? MezActivo { get; set; }

    [ForeignKey("MezMetodoId")]
    [InverseProperty("MetodosEnvioZonas")]
    public virtual MetodosEnvio MezMetodo { get; set; } = null!;

    [ForeignKey("MezZonaId")]
    [InverseProperty("MetodosEnvioZonas")]
    public virtual ZonasEnvio MezZona { get; set; } = null!;
}
