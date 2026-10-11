using Cafeteria.Application.DTOs.Productos;

namespace Cafeteria.Application.DTOs.Dashboard;

public class ResumenVentaItemDto
{
    public decimal MontoTotal { get; set; }
    public int CantidadPedidos { get; set; }
}

public class DashboardResumenDto
{
    public ResumenVentaItemDto VentasDelDia { get; set; } = new();
    public ResumenVentaItemDto VentasDelMes { get; set; } = new();
    public int TotalPedidosRealizados { get; set; }
    public int TotalClientesRegistrados { get; set; }
    public int TotalProductosDisponibles { get; set; }
    public List<ProductoResponseDto> ProductosStockCritico { get; set; } = new();
}

public class TopProductoDto
{
    public int ProductoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string CategoriaNombre { get; set; } = string.Empty;
    public int UnidadesVendidas { get; set; }
    public decimal TotalRecaudado { get; set; }
}

public class VentasGraficoPuntoDto
{
    public string Fecha { get; set; } = string.Empty;
    public string DiaNombre { get; set; } = string.Empty;
    public decimal MontoTotal { get; set; }
    public int CantidadPedidos { get; set; }
}

public class VentasGraficoDto
{
    public string Periodo { get; set; } = string.Empty; // "semana" | "mes"
    public decimal MontoTotalPeriodo { get; set; }
    public int PedidosTotalPeriodo { get; set; }
    public List<VentasGraficoPuntoDto> DatosGrafico { get; set; } = new();
}

public class PedidoRecienteDto
{
    public int Id { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public int TotalItems { get; set; }
}
