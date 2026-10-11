using Cafeteria.Application.Common;
using Cafeteria.Application.DTOs.Categorias;
using Cafeteria.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cafeteria.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriasController : ControllerBase
{
    private readonly ICategoriaService _categoriaService;

    public CategoriasController(ICategoriaService categoriaService)
    {
        _categoriaService = categoriaService;
    }

    /// <summary>
    /// Obtiene la lista de todas las categorías activas.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoriaResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerActivas()
    {
        var resultado = await _categoriaService.ObtenerActivasAsync();
        return Ok(ApiResponse<IEnumerable<CategoriaResponseDto>>.Ok(resultado, "Categorías obtenidas correctamente."));
    }

    /// <summary>
    /// Obtiene una categoría por su identificador único.
    /// </summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<CategoriaResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var resultado = await _categoriaService.ObtenerPorIdAsync(id);
        return Ok(ApiResponse<CategoriaResponseDto>.Ok(resultado));
    }

    /// <summary>
    /// Crea una nueva categoría (Requiere rol Admin).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<CategoriaResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear([FromBody] CrearCategoriaDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fallo("Datos inválidos", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
        }

        var creada = await _categoriaService.CrearAsync(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, ApiResponse<CategoriaResponseDto>.Ok(creada, "Categoría creada con éxito."));
    }

    /// <summary>
    /// Actualiza los datos de una categoría existente (Requiere rol Admin).
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<CategoriaResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarCategoriaDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fallo("Datos inválidos", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
        }

        var actualizada = await _categoriaService.ActualizarAsync(id, dto);
        return Ok(ApiResponse<CategoriaResponseDto>.Ok(actualizada, "Categoría actualizada correctamente."));
    }

    /// <summary>
    /// Elimina una categoría o la desactiva si contiene productos asociados (Requiere rol Admin).
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _categoriaService.EliminarAsync(id);
        return Ok(ApiResponse<object>.Ok(new { Id = id }, "Categoría eliminada o desactivada correctamente."));
    }
}
