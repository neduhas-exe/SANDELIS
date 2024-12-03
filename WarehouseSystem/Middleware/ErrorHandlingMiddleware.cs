using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace WarehouseSystem.Middleware
{
    /// <summary>
    /// Middleware skirtas centralizuotam klaidų valdymui
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(
            RequestDelegate next,
            ILogger<ErrorHandlingMiddleware> logger)
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
            catch (Exception error)
            {
                var response = context.Response;
                response.ContentType = "application/json";

                var errorResponse = new ErrorResponse
                {
                    TraceId = context.TraceIdentifier
                };

                switch (error)
                {
                    case ValidationException e:
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        errorResponse.Message = "Validacijos klaida";
                        errorResponse.Details = e.Errors;
                        _logger.LogWarning(e, "Validacijos klaida: {Message}", e.Message);
                        break;

                    case NotFoundException e:
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        errorResponse.Message = e.Message;
                        _logger.LogWarning(e, "Resursas nerastas: {Message}", e.Message);
                        break;

                    case UnauthorizedException e:
                        response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        errorResponse.Message = "Neautorizuota prieiga";
                        _logger.LogWarning(e, "Neautorizuota prieiga: {Message}", e.Message);
                        break;

                    case BusinessRuleException e:
                        response.StatusCode = (int)HttpStatusCode.Conflict;
                        errorResponse.Message = e.Message;
                        errorResponse.Details = e.Details;
                        _logger.LogWarning(e, "Verslo taisyklių klaida: {Message}", e.Message);
                        break;

                    case ConcurrencyException e:
                        response.StatusCode = (int)HttpStatusCode.Conflict;
                        errorResponse.Message = "Duomenų konfliktas";
                        _logger.LogWarning(e, "Konkurencijos klaida: {Message}", e.Message);
                        break;

                    default:
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        errorResponse.Message = "Įvyko vidinė serverio klaida";
                        _logger.LogError(error, "Neapdorota klaida");
                        break;
                }

                var result = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                });
                
                await response.WriteAsync(result);
            }
        }
    }

    /// <summary>
    /// Pagalbinė klasė klaidų atsakymui
    /// </summary>
    public class ErrorResponse
    {
        public string TraceId { get; set; }
        public string Message { get; set; }
        public object Details { get; set; }
    }

    /// <summary>
    /// Išimties klasės
    /// </summary>
    public class ValidationException : Exception
    {
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException(IDictionary<string, string[]> errors)
            : base("Validacijos klaida")
        {
            Errors = errors;
        }
    }

    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message)
        {
        }
    }

    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message)
        {
        }
    }

    public class BusinessRuleException : Exception
    {
        public object Details { get; }

        public BusinessRuleException(string message, object details = null) 
            : base(message)
        {
            Details = details;
        }
    }

    public class ConcurrencyException : Exception
    {
        public ConcurrencyException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Middleware plėtinys registracijai
    /// </summary>
    public static class ErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandling(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}
