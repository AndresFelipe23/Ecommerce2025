using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

[Index("PaiCodigo", Name = "UQ__Paises__B1C9E78A2F411E1A", IsUnique = true)]
public partial class Paise
{
    [Key]
    public int PaiId { get; set; }

    [StringLength(100)]
    public string PaiNombre { get; set; } = null!;

    [StringLength(3)]
    public string PaiCodigo { get; set; } = null!;

    [StringLength(5)]
    public string? PaiCodigoTelefono { get; set; }

    [StringLength(3)]
    public string? PaiMoneda { get; set; }

    public bool? PaiActivo { get; set; }

    [InverseProperty("EstPais")]
    public virtual ICollection<Estado> Estados { get; set; } = new List<Estado>();
}
