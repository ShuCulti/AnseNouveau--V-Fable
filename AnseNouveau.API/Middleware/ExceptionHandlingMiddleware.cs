using AnseNouveau.Domain.Exceptions;

namespace AnseNouveau.API.Middleware
{
    // Translates domain exceptions to HTTP status codes so controllers stay thin.
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            catch (ValidationException exception)
            {
                await WriteError(context, StatusCodes.Status400BadRequest, exception.Message);
            }
            catch (DuplicateRecordException exception)
            {
                await WriteError(context, StatusCodes.Status409Conflict, exception.Message);
            }
            catch (ForeignKeyViolationException exception)
            {
                await WriteError(context, StatusCodes.Status409Conflict, exception.Message);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unhandled exception");
                await WriteError(context, StatusCodes.Status500InternalServerError, "Something went wrong.");
            }
        }

        private static Task WriteError(HttpContext context, int statusCode, string message)
        {
            context.Response.StatusCode = statusCode;
            return context.Response.WriteAsJsonAsync(new { error = message });
        }
    }
}
