using System.ComponentModel.DataAnnotations;
using Cafeteria.Domain.Enums;

namespace Cafeteria.Application.DTOs.Categorias;

public class CrearCategoriaDto
{
    [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "La descripción no puede exceder los 250 caracteres.")]
    public string? Descripcion { get; set; }

    public EstadoCategoria Estado { get; set; } = EstadoCategoria.Activo;
}

public class ActualizarCategoriaDto
{
    [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "La descripción no puede exceder los 250 caracteres.")]
    public string? Descripcion { get; set; }

    public EstadoCategoria Estado { get; set; }
}

public class CategoriaResponseDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public EstadoCategoria Estado { get; set; }
    public string EstadoNombre => Estado.ToString();
    public DateTime FechaCreacion { get; set; }
    public int TotalProductos { get; set; }
}
