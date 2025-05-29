using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

[Table("Carrito")]
[Index("CarProductoId", Name = "IX_Carrito_Producto")]
[Index("CarSesionId", Name = "IX_Carrito_Sesion")]
[Index("CarUsuarioId", Name = "IX_Carrito_Usuario")]
public partial class Carrito
{
    [Key]
    public int CarId { get; set; }

    public int? CarUsuarioId { get; set; }

    [StringLength(100)]
    public string? CarSesionId { get; set; }

    public int CarProductoId { get; set; }

    public int? CarVarianteId { get; set; }

    public int CarCantidad { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal CarPrecio { get; set; }

    public DateTime? CarFechaCreacion { get; set; }

    public DateTime? CarFechaModificacion { get; set; }

    [ForeignKey("CarProductoId")]
    [InverseProperty("Carritos")]
    public virtual Producto CarProducto { get; set; } = null!;

    [ForeignKey("CarUsuarioId")]
    [InverseProperty("Carritos")]
    public virtual Usuario? CarUsuario { get; set; }

    [ForeignKey("CarVarianteId")]
    [InverseProperty("Carritos")]
    public virtual ProductosVariante? CarVariante { get; set; }
}
