using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

[PrimaryKey("UsrUsuarioId", "UsrRolId")]
public partial class UsuariosRole
{
    [Key]
    public int UsrUsuarioId { get; set; }

    [Key]
    public int UsrRolId { get; set; }

    public DateTime? UsrFechaAsignacion { get; set; }

    public bool? UsrActivo { get; set; }

    [ForeignKey("UsrRolId")]
    [InverseProperty("UsuariosRoles")]
    public virtual Role UsrRol { get; set; } = null!;

    [ForeignKey("UsrUsuarioId")]
    [InverseProperty("UsuariosRoles")]
    public virtual Usuario UsrUsuario { get; set; } = null!;
}
