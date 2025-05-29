using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

public partial class Estado
{
    [Key]
    public int EstId { get; set; }

    public int EstPaisId { get; set; }

    [StringLength(100)]
    public string EstNombre { get; set; } = null!;

    [StringLength(10)]
    public string? EstCodigo { get; set; }

    public bool? EstActivo { get; set; }

    [InverseProperty("CiuEstado")]
    public virtual ICollection<Ciudade> Ciudades { get; set; } = new List<Ciudade>();

    [ForeignKey("EstPaisId")]
    [InverseProperty("Estados")]
    public virtual Paise EstPais { get; set; } = null!;

    [ForeignKey("ZeeEstadoId")]
    [InverseProperty("ZeeEstados")]
    public virtual ICollection<ZonasEnvio> ZeeZonas { get; set; } = new List<ZonasEnvio>();
}
