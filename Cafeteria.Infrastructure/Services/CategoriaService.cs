using Cafeteria.Application.DTOs.Categorias;
using Cafeteria.Application.Exceptions;
using Cafeteria.Application.Interfaces;
using Cafeteria.Domain.Entities;
using Cafeteria.Domain.Enums;
using Cafeteria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cafeteria.Infrastructure.Services;

public class CategoriaService : ICategoriaService
{
    private readonly ApplicationDbContext _context;

    public CategoriaService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CategoriaResponseDto>> ObtenerActivasAsync()
    {
        return await _context.Categorias
            .Where(c => c.Estado == EstadoCategoria.Activo)
            .OrderBy(c => c.Nombre)
            .Select(c => new CategoriaResponseDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion,
                Estado = c.Estado,
                FechaCreacion = c.FechaCreacion,
                TotalProductos = c.Productos.Count(p => p.Estado != EstadoProducto.Inactivo)
            })
            .ToListAsync();
    }

    public async Task<CategoriaResponseDto> ObtenerPorIdAsync(int id)
    {
        var categoria = await _context.Categorias
            .Include(c => c.Productos)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (categoria == null)
        {
            throw new NotFoundException(nameof(Categoria), id);
        }

        return new CategoriaResponseDto
        {
            Id = categoria.Id,
            Nombre = categoria.Nombre,
            Descripcion = categoria.Descripcion,
            Estado = categoria.Estado,
            FechaCreacion = categoria.FechaCreacion,
            TotalProductos = categoria.Productos.Count(p => p.Estado != EstadoProducto.Inactivo)
        };
    }

    public async Task<CategoriaResponseDto> CrearAsync(CrearCategoriaDto dto)
    {
        var existeNombre = await _context.Categorias
            .AnyAsync(c => c.Nombre.ToLower() == dto.Nombre.Trim().ToLower());

        if (existeNombre)
        {
            throw new ConflictException($"Ya existe una categoría con el nombre '{dto.Nombre}'.");
        }

        var categoria = new Categoria
        {
            Nombre = dto.Nombre.Trim(),
            Descripcion = dto.Descripcion?.Trim(),
            Estado = dto.Estado,
            FechaCreacion = DateTime.UtcNow
        };

        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        return new CategoriaResponseDto
        {
            Id = categoria.Id,
            Nombre = categoria.Nombre,
            Descripcion = categoria.Descripcion,
            Estado = categoria.Estado,
            FechaCreacion = categoria.FechaCreacion,
            TotalProductos = 0
        };
    }

    public async Task<CategoriaResponseDto> ActualizarAsync(int id, ActualizarCategoriaDto dto)
    {
        var categoria = await _context.Categorias
            .Include(c => c.Productos)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (categoria == null)
        {
            throw new NotFoundException(nameof(Categoria), id);
        }

        var nombreEnUso = await _context.Categorias
            .AnyAsync(c => c.Id != id && c.Nombre.ToLower() == dto.Nombre.Trim().ToLower());

        if (nombreEnUso)
        {
            throw new ConflictException($"Ya existe otra categoría con el nombre '{dto.Nombre}'.");
        }

        categoria.Nombre = dto.Nombre.Trim();
        categoria.Descripcion = dto.Descripcion?.Trim();
        categoria.Estado = dto.Estado;

        await _context.SaveChangesAsync();

        return new CategoriaResponseDto
        {
            Id = categoria.Id,
            Nombre = categoria.Nombre,
            Descripcion = categoria.Descripcion,
            Estado = categoria.Estado,
            FechaCreacion = categoria.FechaCreacion,
            TotalProductos = categoria.Productos.Count(p => p.Estado != EstadoProducto.Inactivo)
        };
    }

    public async Task EliminarAsync(int id)
    {
        var categoria = await _context.Categorias
            .Include(c => c.Productos)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (categoria == null)
        {
            throw new NotFoundException(nameof(Categoria), id);
        }

        var tieneProductosAsociados = categoria.Productos.Any();

        if (tieneProductosAsociados)
        {
            // Eliminación lógica para preservar integridad referencial
            categoria.Estado = EstadoCategoria.Inactivo;
            await _context.SaveChangesAsync();
        }
        else
        {
            // Eliminación física si no tiene dependencias
            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();
        }
    }
}
