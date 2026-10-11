using Cafeteria.Application.Common;
using Cafeteria.Application.DTOs.Pedidos;
using Cafeteria.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cafeteria.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PedidosController : ControllerBase
{
    private readonly IPedidoService _pedidoService;

    public PedidosController(IPedidoService pedidoService)
    {
        _pedidoService = pedidoService;
    }

    /// <summary>
    /// Consulta pedidos con filtros avanzados (rango de fechas, cliente, estado) y paginación.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PedidoResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTodos([FromQuery] PedidoFilterParams filtros)
    {
        var resultado = await _pedidoService.ObtenerTodosAsync(filtros);
        return Ok(ApiResponse<PagedResult<PedidoResponseDto>>.Ok(resultado, "Pedidos consultados exitosamente."));
    }

    /// <summary>
    /// Obtiene el detalle completo de un pedido por su ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PedidoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var resultado = await _pedidoService.ObtenerPorIdAsync(id);
        return Ok(ApiResponse<PedidoResponseDto>.Ok(resultado));
    }

    /// <summary>
    /// Crea un nuevo pedido, valida y descuenta el stock de inmediato y calcula montos en servidor.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PedidoResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Crear([FromBody] CrearPedidoDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fallo("Datos inválidos", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
        }

        var nuevoPedido = await _pedidoService.CrearAsync(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevoPedido.Id }, ApiResponse<PedidoResponseDto>.Ok(nuevoPedido, "Pedido registrado exitosamente."));
    }

    /// <summary>
    /// Realiza una transición controlada del estado del pedido (Si se cancela, restaura stock).
    /// </summary>
    [HttpPut("{id:int}/estado")]
    [ProducesResponseType(typeof(ApiResponse<PedidoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoPedidoDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fallo("Datos inválidos", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
        }

        var pedidoActualizado = await _pedidoService.CambiarEstadoAsync(id, dto);
        return Ok(ApiResponse<PedidoResponseDto>.Ok(pedidoActualizado, $"Estado del pedido actualizado a {dto.NuevoEstado}."));
    }

    /// <summary>
    /// Elimina un pedido y restaura el stock si no estaba cancelado (Requiere rol Admin).
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _pedidoService.EliminarAsync(id);
        return Ok(ApiResponse<object>.Ok(new { Id = id }, "Pedido eliminado correctamente."));
    }
}
