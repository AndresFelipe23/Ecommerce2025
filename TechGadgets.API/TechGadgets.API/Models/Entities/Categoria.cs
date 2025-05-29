using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

[Index("CatSlug", Name = "UQ__Categori__ED805CFEEA8E9DD6", IsUnique = true)]
public partial class Categoria
{
    [Key]
    public int CatId { get; set; }

    [StringLength(100)]
    public string CatNombre { get; set; } = null!;

    [StringLength(500)]
    public string? CatDescripcion { get; set; }

    public int? CatCategoriaPadreId { get; set; }

    [StringLength(255)]
    public string? CatImagen { get; set; }

    [StringLength(50)]
    public string? CatIcono { get; set; }

    [StringLength(100)]
    public string CatSlug { get; set; } = null!;

    public int? CatOrden { get; set; }

    public bool? CatActivo { get; set; }

    public DateTime? CatFechaCreacion { get; set; }

    [ForeignKey("CatCategoriaPadreId")]
    [InverseProperty("InverseCatCategoriaPadre")]
    public virtual Categoria? CatCategoriaPadre { get; set; }

    [InverseProperty("CatCategoriaPadre")]
    public virtual ICollection<Categoria> InverseCatCategoriaPadre { get; set; } = new List<Categoria>();

    [InverseProperty("PrdCategoria")]
    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
