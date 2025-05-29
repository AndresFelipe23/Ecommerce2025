using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

[Index("ConClave", Name = "UQ__Configur__440CEC021A7073F0", IsUnique = true)]
public partial class Configuracione
{
    [Key]
    public int ConId { get; set; }

    [StringLength(100)]
    public string ConClave { get; set; } = null!;

    [StringLength(500)]
    public string ConValor { get; set; } = null!;

    [StringLength(255)]
    public string? ConDescripcion { get; set; }

    [StringLength(50)]
    public string? ConTipo { get; set; }

    public bool? ConActivo { get; set; }

    public DateTime? ConFechaCreacion { get; set; }

    public DateTime? ConFechaModificacion { get; set; }
}
