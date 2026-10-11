namespace Cafeteria.Application.Common;

public class PagedResult<T>
{
    public IEnumerable<T> Elementos { get; set; } = new List<T>();
    public int PaginaActual { get; set; }
    public int TamanoPagina { get; set; }
    public int TotalElementos { get; set; }
    public int TotalPaginas { get; set; }
    public bool TienePaginaAnterior => PaginaActual > 1;
    public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;

    public PagedResult(IEnumerable<T> elementos, int totalElementos, int paginaActual, int tamanoPagina)
    {
        Elementos = elementos;
        TotalElementos = totalElementos;
        PaginaActual = paginaActual;
        TamanoPagina = tamanoPagina;
        TotalPaginas = (int)Math.Ceiling(totalElementos / (double)tamanoPagina);
    }

    public PagedResult() { }
}

public class PaginationParams
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;

    public int Pagina { get; set; } = 1;

    public int TamanoPagina
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : (value < 1 ? 1 : value);
    }

    public string? Busqueda { get; set; }
}
