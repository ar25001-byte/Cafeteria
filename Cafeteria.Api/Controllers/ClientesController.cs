using Cafeteria.Application.Common;
using Cafeteria.Application.DTOs.Clientes;
using Cafeteria.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cafeteria.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;

    public ClientesController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    /// <summary>
    /// Obtiene la lista paginada de clientes con soporte para búsqueda por nombre o email.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ClienteResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTodos([FromQuery] PaginationParams filtros)
    {
        var resultado = await _clienteService.ObtenerTodosAsync(filtros);
        return Ok(ApiResponse<PagedResult<ClienteResponseDto>>.Ok(resultado, "Clientes obtenidos exitosamente."));
    }

    /// <summary>
    /// Obtiene los detalles de un cliente específico por su ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ClienteResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var resultado = await _clienteService.ObtenerPorIdAsync(id);
        return Ok(ApiResponse<ClienteResponseDto>.Ok(resultado));
    }

    /// <summary>
    /// Registra un nuevo cliente en el sistema.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ClienteResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear([FromBody] CrearClienteDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fallo("Datos inválidos", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
        }

        var nuevo = await _clienteService.CrearAsync(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevo.Id }, ApiResponse<ClienteResponseDto>.Ok(nuevo, "Cliente registrado exitosamente."));
    }

    /// <summary>
    /// Actualiza la información de un cliente existente.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ClienteResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarClienteDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fallo("Datos inválidos", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
        }

        var actualizado = await _clienteService.ActualizarAsync(id, dto);
        return Ok(ApiResponse<ClienteResponseDto>.Ok(actualizado, "Cliente actualizado exitosamente."));
    }

    /// <summary>
    /// Elimina un cliente si no tiene pedidos históricos (Requiere rol Admin).
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _clienteService.EliminarAsync(id);
        return Ok(ApiResponse<object>.Ok(new { Id = id }, "Cliente eliminado correctamente."));
    }

    /// <summary>
    /// Consulta el historial paginado de compras y pedidos realizados por el cliente.
    /// </summary>
    [HttpGet("{id:int}/historial-compras")]
    [ProducesResponseType(typeof(ApiResponse<ClienteHistorialDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerHistorialCompras(int id, [FromQuery] PaginationParams filtros)
    {
        var historial = await _clienteService.ObtenerHistorialComprasAsync(id, filtros);
        return Ok(ApiResponse<ClienteHistorialDto>.Ok(historial, "Historial de compras obtenido exitosamente."));
    }
}
