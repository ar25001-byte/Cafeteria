using System.ComponentModel.DataAnnotations;
using Cafeteria.Domain.Enums;

namespace Cafeteria.Application.DTOs.Auth;

public class LoginRequestDto
{
    [Required(ErrorMessage = "El nombre de usuario o email es requerido.")]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida.")]
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequestDto
{
    [Required(ErrorMessage = "El nombre completo es requerido.")]
    [StringLength(100, MinimumLength = 3)]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre de usuario es requerido.")]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es requerido.")]
    [EmailAddress(ErrorMessage = "El formato de email no es válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    public string Password { get; set; } = string.Empty;

    public RolUsuario Rol { get; set; } = RolUsuario.Empleado;
}

public class UsuarioDto
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public bool Activo { get; set; }
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiracion { get; set; }
    public UsuarioDto Usuario { get; set; } = null!;
}
