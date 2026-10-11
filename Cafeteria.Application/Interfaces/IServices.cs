using Cafeteria.Application.Common;
using Cafeteria.Application.DTOs.Auth;
using Cafeteria.Application.DTOs.Categorias;
using Cafeteria.Application.DTOs.Clientes;
using Cafeteria.Application.DTOs.Dashboard;
using Cafeteria.Application.DTOs.Pedidos;
using Cafeteria.Application.DTOs.Productos;
using Cafeteria.Domain.Entities;

namespace Cafeteria.Application.Interfaces;

public interface ICategoriaService
{
    Task<IEnumerable<CategoriaResponseDto>> ObtenerActivasAsync();
    Task<CategoriaResponseDto> ObtenerPorIdAsync(int id);
    Task<CategoriaResponseDto> CrearAsync(CrearCategoriaDto dto);
    Task<CategoriaResponseDto> ActualizarAsync(int id, ActualizarCategoriaDto dto);
    Task EliminarAsync(int id);
}

public interface IProductoService
{
    Task<PagedResult<ProductoResponseDto>> ObtenerTodosAsync(ProductoFilterParams filtros);
    Task<ProductoResponseDto> ObtenerPorIdAsync(int id);
    Task<ProductoResponseDto> CrearAsync(CrearProductoDto dto);
    Task<ProductoResponseDto> ActualizarAsync(int id, ActualizarProductoDto dto);
    Task EliminarAsync(int id);
    Task<ProductoResponseDto> AjustarStockAsync(int id, AjustarStockDto dto);
}

public interface IClienteService
{
    Task<PagedResult<ClienteResponseDto>> ObtenerTodosAsync(PaginationParams filtros);
    Task<ClienteResponseDto> ObtenerPorIdAsync(int id);
    Task<ClienteResponseDto> CrearAsync(CrearClienteDto dto);
    Task<ClienteResponseDto> ActualizarAsync(int id, ActualizarClienteDto dto);
    Task EliminarAsync(int id);
    Task<ClienteHistorialDto> ObtenerHistorialComprasAsync(int clienteId, PaginationParams filtros);
}

public interface IPedidoService
{
    Task<PagedResult<PedidoResponseDto>> ObtenerTodosAsync(PedidoFilterParams filtros);
    Task<PedidoResponseDto> ObtenerPorIdAsync(int id);
    Task<PedidoResponseDto> CrearAsync(CrearPedidoDto dto);
    Task<PedidoResponseDto> CambiarEstadoAsync(int id, CambiarEstadoPedidoDto dto);
    Task EliminarAsync(int id);
}

public interface IDashboardService
{
    Task<DashboardResumenDto> ObtenerResumenAsync();
    Task<IEnumerable<TopProductoDto>> ObtenerTopProductosAsync(int limite);
    Task<VentasGraficoDto> ObtenerVentasGraficoAsync(string periodo);
    Task<IEnumerable<PedidoRecienteDto>> ObtenerPedidosRecientesAsync(int limite);
}

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto dto);
    Task<UsuarioDto> RegistrarAsync(RegisterRequestDto dto);
}

public interface IJwtTokenGenerator
{
    string GenerarToken(Usuario usuario, out DateTime expiracion);
}
