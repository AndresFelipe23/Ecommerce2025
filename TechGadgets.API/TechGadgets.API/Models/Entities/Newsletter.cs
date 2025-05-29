using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

[Table("Newsletter")]
[Index("NewEmail", Name = "UQ__Newslett__72B0B81F1E11B310", IsUnique = true)]
public partial class Newsletter
{
    [Key]
    public int NewId { get; set; }

    [StringLength(255)]
    public string NewEmail { get; set; } = null!;

    [StringLength(100)]
    public string? NewNombre { get; set; }

    public bool? NewActivo { get; set; }

    public DateTime? NewFechaCreacion { get; set; }

    public DateTime? NewFechaBaja { get; set; }
}
