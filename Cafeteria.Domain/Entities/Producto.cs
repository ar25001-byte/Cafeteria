using Cafeteria.Domain.Enums;

namespace Cafeteria.Domain.Entities;

public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public int StockMinimo { get; set; }
    public string? ImagenUrl { get; set; }
    public int CategoriaId { get; set; }
    public EstadoProducto Estado { get; set; } = EstadoProducto.Disponible;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Relaciones
    public Categoria? Categoria { get; set; }
    public ICollection<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
}
