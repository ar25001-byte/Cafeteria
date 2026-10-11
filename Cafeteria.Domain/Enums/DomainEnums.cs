namespace Cafeteria.Domain.Enums;

public enum EstadoCategoria
{
    Inactivo = 0,
    Activo = 1
}

public enum EstadoProducto
{
    Inactivo = 0,
    Disponible = 1,
    Agotado = 2
}

public enum EstadoPedido
{
    Pendiente = 0,
    Preparando = 1,
    Listo = 2,
    Entregado = 3,
    Cancelado = 4
}

public enum RolUsuario
{
    Cajero = 1,
    Empleado = 2,
    Admin = 3
}
