using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

[Table("ZonasEnvio")]
public partial class ZonasEnvio
{
    [Key]
    public int ZenId { get; set; }

    [StringLength(100)]
    public string ZenNombre { get; set; } = null!;

    [StringLength(255)]
    public string? ZenDescripcion { get; set; }

    public bool? ZenActivo { get; set; }

    [InverseProperty("MezZona")]
    public virtual ICollection<MetodosEnvioZona> MetodosEnvioZonas { get; set; } = new List<MetodosEnvioZona>();

    [ForeignKey("ZeeZonaId")]
    [InverseProperty("ZeeZonas")]
    public virtual ICollection<Estado> ZeeEstados { get; set; } = new List<Estado>();
}
