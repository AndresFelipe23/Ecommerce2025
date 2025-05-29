using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

[Index("AtrNombre", Name = "UQ__Atributo__FD890C82350140E2", IsUnique = true)]
public partial class Atributo
{
    [Key]
    public int AtrId { get; set; }

    [StringLength(100)]
    public string AtrNombre { get; set; } = null!;

    [StringLength(20)]
    public string? AtrTipo { get; set; }

    public bool? AtrRequerido { get; set; }

    public bool? AtrFiltrable { get; set; }

    public int? AtrOrden { get; set; }

    public bool? AtrActivo { get; set; }

    [InverseProperty("ValAtributo")]
    public virtual ICollection<ValoresAtributo> ValoresAtributos { get; set; } = new List<ValoresAtributo>();
}
