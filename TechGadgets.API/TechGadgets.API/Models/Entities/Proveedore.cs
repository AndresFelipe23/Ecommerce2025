using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

public partial class Proveedore
{
    [Key]
    public int ProId { get; set; }

    [StringLength(150)]
    public string ProNombre { get; set; } = null!;

    [StringLength(100)]
    public string? ProContacto { get; set; }

    [StringLength(255)]
    public string? ProEmail { get; set; }

    [StringLength(20)]
    public string? ProTelefono { get; set; }

    [StringLength(255)]
    public string? ProDireccion { get; set; }

    public int? ProCiudadId { get; set; }

    [StringLength(255)]
    public string? ProSitioWeb { get; set; }

    public int? ProTiempoEntrega { get; set; }

    [StringLength(100)]
    public string? ProTerminosPago { get; set; }

    public bool? ProActivo { get; set; }

    public DateTime? ProFechaCreacion { get; set; }

    [ForeignKey("ProCiudadId")]
    [InverseProperty("Proveedores")]
    public virtual Ciudade? ProCiudad { get; set; }

    [InverseProperty("PprProveedor")]
    public virtual ICollection<ProductosProveedore> ProductosProveedores { get; set; } = new List<ProductosProveedore>();
}
