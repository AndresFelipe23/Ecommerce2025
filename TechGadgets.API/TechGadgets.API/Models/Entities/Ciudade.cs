using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

public partial class Ciudade
{
    [Key]
    public int CiuId { get; set; }

    public int CiuEstadoId { get; set; }

    [StringLength(100)]
    public string CiuNombre { get; set; } = null!;

    [StringLength(20)]
    public string? CiuCodigoPostal { get; set; }

    public bool? CiuActivo { get; set; }

    [ForeignKey("CiuEstadoId")]
    [InverseProperty("Ciudades")]
    public virtual Estado CiuEstado { get; set; } = null!;

    [InverseProperty("DirCiudad")]
    public virtual ICollection<Direccione> Direcciones { get; set; } = new List<Direccione>();

    [InverseProperty("ProCiudad")]
    public virtual ICollection<Proveedore> Proveedores { get; set; } = new List<Proveedore>();
}
