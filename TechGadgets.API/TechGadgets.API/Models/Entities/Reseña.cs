using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

[Index("ResProductoId", "ResUsuarioId", Name = "UQ__Reseñas__395FCE8D6C4ECACD", IsUnique = true)]
public partial class Reseña
{
    [Key]
    public int ResId { get; set; }

    public int ResProductoId { get; set; }

    public int ResUsuarioId { get; set; }

    public int? ResPedidoId { get; set; }

    public int ResCalificacion { get; set; }

    [StringLength(200)]
    public string? ResTitulo { get; set; }

    [Column(TypeName = "ntext")]
    public string? ResComentario { get; set; }

    public bool? ResAprobado { get; set; }

    public int? ResUtil { get; set; }

    public DateTime? ResFechaCreacion { get; set; }

    [ForeignKey("ResPedidoId")]
    [InverseProperty("Reseñas")]
    public virtual Pedido? ResPedido { get; set; }

    [ForeignKey("ResProductoId")]
    [InverseProperty("Reseñas")]
    public virtual Producto ResProducto { get; set; } = null!;

    [ForeignKey("ResUsuarioId")]
    [InverseProperty("Reseñas")]
    public virtual Usuario ResUsuario { get; set; } = null!;
}
