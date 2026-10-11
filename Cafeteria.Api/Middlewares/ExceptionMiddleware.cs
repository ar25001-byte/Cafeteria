using System.Net;
using System.Text.Json;
using Cafeteria.Application.Common;
using Cafeteria.Application.Exceptions;

namespace Cafeteria.Api.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocurrió un error inesperado al procesar la solicitud: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var statusCode = HttpStatusCode.InternalServerError;
        var mensaje = "Ha ocurrido un error interno en el servidor.";
        List<string>? errores = null;

        switch (exception)
        {
            case NotFoundException notFoundEx:
                statusCode = HttpStatusCode.NotFound;
                mensaje = notFoundEx.Message;
                break;

            case BadRequestException badRequestEx:
                statusCode = HttpStatusCode.BadRequest;
                mensaje = badRequestEx.Message;
                errores = badRequestEx.Errors;
                break;

            case ConflictException conflictEx:
                statusCode = HttpStatusCode.Conflict;
                mensaje = conflictEx.Message;
                break;

            case UnauthorizedException unauthEx:
                statusCode = HttpStatusCode.Unauthorized;
                mensaje = unauthEx.Message;
                break;

            case KeyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                mensaje = "El recurso solicitado no fue encontrado.";
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                errores = new List<string> { exception.Message };
                break;
        }

        context.Response.StatusCode = (int)statusCode;

        var respuesta = ApiResponse<object>.Fallo(mensaje, errores);
        var json = JsonSerializer.Serialize(respuesta, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
