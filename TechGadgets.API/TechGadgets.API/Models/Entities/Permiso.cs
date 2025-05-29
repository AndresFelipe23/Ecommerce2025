using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

public partial class Permiso
{
    [Key]
    [StringLength(50)]
    public string PerCodigo { get; set; } = null!;

    [StringLength(100)]
    public string PerNombre { get; set; } = null!;

    [StringLength(255)]
    public string? PerDescripcion { get; set; }

    [StringLength(50)]
    public string? PerModulo { get; set; }

    public bool? PerActivo { get; set; }

    [InverseProperty("RpePermisoCodigoNavigation")]
    public virtual ICollection<RolesPermiso> RolesPermisos { get; set; } = new List<RolesPermiso>();
}
