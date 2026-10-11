using System.Globalization;
using Cafeteria.Application.DTOs.Dashboard;
using Cafeteria.Application.DTOs.Productos;
using Cafeteria.Application.Interfaces;
using Cafeteria.Domain.Enums;
using Cafeteria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cafeteria.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardResumenDto> ObtenerResumenAsync()
    {
        var hoy = DateTime.UtcNow.Date;
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var finHoy = hoy.AddDays(1);

        // Pedidos completados/activos (excluyendo cancelados para metricas de venta reales)
        var pedidosActivos = _context.Pedidos.Where(p => p.Estado != EstadoPedido.Cancelado);

        // Ventas del día
        var pedidosDia = await pedidosActivos
            .Where(p => p.Fecha >= hoy && p.Fecha < finHoy)
            .ToListAsync();

        var ventasDiaMonto = pedidosDia.Sum(p => p.Total);
        var ventasDiaCantidad = pedidosDia.Count;

        // Ventas del mes
        var pedidosMes = await pedidosActivos
            .Where(p => p.Fecha >= inicioMes)
            .ToListAsync();

        var ventasMesMonto = pedidosMes.Sum(p => p.Total);
        var ventasMesCantidad = pedidosMes.Count;

        // Totales globales
        var totalPedidos = await _context.Pedidos.CountAsync();
        var totalClientes = await _context.Clientes.CountAsync();
        var totalProductosDisponibles = await _context.Productos
            .CountAsync(p => p.Estado == EstadoProducto.Disponible && p.Stock > 0);

        // Productos en stock crítico
        var productosCriticos = await _context.Productos
            .Include(p => p.Categoria)
            .Where(p => p.Estado != EstadoProducto.Inactivo && p.Stock <= p.StockMinimo)
            .OrderBy(p => p.Stock)
            .Select(p => new ProductoResponseDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Precio = p.Precio,
                Stock = p.Stock,
                StockMinimo = p.StockMinimo,
                ImagenUrl = p.ImagenUrl,
                CategoriaId = p.CategoriaId,
                CategoriaNombre = p.Categoria != null ? p.Categoria.Nombre : string.Empty,
                Estado = p.Estado,
                FechaCreacion = p.FechaCreacion
            })
            .ToListAsync();

        return new DashboardResumenDto
        {
            VentasDelDia = new ResumenVentaItemDto
            {
                MontoTotal = Math.Round(ventasDiaMonto, 2),
                CantidadPedidos = ventasDiaCantidad
            },
            VentasDelMes = new ResumenVentaItemDto
            {
                MontoTotal = Math.Round(ventasMesMonto, 2),
                CantidadPedidos = ventasMesCantidad
            },
            TotalPedidosRealizados = totalPedidos,
            TotalClientesRegistrados = totalClientes,
            TotalProductosDisponibles = totalProductosDisponibles,
            ProductosStockCritico = productosCriticos
        };
    }

    public async Task<IEnumerable<TopProductoDto>> ObtenerTopProductosAsync(int limite)
    {
        limite = limite <= 0 ? 5 : limite;

        var top = await _context.DetallesPedido
            .Include(d => d.Pedido)
            .Include(d => d.Producto)
                .ThenInclude(p => p!.Categoria)
            .Where(d => d.Pedido != null && d.Pedido.Estado != EstadoPedido.Cancelado)
            .GroupBy(d => new
            {
                d.ProductoId,
                Nombre = d.Producto != null ? d.Producto.Nombre : "Desconocido",
                CategoriaNombre = d.Producto != null && d.Producto.Categoria != null ? d.Producto.Categoria.Nombre : "Sin categoría"
            })
            .Select(g => new TopProductoDto
            {
                ProductoId = g.Key.ProductoId,
                Nombre = g.Key.Nombre,
                CategoriaNombre = g.Key.CategoriaNombre,
                UnidadesVendidas = g.Sum(x => x.Cantidad),
                TotalRecaudado = Math.Round(g.Sum(x => x.Subtotal), 2)
            })
            .OrderByDescending(x => x.UnidadesVendidas)
            .Take(limite)
            .ToListAsync();

        return top;
    }

    public async Task<VentasGraficoDto> ObtenerVentasGraficoAsync(string periodo)
    {
        var periodoNormalizado = (periodo ?? "semana").ToLower().Trim();
        var hoy = DateTime.UtcNow.Date;
        var culture = new CultureInfo("es-ES");

        DateTime fechaInicio;
        int dias;

        if (periodoNormalizado == "mes")
        {
            dias = 30;
            fechaInicio = hoy.AddDays(-29);
        }
        else
        {
            periodoNormalizado = "semana";
            dias = 7;
            fechaInicio = hoy.AddDays(-6);
        }

        var pedidosPeriodo = await _context.Pedidos
            .Where(p => p.Estado != EstadoPedido.Cancelado && p.Fecha >= fechaInicio)
            .Select(p => new { Fecha = p.Fecha.Date, p.Total })
            .ToListAsync();

        var puntos = new List<VentasGraficoPuntoDto>();
        decimal acumuladoPeriodo = 0m;
        int pedidosPeriodoTotal = 0;

        for (int i = 0; i < dias; i++)
        {
            var diaIterado = fechaInicio.AddDays(i);
            var pedidosDelDia = pedidosPeriodo.Where(p => p.Fecha == diaIterado).ToList();

            var montoDia = pedidosDelDia.Sum(p => p.Total);
            var cantidadDia = pedidosDelDia.Count;

            acumuladoPeriodo += montoDia;
            pedidosPeriodoTotal += cantidadDia;

            puntos.Add(new VentasGraficoPuntoDto
            {
                Fecha = diaIterado.ToString("yyyy-MM-dd"),
                DiaNombre = culture.DateTimeFormat.GetDayName(diaIterado.DayOfWeek),
                MontoTotal = Math.Round(montoDia, 2),
                CantidadPedidos = cantidadDia
            });
        }

        return new VentasGraficoDto
        {
            Periodo = periodoNormalizado,
            MontoTotalPeriodo = Math.Round(acumuladoPeriodo, 2),
            PedidosTotalPeriodo = pedidosPeriodoTotal,
            DatosGrafico = puntos
        };
    }

    public async Task<IEnumerable<PedidoRecienteDto>> ObtenerPedidosRecientesAsync(int limite)
    {
        limite = limite <= 0 ? 10 : limite;

        return await _context.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Detalles)
            .OrderByDescending(p => p.Fecha)
            .Take(limite)
            .Select(p => new PedidoRecienteDto
            {
                Id = p.Id,
                ClienteNombre = p.Cliente != null ? p.Cliente.Nombre : "Sin cliente",
                Fecha = p.Fecha,
                Estado = p.Estado.ToString(),
                Total = p.Total,
                TotalItems = p.Detalles.Sum(d => d.Cantidad)
            })
            .ToListAsync();
    }
}
