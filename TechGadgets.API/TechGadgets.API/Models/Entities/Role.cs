using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

[Index("RolNombre", Name = "UQ__Roles__65F09DC1733AA566", IsUnique = true)]
public partial class Role
{
    [Key]
    public int RolId { get; set; }

    [StringLength(50)]
    public string RolNombre { get; set; } = null!;

    [StringLength(255)]
    public string? RolDescripcion { get; set; }

    public bool? RolActivo { get; set; }

    public DateTime? RolFechaCreacion { get; set; }

    [InverseProperty("RpeRol")]
    public virtual ICollection<RolesPermiso> RolesPermisos { get; set; } = new List<RolesPermiso>();

    [InverseProperty("UsrRol")]
    public virtual ICollection<UsuariosRole> UsuariosRoles { get; set; } = new List<UsuariosRole>();
}
