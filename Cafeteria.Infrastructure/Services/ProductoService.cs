using Cafeteria.Application.Common;
using Cafeteria.Application.DTOs.Productos;
using Cafeteria.Application.Exceptions;
using Cafeteria.Application.Interfaces;
using Cafeteria.Domain.Entities;
using Cafeteria.Domain.Enums;
using Cafeteria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cafeteria.Infrastructure.Services;

public class ProductoService : IProductoService
{
    private readonly ApplicationDbContext _context;

    public ProductoService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ProductoResponseDto>> ObtenerTodosAsync(ProductoFilterParams filtros)
    {
        var query = _context.Productos
            .Include(p => p.Categoria)
            .AsNoTracking()
            .AsQueryable();

        // Filtro por término de búsqueda (nombre o descripción)
        if (!string.IsNullOrWhiteSpace(filtros.Busqueda))
        {
            var search = filtros.Busqueda.Trim().ToLower();
            query = query.Where(p => p.Nombre.ToLower().Contains(search) || 
                                     (p.Descripcion != null && p.Descripcion.ToLower().Contains(search)));
        }

        // Filtro por categoría
        if (filtros.CategoriaId.HasValue)
        {
            query = query.Where(p => p.CategoriaId == filtros.CategoriaId.Value);
        }

        // Filtro por estado
        if (filtros.Estado.HasValue)
        {
            query = query.Where(p => p.Estado == filtros.Estado.Value);
        }

        // Filtro por disponibilidad
        if (filtros.SoloDisponibles == true)
        {
            query = query.Where(p => p.Estado == EstadoProducto.Disponible && p.Stock > 0);
        }

        // Ordenamiento
        if (!string.IsNullOrWhiteSpace(filtros.OrdenarPrecio))
        {
            if (filtros.OrdenarPrecio.ToLower() == "asc")
            {
                query = query.OrderBy(p => p.Precio);
            }
            else if (filtros.OrdenarPrecio.ToLower() == "desc")
            {
                query = query.OrderByDescending(p => p.Precio);
            }
        }
        else
        {
            query = query.OrderBy(p => p.Nombre);
        }

        var totalItems = await query.CountAsync();

        var items = await query
            .Skip((filtros.Pagina - 1) * filtros.TamanoPagina)
            .Take(filtros.TamanoPagina)
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

        return new PagedResult<ProductoResponseDto>(items, totalItems, filtros.Pagina, filtros.TamanoPagina);
    }

    public async Task<ProductoResponseDto> ObtenerPorIdAsync(int id)
    {
        var producto = await _context.Productos
            .Include(p => p.Categoria)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (producto == null)
        {
            throw new NotFoundException(nameof(Producto), id);
        }

        return MapToDto(producto);
    }

    public async Task<ProductoResponseDto> CrearAsync(CrearProductoDto dto)
    {
        var categoria = await _context.Categorias.FindAsync(dto.CategoriaId);
        if (categoria == null || categoria.Estado != EstadoCategoria.Activo)
        {
            throw new BadRequestException("La categoría especificada no existe o se encuentra inactiva.");
        }

        var estadoFinal = dto.Estado;
        if (dto.Stock <= 0 && estadoFinal == EstadoProducto.Disponible)
        {
            estadoFinal = EstadoProducto.Agotado;
        }

        var producto = new Producto
        {
            Nombre = dto.Nombre.Trim(),
            Descripcion = dto.Descripcion?.Trim(),
            Precio = dto.Precio,
            Stock = dto.Stock,
            StockMinimo = dto.StockMinimo,
            ImagenUrl = dto.ImagenUrl?.Trim(),
            CategoriaId = dto.CategoriaId,
            Estado = estadoFinal,
            FechaCreacion = DateTime.UtcNow
        };

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        producto.Categoria = categoria;
        return MapToDto(producto);
    }

    public async Task<ProductoResponseDto> ActualizarAsync(int id, ActualizarProductoDto dto)
    {
        var producto = await _context.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (producto == null)
        {
            throw new NotFoundException(nameof(Producto), id);
        }

        var categoria = await _context.Categorias.FindAsync(dto.CategoriaId);
        if (categoria == null || categoria.Estado != EstadoCategoria.Activo)
        {
            throw new BadRequestException("La categoría especificada no existe o se encuentra inactiva.");
        }

        producto.Nombre = dto.Nombre.Trim();
        producto.Descripcion = dto.Descripcion?.Trim();
        producto.Precio = dto.Precio;
        producto.Stock = dto.Stock;
        producto.StockMinimo = dto.StockMinimo;
        producto.ImagenUrl = dto.ImagenUrl?.Trim();
        producto.CategoriaId = dto.CategoriaId;

        if (dto.Stock <= 0 && dto.Estado == EstadoProducto.Disponible)
        {
            producto.Estado = EstadoProducto.Agotado;
        }
        else
        {
            producto.Estado = dto.Estado;
        }

        await _context.SaveChangesAsync();
        producto.Categoria = categoria;

        return MapToDto(producto);
    }

    public async Task EliminarAsync(int id)
    {
        var producto = await _context.Productos
            .Include(p => p.Detalles)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (producto == null)
        {
            throw new NotFoundException(nameof(Producto), id);
        }

        if (producto.Detalles.Any())
        {
            // Baja lógica para mantener integridad en históricos de pedidos
            producto.Estado = EstadoProducto.Inactivo;
            await _context.SaveChangesAsync();
        }
        else
        {
            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<ProductoResponseDto> AjustarStockAsync(int id, AjustarStockDto dto)
    {
        var producto = await _context.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (producto == null)
        {
            throw new NotFoundException(nameof(Producto), id);
        }

        producto.Stock = dto.NuevoStock;

        if (producto.Stock == 0 && producto.Estado == EstadoProducto.Disponible)
        {
            producto.Estado = EstadoProducto.Agotado;
        }
        else if (producto.Stock > 0 && producto.Estado == EstadoProducto.Agotado)
        {
            producto.Estado = EstadoProducto.Disponible;
        }

        await _context.SaveChangesAsync();
        return MapToDto(producto);
    }

    private static ProductoResponseDto MapToDto(Producto p)
    {
        return new ProductoResponseDto
        {
            Id = p.Id,
            Nombre = p.Nombre,
            Descripcion = p.Descripcion,
            Precio = p.Precio,
            Stock = p.Stock,
            StockMinimo = p.StockMinimo,
            ImagenUrl = p.ImagenUrl,
            CategoriaId = p.CategoriaId,
            CategoriaNombre = p.Categoria?.Nombre ?? string.Empty,
            Estado = p.Estado,
            FechaCreacion = p.FechaCreacion
        };
    }
}
