using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

[PrimaryKey("RpeRolId", "RpePermisoCodigo")]
public partial class RolesPermiso
{
    [Key]
    public int RpeRolId { get; set; }

    [Key]
    [StringLength(50)]
    public string RpePermisoCodigo { get; set; } = null!;

    public DateTime? RpeFechaAsignacion { get; set; }

    [ForeignKey("RpePermisoCodigo")]
    [InverseProperty("RolesPermisos")]
    public virtual Permiso RpePermisoCodigoNavigation { get; set; } = null!;

    [ForeignKey("RpeRolId")]
    [InverseProperty("RolesPermisos")]
    public virtual Role RpeRol { get; set; } = null!;
}
