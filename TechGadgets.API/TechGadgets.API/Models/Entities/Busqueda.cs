using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

[Index("BusFecha", Name = "IX_Busquedas_Fecha")]
[Index("BusTermino", Name = "IX_Busquedas_Termino")]
public partial class Busqueda
{
    [Key]
    public long BusId { get; set; }

    [StringLength(255)]
    public string BusTermino { get; set; } = null!;

    public int? BusResultados { get; set; }

    public int? BusUsuarioId { get; set; }

    [StringLength(100)]
    public string? BusSesionId { get; set; }

    public DateTime? BusFecha { get; set; }

    [ForeignKey("BusUsuarioId")]
    [InverseProperty("Busqueda")]
    public virtual Usuario? BusUsuario { get; set; }
}
