using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

[Index("MarNombre", Name = "UQ__Marcas__3BA1D749CDE0C9CB", IsUnique = true)]
public partial class Marca
{
    [Key]
    public int MarId { get; set; }

    [StringLength(100)]
    public string MarNombre { get; set; } = null!;

    [StringLength(500)]
    public string? MarDescripcion { get; set; }

    [StringLength(255)]
    public string? MarLogo { get; set; }

    [StringLength(255)]
    public string? MarSitioWeb { get; set; }

    public bool? MarActivo { get; set; }

    public DateTime? MarFechaCreacion { get; set; }

    [InverseProperty("PrdMarca")]
    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
