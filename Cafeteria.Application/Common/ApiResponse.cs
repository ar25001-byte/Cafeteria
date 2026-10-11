namespace Cafeteria.Application.Common;

public class ApiResponse<T>
{
    public bool Exito { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public T? Datos { get; set; }
    public List<string>? Errores { get; set; }

    public static ApiResponse<T> Ok(T datos, string mensaje = "Operación realizada con éxito")
    {
        return new ApiResponse<T>
        {
            Exito = true,
            Mensaje = mensaje,
            Datos = datos,
            Errores = null
        };
    }

    public static ApiResponse<T> Fallo(string mensaje, List<string>? errores = null)
    {
        return new ApiResponse<T>
        {
            Exito = false,
            Mensaje = mensaje,
            Datos = default,
            Errores = errores ?? new List<string>()
        };
    }
}
