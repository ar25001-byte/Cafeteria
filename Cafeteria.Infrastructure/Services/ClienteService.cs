using Cafeteria.Application.Common;
using Cafeteria.Application.DTOs.Clientes;
using Cafeteria.Application.DTOs.Pedidos;
using Cafeteria.Application.Exceptions;
using Cafeteria.Application.Interfaces;
using Cafeteria.Domain.Entities;
using Cafeteria.Domain.Enums;
using Cafeteria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cafeteria.Infrastructure.Services;

public class ClienteService : IClienteService
{
    private readonly ApplicationDbContext _context;

    public ClienteService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ClienteResponseDto>> ObtenerTodosAsync(PaginationParams filtros)
    {
        var query = _context.Clientes
            .Include(c => c.Pedidos)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtros.Busqueda))
        {
            var search = filtros.Busqueda.Trim().ToLower();
            query = query.Where(c => c.Nombre.ToLower().Contains(search) || c.Email.ToLower().Contains(search));
        }

        query = query.OrderBy(c => c.Nombre);

        var totalItems = await query.CountAsync();

        var items = await query
            .Skip((filtros.Pagina - 1) * filtros.TamanoPagina)
            .Take(filtros.TamanoPagina)
            .Select(c => new ClienteResponseDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Telefono = c.Telefono,
                Email = c.Email,
                FechaRegistro = c.FechaRegistro,
                TotalPedidosRealizados = c.Pedidos.Count,
                TotalGastado = c.Pedidos.Where(p => p.Estado != EstadoPedido.Cancelado).Sum(p => p.Total)
            })
            .ToListAsync();

        return new PagedResult<ClienteResponseDto>(items, totalItems, filtros.Pagina, filtros.TamanoPagina);
    }

    public async Task<ClienteResponseDto> ObtenerPorIdAsync(int id)
    {
        var cliente = await _context.Clientes
            .Include(c => c.Pedidos)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cliente == null)
        {
            throw new NotFoundException(nameof(Cliente), id);
        }

        return new ClienteResponseDto
        {
            Id = cliente.Id,
            Nombre = cliente.Nombre,
            Telefono = cliente.Telefono,
            Email = cliente.Email,
            FechaRegistro = cliente.FechaRegistro,
            TotalPedidosRealizados = cliente.Pedidos.Count,
            TotalGastado = cliente.Pedidos.Where(p => p.Estado != EstadoPedido.Cancelado).Sum(p => p.Total)
        };
    }

    public async Task<ClienteResponseDto> CrearAsync(CrearClienteDto dto)
    {
        var emailExiste = await _context.Clientes.AnyAsync(c => c.Email.ToLower() == dto.Email.Trim().ToLower());
        if (emailExiste)
        {
            throw new ConflictException($"Ya existe un cliente con el correo electrónico '{dto.Email}'.");
        }

        var cliente = new Cliente
        {
            Nombre = dto.Nombre.Trim(),
            Telefono = dto.Telefono.Trim(),
            Email = dto.Email.Trim().ToLower(),
            FechaRegistro = DateTime.UtcNow
        };

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        return new ClienteResponseDto
        {
            Id = cliente.Id,
            Nombre = cliente.Nombre,
            Telefono = cliente.Telefono,
            Email = cliente.Email,
            FechaRegistro = cliente.FechaRegistro,
            TotalPedidosRealizados = 0,
            TotalGastado = 0
        };
    }

    public async Task<ClienteResponseDto> ActualizarAsync(int id, ActualizarClienteDto dto)
    {
        var cliente = await _context.Clientes
            .Include(c => c.Pedidos)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cliente == null)
        {
            throw new NotFoundException(nameof(Cliente), id);
        }

        var emailExiste = await _context.Clientes
            .AnyAsync(c => c.Id != id && c.Email.ToLower() == dto.Email.Trim().ToLower());

        if (emailExiste)
        {
            throw new ConflictException($"Ya existe otro cliente con el correo electrónico '{dto.Email}'.");
        }

        cliente.Nombre = dto.Nombre.Trim();
        cliente.Telefono = dto.Telefono.Trim();
        cliente.Email = dto.Email.Trim().ToLower();

        await _context.SaveChangesAsync();

        return new ClienteResponseDto
        {
            Id = cliente.Id,
            Nombre = cliente.Nombre,
            Telefono = cliente.Telefono,
            Email = cliente.Email,
            FechaRegistro = cliente.FechaRegistro,
            TotalPedidosRealizados = cliente.Pedidos.Count,
            TotalGastado = cliente.Pedidos.Where(p => p.Estado != EstadoPedido.Cancelado).Sum(p => p.Total)
        };
    }

    public async Task EliminarAsync(int id)
    {
        var cliente = await _context.Clientes
            .Include(c => c.Pedidos)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cliente == null)
        {
            throw new NotFoundException(nameof(Cliente), id);
        }

        if (cliente.Pedidos.Any())
        {
            throw new BadRequestException("No se puede eliminar el cliente porque tiene pedidos históricos registrados.");
        }

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();
    }

    public async Task<ClienteHistorialDto> ObtenerHistorialComprasAsync(int clienteId, PaginationParams filtros)
    {
        var cliente = await _context.Clientes
            .Include(c => c.Pedidos)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == clienteId);

        if (cliente == null)
        {
            throw new NotFoundException(nameof(Cliente), clienteId);
        }

        var query = _context.Pedidos
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
            .Where(p => p.ClienteId == clienteId)
            .OrderByDescending(p => p.Fecha)
            .AsNoTracking();

        var totalPedidos = await query.CountAsync();

        var pedidos = await query
            .Skip((filtros.Pagina - 1) * filtros.TamanoPagina)
            .Take(filtros.TamanoPagina)
            .Select(p => new PedidoResponseDto
            {
                Id = p.Id,
                ClienteId = p.ClienteId,
                ClienteNombre = cliente.Nombre,
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

        var pagedPedidos = new PagedResult<PedidoResponseDto>(pedidos, totalPedidos, filtros.Pagina, filtros.TamanoPagina);

        return new ClienteHistorialDto
        {
            Cliente = new ClienteResponseDto
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Telefono = cliente.Telefono,
                Email = cliente.Email,
                FechaRegistro = cliente.FechaRegistro,
                TotalPedidosRealizados = cliente.Pedidos.Count,
                TotalGastado = cliente.Pedidos.Where(p => p.Estado != EstadoPedido.Cancelado).Sum(p => p.Total)
            },
            Pedidos = pagedPedidos
        };
    }
}
