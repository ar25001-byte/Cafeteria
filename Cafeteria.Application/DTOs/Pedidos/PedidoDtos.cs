using System.ComponentModel.DataAnnotations;
using Cafeteria.Application.Common;
using Cafeteria.Domain.Enums;

namespace Cafeteria.Application.DTOs.Pedidos;

public class CrearDetallePedidoDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Debe especificar un ID de producto válido.")]
    public int ProductoId { get; set; }

    [Range(1, 1000, ErrorMessage = "La cantidad debe ser de al menos 1 unidad.")]
    public int Cantidad { get; set; }
}

public class CrearPedidoDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Debe especificar un ID de cliente válido.")]
    public int ClienteId { get; set; }

    [Required(ErrorMessage = "El pedido debe contener al menos un producto.")]
    [MinLength(1, ErrorMessage = "El pedido debe contener al menos un producto.")]
    public List<CrearDetallePedidoDto> Detalles { get; set; } = new();
}

public class CambiarEstadoPedidoDto
{
    [Required(ErrorMessage = "Debe especificar el nuevo estado del pedido.")]
    public EstadoPedido NuevoEstado { get; set; }
}

public class DetallePedidoResponseDto
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public class PedidoResponseDto
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public EstadoPedido Estado { get; set; }
    public string EstadoNombre => Estado.ToString();
    public decimal Subtotal { get; set; }
    public decimal Impuesto { get; set; }
    public decimal Total { get; set; }
    public List<DetallePedidoResponseDto> Detalles { get; set; } = new();
}

public class PedidoFilterParams : PaginationParams
{
    public int? ClienteId { get; set; }
    public EstadoPedido? Estado { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
}
