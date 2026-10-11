using System.ComponentModel.DataAnnotations;
using Cafeteria.Application.Common;
using Cafeteria.Domain.Enums;

namespace Cafeteria.Application.DTOs.Productos;

public class CrearProductoDto
{
    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres.")]
    public string? Descripcion { get; set; }

    [Range(0.01, 100000.00, ErrorMessage = "El precio debe ser mayor a 0.")]
    public decimal Precio { get; set; }

    [Range(0, 1000000, ErrorMessage = "El stock inicial no puede ser negativo.")]
    public int Stock { get; set; }

    [Range(0, 100000, ErrorMessage = "El stock mínimo no puede ser negativo.")]
    public int StockMinimo { get; set; } = 5;

    [Url(ErrorMessage = "La URL de la imagen debe ser válida.")]
    public string? ImagenUrl { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe especificar una categoría válida.")]
    public int CategoriaId { get; set; }

    public EstadoProducto Estado { get; set; } = EstadoProducto.Disponible;
}

public class ActualizarProductoDto
{
    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [StringLength(150, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Descripcion { get; set; }

    [Range(0.01, 100000.00, ErrorMessage = "El precio debe ser mayor a 0.")]
    public decimal Precio { get; set; }

    [Range(0, 1000000, ErrorMessage = "El stock no puede ser negativo.")]
    public int Stock { get; set; }

    [Range(0, 100000, ErrorMessage = "El stock mínimo no puede ser negativo.")]
    public int StockMinimo { get; set; }

    [Url(ErrorMessage = "La URL de la imagen debe ser válida.")]
    public string? ImagenUrl { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe especificar una categoría válida.")]
    public int CategoriaId { get; set; }

    public EstadoProducto Estado { get; set; }
}

public class AjustarStockDto
{
    [Range(0, 1000000, ErrorMessage = "El stock no puede ser un valor negativo.")]
    public int NuevoStock { get; set; }

    public string? Motivo { get; set; }
}

public class ProductoResponseDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public int StockMinimo { get; set; }
    public bool StockBajo => Stock <= StockMinimo;
    public string? ImagenUrl { get; set; }
    public int CategoriaId { get; set; }
    public string CategoriaNombre { get; set; } = string.Empty;
    public EstadoProducto Estado { get; set; }
    public string EstadoNombre => Estado.ToString();
    public DateTime FechaCreacion { get; set; }
}

public class ProductoFilterParams : PaginationParams
{
    public int? CategoriaId { get; set; }
    public EstadoProducto? Estado { get; set; }
    public bool? SoloDisponibles { get; set; }
    public string? OrdenarPrecio { get; set; } // "asc" | "desc"
}
