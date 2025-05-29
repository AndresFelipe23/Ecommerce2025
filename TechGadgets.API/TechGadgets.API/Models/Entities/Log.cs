using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

[Index("LogFecha", Name = "IX_Logs_Fecha")]
[Index("LogNivel", Name = "IX_Logs_Nivel")]
public partial class Log
{
    [Key]
    public long LogId { get; set; }

    [StringLength(20)]
    public string LogNivel { get; set; } = null!;

    public string LogMensaje { get; set; } = null!;

    public string? LogExcepcion { get; set; }

    public int? LogUsuarioId { get; set; }

    [Column("LogDireccionIP")]
    [StringLength(45)]
    public string? LogDireccionIp { get; set; }

    [StringLength(500)]
    public string? LogUserAgent { get; set; }

    [StringLength(500)]
    public string? LogUrl { get; set; }

    public DateTime? LogFecha { get; set; }

    [ForeignKey("LogUsuarioId")]
    [InverseProperty("Logs")]
    public virtual Usuario? LogUsuario { get; set; }
}
