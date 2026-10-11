using Cafeteria.Application.Common;
using Cafeteria.Application.DTOs.Dashboard;
using Cafeteria.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cafeteria.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Empleado")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Obtiene un resumen integral de métricas: ventas del día/mes, conteos globales y alertas de stock crítico.
    /// </summary>
    [HttpGet("resumen")]
    [ProducesResponseType(typeof(ApiResponse<DashboardResumenDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerResumen()
    {
        var resumen = await _dashboardService.ObtenerResumenAsync();
        return Ok(ApiResponse<DashboardResumenDto>.Ok(resumen, "Resumen del dashboard generado exitosamente."));
    }

    /// <summary>
    /// Obtiene los productos con mayor cantidad de unidades vendidas.
    /// </summary>
    [HttpGet("top-productos")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TopProductoDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTopProductos([FromQuery] int limite = 5)
    {
        var top = await _dashboardService.ObtenerTopProductosAsync(limite);
        return Ok(ApiResponse<IEnumerable<TopProductoDto>>.Ok(top, "Top de productos obtenido correctamente."));
    }

    /// <summary>
    /// Obtiene datos cronológicos agregados para graficar el comportamiento de ventas por semana o mes.
    /// </summary>
    [HttpGet("ventas-grafico")]
    [ProducesResponseType(typeof(ApiResponse<VentasGraficoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerVentasGrafico([FromQuery] string periodo = "semana")
    {
        var grafico = await _dashboardService.ObtenerVentasGraficoAsync(periodo);
        return Ok(ApiResponse<VentasGraficoDto>.Ok(grafico, "Datos de gráfico generados exitosamente."));
    }

    /// <summary>
    /// Obtiene los pedidos más recientes con su estado y monto total.
    /// </summary>
    [HttpGet("pedidos-recientes")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PedidoRecienteDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerPedidosRecientes([FromQuery] int limite = 10)
    {
        var recientes = await _dashboardService.ObtenerPedidosRecientesAsync(limite);
        return Ok(ApiResponse<IEnumerable<PedidoRecienteDto>>.Ok(recientes, "Pedidos recientes obtenidos correctamente."));
    }
}
