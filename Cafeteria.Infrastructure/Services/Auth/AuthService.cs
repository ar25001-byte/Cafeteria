using Cafeteria.Application.DTOs.Auth;
using Cafeteria.Application.Exceptions;
using Cafeteria.Application.Interfaces;
using Cafeteria.Domain.Entities;
using Cafeteria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cafeteria.Infrastructure.Services.Auth;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtTokenGenerator _jwtGenerator;

    public AuthService(ApplicationDbContext context, IJwtTokenGenerator jwtGenerator)
    {
        _context = context;
        _jwtGenerator = jwtGenerator;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
    {
        var input = dto.UsernameOrEmail.Trim().ToLower();

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Username.ToLower() == input || u.Email.ToLower() == input);

        if (usuario == null || !usuario.Activo)
        {
            throw new UnauthorizedException("Credenciales incorrectas o usuario inactivo.");
        }

        bool passwordValida = BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash);
        if (!passwordValida)
        {
            throw new UnauthorizedException("Credenciales incorrectas o usuario inactivo.");
        }

        var token = _jwtGenerator.GenerarToken(usuario, out var expiracion);

        return new LoginResponseDto
        {
            Token = token,
            Expiracion = expiracion,
            Usuario = new UsuarioDto
            {
                Id = usuario.Id,
                NombreCompleto = usuario.NombreCompleto,
                Username = usuario.Username,
                Email = usuario.Email,
                Rol = usuario.Rol.ToString(),
                Activo = usuario.Activo
            }
        };
    }

    public async Task<UsuarioDto> RegistrarAsync(RegisterRequestDto dto)
    {
        var usernameExists = await _context.Usuarios.AnyAsync(u => u.Username.ToLower() == dto.Username.ToLower());
        if (usernameExists)
        {
            throw new ConflictException($"El nombre de usuario '{dto.Username}' ya se encuentra registrado.");
        }

        var emailExists = await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower());
        if (emailExists)
        {
            throw new ConflictException($"El correo electrónico '{dto.Email}' ya se encuentra registrado.");
        }

        var nuevoUsuario = new Usuario
        {
            NombreCompleto = dto.NombreCompleto.Trim(),
            Username = dto.Username.Trim(),
            Email = dto.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Rol = dto.Rol,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        _context.Usuarios.Add(nuevoUsuario);
        await _context.SaveChangesAsync();

        return new UsuarioDto
        {
            Id = nuevoUsuario.Id,
            NombreCompleto = nuevoUsuario.NombreCompleto,
            Username = nuevoUsuario.Username,
            Email = nuevoUsuario.Email,
            Rol = nuevoUsuario.Rol.ToString(),
            Activo = nuevoUsuario.Activo
        };
    }
}
