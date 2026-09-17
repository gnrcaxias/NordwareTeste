using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Nordware.Application.Exceptions;

namespace Nordware.Api.ExceptionHandling;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Ocorreu uma exceção não tratada.");

        var (statusCode, title) = exception switch
        {
            ProductNotFoundException =>
                (StatusCodes.Status404NotFound,
                 "Produto não encontrado"),

            CustomerNotFoundException =>
                (StatusCodes.Status404NotFound,
                 "Cliente não encontrado"),

            ProductUnavailableException =>
                (StatusCodes.Status409Conflict,
                 "Produto indisponível"),

            ReservationNotFoundException =>
                (
                    StatusCodes.Status404NotFound,
                    "Reserva não encontrada"
                ),

            ReservationNotOwnedException =>
                (
                    StatusCodes.Status403Forbidden,
                    "Reserva não pertence ao cliente"
                ),

            ArgumentException =>
                (StatusCodes.Status400BadRequest,
                 "Requisição inválida"),

            _ =>
                (StatusCodes.Status500InternalServerError,
                 "Erro interno")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode == StatusCodes.Status500InternalServerError
                ? "Ocorreu um erro interno ao processar a requisição."
                : exception.Message,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }
}