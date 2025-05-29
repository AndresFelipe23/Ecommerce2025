using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

public partial class ValoresAtributo
{
    [Key]
    public int ValId { get; set; }

    public int ValAtributoId { get; set; }

    [StringLength(100)]
    public string ValValor { get; set; } = null!;

    [StringLength(7)]
    public string? ValCodigoColor { get; set; }

    public int? ValOrden { get; set; }

    public bool? ValActivo { get; set; }

    [ForeignKey("ValAtributoId")]
    [InverseProperty("ValoresAtributos")]
    public virtual Atributo ValAtributo { get; set; } = null!;

    [ForeignKey("VatValorAtributoId")]
    [InverseProperty("VatValorAtributos")]
    public virtual ICollection<ProductosVariante> VatVariantes { get; set; } = new List<ProductosVariante>();
}
