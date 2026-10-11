using System.ComponentModel.DataAnnotations;
using Cafeteria.Application.Common;
using Cafeteria.Application.DTOs.Pedidos;

namespace Cafeteria.Application.DTOs.Clientes;

public class CrearClienteDto
{
    [Required(ErrorMessage = "El nombre del cliente es obligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [Phone(ErrorMessage = "El formato del teléfono no es válido.")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
    public string Email { get; set; } = string.Empty;
}

public class ActualizarClienteDto
{
    [Required(ErrorMessage = "El nombre del cliente es obligatorio.")]
    [StringLength(100, MinimumLength = 3)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [Phone]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class ClienteResponseDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
    public int TotalPedidosRealizados { get; set; }
    public decimal TotalGastado { get; set; }
}

public class ClienteHistorialDto
{
    public ClienteResponseDto Cliente { get; set; } = null!;
    public PagedResult<PedidoResponseDto> Pedidos { get; set; } = null!;
}
