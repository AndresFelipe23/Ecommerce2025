using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

public partial class Direccione
{
    [Key]
    public int DirId { get; set; }

    public int DirUsuarioId { get; set; }

    [StringLength(20)]
    public string? DirTipo { get; set; }

    [StringLength(100)]
    public string? DirNombre { get; set; }

    [StringLength(255)]
    public string DirDireccionLinea1 { get; set; } = null!;

    [StringLength(255)]
    public string? DirDireccionLinea2 { get; set; }

    public int DirCiudadId { get; set; }

    [StringLength(20)]
    public string? DirCodigoPostal { get; set; }

    [StringLength(255)]
    public string? DirReferencias { get; set; }

    public bool? DirEsPrincipal { get; set; }

    public bool? DirActivo { get; set; }

    public DateTime? DirFechaCreacion { get; set; }

    [ForeignKey("DirCiudadId")]
    [InverseProperty("Direcciones")]
    public virtual Ciudade DirCiudad { get; set; } = null!;

    [ForeignKey("DirUsuarioId")]
    [InverseProperty("Direcciones")]
    public virtual Usuario DirUsuario { get; set; } = null!;
}
