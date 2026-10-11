using Cafeteria.Application.Common;
using Cafeteria.Application.DTOs.Auth;
using Cafeteria.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cafeteria.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Inicia sesión y genera un JWT con los roles del usuario.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fallo("Datos de entrada inválidos", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
        }

        var resultado = await _authService.LoginAsync(dto);
        return Ok(ApiResponse<LoginResponseDto>.Ok(resultado, "Inicio de sesión exitoso."));
    }

    /// <summary>
    /// Registra un nuevo usuario en el sistema (Requiere rol Admin).
    /// </summary>
    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<UsuarioDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fallo("Datos de entrada inválidos", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
        }

        var resultado = await _authService.RegistrarAsync(dto);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<UsuarioDto>.Ok(resultado, "Usuario registrado exitosamente."));
    }

    /// <summary>
    /// Devuelve el perfil y claims del usuario autenticado actual.
    /// </summary>
    [HttpGet("perfil")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public IActionResult Perfil()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        return Ok(ApiResponse<object>.Ok(new
        {
            Usuario = User.Identity?.Name,
            EstaAutenticado = User.Identity?.IsAuthenticated,
            Claims = claims
        }, "Información del usuario autenticado."));
    }
}
