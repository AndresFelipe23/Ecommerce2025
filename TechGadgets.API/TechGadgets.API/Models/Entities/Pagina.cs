using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

[Index("PagSlug", Name = "UQ__Paginas__AA717165ADCE6412", IsUnique = true)]
public partial class Pagina
{
    [Key]
    public int PagId { get; set; }

    [StringLength(200)]
    public string PagTitulo { get; set; } = null!;

    [StringLength(200)]
    public string PagSlug { get; set; } = null!;

    [Column(TypeName = "ntext")]
    public string? PagContenido { get; set; }

    [StringLength(200)]
    public string? PagMetaTitulo { get; set; }

    [StringLength(500)]
    public string? PagMetaDescripcion { get; set; }

    public bool? PagActivo { get; set; }

    public DateTime? PagFechaCreacion { get; set; }

    public DateTime? PagFechaModificacion { get; set; }
}
