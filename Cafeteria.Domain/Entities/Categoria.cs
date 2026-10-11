using Cafeteria.Domain.Enums;

namespace Cafeteria.Domain.Entities;

public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public EstadoCategoria Estado { get; set; } = EstadoCategoria.Activo;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Relaciones
    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
