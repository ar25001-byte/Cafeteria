namespace Cafeteria.Domain.Entities;

public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    // Relaciones
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
