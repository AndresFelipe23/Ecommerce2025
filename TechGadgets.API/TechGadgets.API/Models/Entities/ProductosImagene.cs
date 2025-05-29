using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

public partial class ProductosImagene
{
    [Key]
    public int PimId { get; set; }

    public int PimProductoId { get; set; }

    public int? PimVarianteId { get; set; }

    [StringLength(500)]
    public string PimUrl { get; set; } = null!;

    [StringLength(255)]
    public string? PimTextoAlternativo { get; set; }

    public bool? PimEsPrincipal { get; set; }

    public int? PimOrden { get; set; }

    public bool? PimActivo { get; set; }

    [ForeignKey("PimProductoId")]
    [InverseProperty("ProductosImagenes")]
    public virtual Producto PimProducto { get; set; } = null!;

    [ForeignKey("PimVarianteId")]
    [InverseProperty("ProductosImagenes")]
    public virtual ProductosVariante? PimVariante { get; set; }
}
