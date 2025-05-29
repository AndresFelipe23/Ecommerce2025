using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

public partial class Banner
{
    [Key]
    public int BanId { get; set; }

    [StringLength(200)]
    public string? BanTitulo { get; set; }

    [StringLength(300)]
    public string? BanSubtitulo { get; set; }

    [StringLength(500)]
    public string BanImagen { get; set; } = null!;

    [StringLength(500)]
    public string? BanImagenMovil { get; set; }

    [StringLength(500)]
    public string? BanEnlace { get; set; }

    [StringLength(50)]
    public string? BanTextoBoton { get; set; }

    [StringLength(20)]
    public string? BanPosicion { get; set; }

    public int? BanOrden { get; set; }

    public DateTime? BanFechaInicio { get; set; }

    public DateTime? BanFechaFin { get; set; }

    public bool? BanActivo { get; set; }

    public DateTime? BanFechaCreacion { get; set; }
}
