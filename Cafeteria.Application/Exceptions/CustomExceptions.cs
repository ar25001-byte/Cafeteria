namespace Cafeteria.Application.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string entityName, object key) : base($"La entidad '{entityName}' con identificador ({key}) no fue encontrada.") { }
}

public class BadRequestException : Exception
{
    public List<string> Errors { get; } = new();

    public BadRequestException(string message) : base(message)
    {
        Errors.Add(message);
    }

    public BadRequestException(string message, IEnumerable<string> errors) : base(message)
    {
        Errors = errors.ToList();
    }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}
