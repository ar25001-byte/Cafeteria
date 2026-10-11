using Cafeteria.Application.Common;
using Cafeteria.Application.DTOs.Productos;
using Cafeteria.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cafeteria.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductosController : ControllerBase
{
    private readonly IProductoService _productoService;

    public ProductosController(IProductoService productoService)
    {
        _productoService = productoService;
    }

    /// <summary>
    /// Lista productos con paginación, filtros de búsqueda, categoría, disponibilidad y ordenación por precio.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductoResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTodos([FromQuery] ProductoFilterParams filtros)
    {
        var resultado = await _productoService.ObtenerTodosAsync(filtros);
        return Ok(ApiResponse<PagedResult<ProductoResponseDto>>.Ok(resultado, "Productos obtenidos exitosamente."));
    }

    /// <summary>
    /// Obtiene un producto por su ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ProductoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var resultado = await _productoService.ObtenerPorIdAsync(id);
        return Ok(ApiResponse<ProductoResponseDto>.Ok(resultado));
    }

    /// <summary>
    /// Crea un nuevo producto (Requiere rol Admin).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<ProductoResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] CrearProductoDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fallo("Datos inválidos", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
        }

        var nuevo = await _productoService.CrearAsync(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevo.Id }, ApiResponse<ProductoResponseDto>.Ok(nuevo, "Producto creado exitosamente."));
    }

    /// <summary>
    /// Actualiza un producto existente (Requiere rol Admin).
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<ProductoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarProductoDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fallo("Datos inválidos", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
        }

        var actualizado = await _productoService.ActualizarAsync(id, dto);
        return Ok(ApiResponse<ProductoResponseDto>.Ok(actualizado, "Producto actualizado correctamente."));
    }

    /// <summary>
    /// Elimina un producto o lo desactiva si tiene pedidos asociados (Requiere rol Admin).
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _productoService.EliminarAsync(id);
        return Ok(ApiResponse<object>.Ok(new { Id = id }, "Producto eliminado o dado de baja correctamente."));
    }

    /// <summary>
    /// Ajuste rápido de stock de inventario (Requiere rol Admin o Empleado).
    /// </summary>
    [HttpPatch("{id:int}/stock")]
    [Authorize(Roles = "Admin,Empleado")]
    [ProducesResponseType(typeof(ApiResponse<ProductoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AjustarStock(int id, [FromBody] AjustarStockDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fallo("Datos inválidos", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
        }

        var actualizado = await _productoService.AjustarStockAsync(id, dto);
        return Ok(ApiResponse<ProductoResponseDto>.Ok(actualizado, $"Stock ajustado correctamente a {dto.NuevoStock} unidades."));
    }
}
