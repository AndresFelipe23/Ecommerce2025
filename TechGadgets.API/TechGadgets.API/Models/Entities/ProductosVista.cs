using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

[Index("PviFecha", Name = "IX_ProductosVistas_Fecha")]
[Index("PviProductoId", Name = "IX_ProductosVistas_Producto")]
public partial class ProductosVista
{
    [Key]
    public long PviId { get; set; }

    public int PviProductoId { get; set; }

    public int? PviUsuarioId { get; set; }

    [StringLength(100)]
    public string? PviSesionId { get; set; }

    [Column("PviDireccionIP")]
    [StringLength(45)]
    public string? PviDireccionIp { get; set; }

    [StringLength(500)]
    public string? PviUserAgent { get; set; }

    [StringLength(500)]
    public string? PviReferrer { get; set; }

    public DateTime? PviFecha { get; set; }

    [ForeignKey("PviProductoId")]
    [InverseProperty("ProductosVista")]
    public virtual Producto PviProducto { get; set; } = null!;

    [ForeignKey("PviUsuarioId")]
    [InverseProperty("ProductosVista")]
    public virtual Usuario? PviUsuario { get; set; }
}
