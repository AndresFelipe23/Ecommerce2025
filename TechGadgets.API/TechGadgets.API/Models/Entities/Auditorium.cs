using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

[Index("AudFecha", Name = "IX_Auditoria_Fecha")]
[Index("AudTabla", Name = "IX_Auditoria_Tabla")]
public partial class Auditorium
{
    [Key]
    public long AudId { get; set; }

    [StringLength(100)]
    public string AudTabla { get; set; } = null!;

    [StringLength(10)]
    public string AudOperacion { get; set; } = null!;

    public int AudIdRegistro { get; set; }

    public string? AudValoresAnteriores { get; set; }

    public string? AudValoresNuevos { get; set; }

    public int? AudUsuarioId { get; set; }

    public DateTime? AudFecha { get; set; }

    [ForeignKey("AudUsuarioId")]
    [InverseProperty("Auditoria")]
    public virtual Usuario? AudUsuario { get; set; }
}
