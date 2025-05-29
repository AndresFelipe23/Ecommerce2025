using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

[Index("LisUsuarioId", "LisProductoId", "LisVarianteId", Name = "UQ__ListaDes__BE82559C4EB7D4C2", IsUnique = true)]
public partial class ListaDeseo
{
    [Key]
    public int LisId { get; set; }

    public int LisUsuarioId { get; set; }

    public int LisProductoId { get; set; }

    public int? LisVarianteId { get; set; }

    public DateTime? LisFechaCreacion { get; set; }

    [ForeignKey("LisProductoId")]
    [InverseProperty("ListaDeseos")]
    public virtual Producto LisProducto { get; set; } = null!;

    [ForeignKey("LisUsuarioId")]
    [InverseProperty("ListaDeseos")]
    public virtual Usuario LisUsuario { get; set; } = null!;

    [ForeignKey("LisVarianteId")]
    [InverseProperty("ListaDeseos")]
    public virtual ProductosVariante? LisVariante { get; set; }
}
