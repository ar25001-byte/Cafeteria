using Cafeteria.Application.Common;
using Cafeteria.Application.DTOs.Pedidos;
using Cafeteria.Application.Exceptions;
using Cafeteria.Application.Interfaces;
using Cafeteria.Domain.Entities;
using Cafeteria.Domain.Enums;
using Cafeteria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cafeteria.Infrastructure.Services;

public class PedidoService : IPedidoService
{
    private readonly ApplicationDbContext _context;
    private const decimal TasaImpuesto = 0.16m; // 16% IVA

    public PedidoService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<PedidoResponseDto>> ObtenerTodosAsync(PedidoFilterParams filtros)
    {
        var query = _context.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
            .AsNoTracking()
            .AsQueryable();

        if (filtros.ClienteId.HasValue)
        {
            query = query.Where(p => p.ClienteId == filtros.ClienteId.Value);
        }

        if (filtros.Estado.HasValue)
        {
            query = query.Where(p => p.Estado == filtros.Estado.Value);
        }

        if (filtros.FechaInicio.HasValue)
        {
            var inicio = DateTime.SpecifyKind(filtros.FechaInicio.Value, DateTimeKind.Utc);
            query = query.Where(p => p.Fecha >= inicio);
        }

        if (filtros.FechaFin.HasValue)
        {
            var fin = DateTime.SpecifyKind(filtros.FechaFin.Value, DateTimeKind.Utc);
            query = query.Where(p => p.Fecha <= fin);
        }

        query = query.OrderByDescending(p => p.Fecha);

        var totalItems = await query.CountAsync();

        var items = await query
            .Skip((filtros.Pagina - 1) * filtros.TamanoPagina)
            .Take(filtros.TamanoPagina)
            .Select(p => new PedidoResponseDto
            {
                Id = p.Id,
                ClienteId = p.ClienteId,
                ClienteNombre = p.Cliente != null ? p.Cliente.Nombre : string.Empty,
                Fecha = p.Fecha,
                Estado = p.Estado,
                Subtotal = p.Subtotal,
                Impuesto = p.Impuesto,
                Total = p.Total,
                Detalles = p.Detalles.Select(d => new DetallePedidoResponseDto
                {
                    Id = d.Id,
                    ProductoId = d.ProductoId,
                    ProductoNombre = d.Producto != null ? d.Producto.Nombre : string.Empty,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal
                }).ToList()
            })
            .ToListAsync();

        return new PagedResult<PedidoResponseDto>(items, totalItems, filtros.Pagina, filtros.TamanoPagina);
    }

    public async Task<PedidoResponseDto> ObtenerPorIdAsync(int id)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido == null)
        {
            throw new NotFoundException(nameof(Pedido), id);
        }

        return new PedidoResponseDto
        {
            Id = pedido.Id,
            ClienteId = pedido.ClienteId,
            ClienteNombre = pedido.Cliente?.Nombre ?? string.Empty,
            Fecha = pedido.Fecha,
            Estado = pedido.Estado,
            Subtotal = pedido.Subtotal,
            Impuesto = pedido.Impuesto,
            Total = pedido.Total,
            Detalles = pedido.Detalles.Select(d => new DetallePedidoResponseDto
            {
                Id = d.Id,
                ProductoId = d.ProductoId,
                ProductoNombre = d.Producto?.Nombre ?? string.Empty,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = d.Subtotal
            }).ToList()
        };
    }

    public async Task<PedidoResponseDto> CrearAsync(CrearPedidoDto dto)
    {
        if (dto.Detalles == null || !dto.Detalles.Any())
        {
            throw new BadRequestException("El pedido debe contener al menos un producto.");
        }

        var cliente = await _context.Clientes.FindAsync(dto.ClienteId);
        if (cliente == null)
        {
            throw new NotFoundException(nameof(Cliente), dto.ClienteId);
        }

        // Agrupamos cantidades por si el cliente envió el mismo producto en múltiples líneas
        var itemsAgrupados = dto.Detalles
            .GroupBy(d => d.ProductoId)
            .Select(g => new { ProductoId = g.Key, Cantidad = g.Sum(x => x.Cantidad) })
            .ToList();

        var productoIds = itemsAgrupados.Select(i => i.ProductoId).ToList();
        var productos = await _context.Productos
            .Where(p => productoIds.Contains(p.Id))
            .ToListAsync();

        if (productos.Count != productoIds.Count)
        {
            var noEncontrados = productoIds.Except(productos.Select(p => p.Id));
            throw new BadRequestException($"Los siguientes productos no existen: {string.Join(", ", noEncontrados)}");
        }

        return await EjecutarEnTransaccionAsync(async () =>
        {
            var pedido = new Pedido
            {
                ClienteId = dto.ClienteId,
                Fecha = DateTime.UtcNow,
                Estado = EstadoPedido.Pendiente,
                Detalles = new List<DetallePedido>()
            };

            decimal subtotalAcumulado = 0m;

            foreach (var item in itemsAgrupados)
            {
                var producto = productos.First(p => p.Id == item.ProductoId);

                if (producto.Estado == EstadoProducto.Inactivo)
                {
                    throw new BadRequestException($"El producto '{producto.Nombre}' está inactivo y no se puede vender.");
                }

                if (producto.Stock < item.Cantidad)
                {
                    throw new BadRequestException(
                        $"Stock insuficiente para el producto '{producto.Nombre}'. Solicitado: {item.Cantidad}, Disponible: {producto.Stock}.");
                }

                // Descuento automático de stock
                producto.Stock -= item.Cantidad;
                if (producto.Stock == 0)
                {
                    producto.Estado = EstadoProducto.Agotado;
                }

                var subtotalLinea = item.Cantidad * producto.Precio;
                subtotalAcumulado += subtotalLinea;

                pedido.Detalles.Add(new DetallePedido
                {
                    ProductoId = producto.Id,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = producto.Precio,
                    Subtotal = subtotalLinea
                });
            }

            // Cálculo seguro en el servidor
            pedido.Subtotal = Math.Round(subtotalAcumulado, 2);
            pedido.Impuesto = Math.Round(pedido.Subtotal * TasaImpuesto, 2);
            pedido.Total = Math.Round(pedido.Subtotal + pedido.Impuesto, 2);

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            return new PedidoResponseDto
            {
                Id = pedido.Id,
                ClienteId = pedido.ClienteId,
                ClienteNombre = cliente.Nombre,
                Fecha = pedido.Fecha,
                Estado = pedido.Estado,
                Subtotal = pedido.Subtotal,
                Impuesto = pedido.Impuesto,
                Total = pedido.Total,
                Detalles = pedido.Detalles.Select(d =>
                {
                    var prod = productos.First(p => p.Id == d.ProductoId);
                    return new DetallePedidoResponseDto
                    {
                        Id = d.Id,
                        ProductoId = d.ProductoId,
                        ProductoNombre = prod.Nombre,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Subtotal
                    };
                }).ToList()
            };
        });
    }

    public async Task<PedidoResponseDto> CambiarEstadoAsync(int id, CambiarEstadoPedidoDto dto)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido == null)
        {
            throw new NotFoundException(nameof(Pedido), id);
        }

        if (pedido.Estado == dto.NuevoEstado)
        {
            return await ObtenerPorIdAsync(id);
        }

        // Validar transiciones prohibidas
        if (pedido.Estado == EstadoPedido.Cancelado)
        {
            throw new BadRequestException("Un pedido cancelado no puede cambiar de estado nuevamente.");
        }

        if (pedido.Estado == EstadoPedido.Entregado && dto.NuevoEstado != EstadoPedido.Cancelado)
        {
            throw new BadRequestException("Un pedido ya entregado solo puede ser cancelado por devolución autorizada.");
        }

        return await EjecutarEnTransaccionAsync(async () =>
        {
            // Regla de Negocio: Si un pedido se cancela, restaurar el stock de los productos
            if (dto.NuevoEstado == EstadoPedido.Cancelado && pedido.Estado != EstadoPedido.Cancelado)
            {
                foreach (var detalle in pedido.Detalles)
                {
                    if (detalle.Producto != null)
                    {
                        detalle.Producto.Stock += detalle.Cantidad;

                        if (detalle.Producto.Stock > 0 && detalle.Producto.Estado == EstadoProducto.Agotado)
                        {
                            detalle.Producto.Estado = EstadoProducto.Disponible;
                        }
                    }
                }
            }

            pedido.Estado = dto.NuevoEstado;
            await _context.SaveChangesAsync();

            return new PedidoResponseDto
            {
                Id = pedido.Id,
                ClienteId = pedido.ClienteId,
                ClienteNombre = pedido.Cliente?.Nombre ?? string.Empty,
                Fecha = pedido.Fecha,
                Estado = pedido.Estado,
                Subtotal = pedido.Subtotal,
                Impuesto = pedido.Impuesto,
                Total = pedido.Total,
                Detalles = pedido.Detalles.Select(d => new DetallePedidoResponseDto
                {
                    Id = d.Id,
                    ProductoId = d.ProductoId,
                    ProductoNombre = d.Producto?.Nombre ?? string.Empty,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal
                }).ToList()
            };
        });
    }

    public async Task EliminarAsync(int id)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido == null)
        {
            throw new NotFoundException(nameof(Pedido), id);
        }

        await EjecutarEnTransaccionAsync(async () =>
        {
            // Si el pedido no estaba cancelado, restauramos stock antes de su eliminación
            if (pedido.Estado != EstadoPedido.Cancelado)
            {
                foreach (var detalle in pedido.Detalles)
                {
                    if (detalle.Producto != null)
                    {
                        detalle.Producto.Stock += detalle.Cantidad;
                        if (detalle.Producto.Stock > 0 && detalle.Producto.Estado == EstadoProducto.Agotado)
                        {
                            detalle.Producto.Estado = EstadoProducto.Disponible;
                        }
                    }
                }
            }

            _context.Pedidos.Remove(pedido);
            await _context.SaveChangesAsync();
        });
    }

    private async Task<T> EjecutarEnTransaccionAsync<T>(Func<Task<T>> action)
    {
        var isInMemory = _context.Database.ProviderName?.Contains("InMemory") == true;
        if (isInMemory)
        {
            return await action();
        }

        var executionStrategy = _context.Database.CreateExecutionStrategy();
        return await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await action();
                await transaction.CommitAsync();
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    private async Task EjecutarEnTransaccionAsync(Func<Task> action)
    {
        var isInMemory = _context.Database.ProviderName?.Contains("InMemory") == true;
        if (isInMemory)
        {
            await action();
            return;
        }

        var executionStrategy = _context.Database.CreateExecutionStrategy();
        await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await action();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }
}

