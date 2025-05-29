using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechGadgets.API.Models.Entities;

[Index("UsuActivo", Name = "IX_Usuarios_Activo")]
[Index("UsuEmail", Name = "IX_Usuarios_Email")]
[Index("UsuFechaCreacion", Name = "IX_Usuarios_FechaCreacion")]
[Index("UsuEmail", Name = "UQ__Usuarios__0FE50E26A42C2B6A", IsUnique = true)]
public partial class Usuario
{
    [Key]
    public int UsuId { get; set; }

    [StringLength(255)]
    public string UsuEmail { get; set; } = null!;

    [StringLength(255)]
    public string UsuPassword { get; set; } = null!;

    [StringLength(100)]
    public string UsuNombre { get; set; } = null!;

    [StringLength(100)]
    public string UsuApellido { get; set; } = null!;

    [StringLength(20)]
    public string? UsuTelefono { get; set; }

    public DateTime? UsuFechaNacimiento { get; set; }

    [StringLength(1)]
    [Unicode(false)]
    public string? UsuGenero { get; set; }

    public bool? UsuActivo { get; set; }

    public bool? UsuEmailVerificado { get; set; }

    public bool? UsuTelefonoVerificado { get; set; }

    public DateTime? UsuFechaCreacion { get; set; }

    public DateTime? UsuFechaModificacion { get; set; }

    public DateTime? UsuUltimoAcceso { get; set; }

    public int? UsuIntentosFallidos { get; set; }

    public DateTime? UsuBloqueadoHasta { get; set; }

    [InverseProperty("AudUsuario")]
    public virtual ICollection<Auditorium> Auditoria { get; set; } = new List<Auditorium>();

    [InverseProperty("BusUsuario")]
    public virtual ICollection<Busqueda> Busqueda { get; set; } = new List<Busqueda>();

    [InverseProperty("CarUsuario")]
    public virtual ICollection<Carrito> Carritos { get; set; } = new List<Carrito>();

    [InverseProperty("DirUsuario")]
    public virtual ICollection<Direccione> Direcciones { get; set; } = new List<Direccione>();

    [InverseProperty("LisUsuario")]
    public virtual ICollection<ListaDeseo> ListaDeseos { get; set; } = new List<ListaDeseo>();

    [InverseProperty("LogUsuario")]
    public virtual ICollection<Log> Logs { get; set; } = new List<Log>();

    [InverseProperty("MovUsuario")]
    public virtual ICollection<MovimientosInventario> MovimientosInventarios { get; set; } = new List<MovimientosInventario>();

    [InverseProperty("PedUsuario")]
    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();

    [InverseProperty("PhiUsuario")]
    public virtual ICollection<PedidosHistorial> PedidosHistorials { get; set; } = new List<PedidosHistorial>();

    [InverseProperty("PviUsuario")]
    public virtual ICollection<ProductosVista> ProductosVista { get; set; } = new List<ProductosVista>();

    [InverseProperty("ResUsuario")]
    public virtual ICollection<Reseña> Reseñas { get; set; } = new List<Reseña>();

    [InverseProperty("UsrUsuario")]
    public virtual ICollection<UsuariosRole> UsuariosRoles { get; set; } = new List<UsuariosRole>();
}
