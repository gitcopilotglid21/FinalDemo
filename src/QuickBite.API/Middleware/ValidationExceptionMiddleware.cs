using FluentValidation;
using QuickBite.API.DTOs;
using System.Net;
using System.Text.Json;

namespace QuickBite.API.Middleware
{
    /// <summary>
    /// Middleware to handle validation exceptions and return standardized error responses
    /// Following BRD error response format specifications
    /// </summary>
    public class ValidationExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ValidationExceptionMiddleware> _logger;

        public ValidationExceptionMiddleware(RequestDelegate next, ILogger<ValidationExceptionMiddleware> logger)
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
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation exception occurred: {Message}", ex.Message);
                await HandleValidationExceptionAsync(context, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred: {Message}", ex.Message);
                await HandleGenericExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Handle FluentValidation exceptions
        /// </summary>
        private static async Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";
            response.StatusCode = (int)HttpStatusCode.BadRequest;

            var errorResponse = new ErrorResponse
            {
                Error = new ErrorDetails
                {
                    Code = "VALIDATION_ERROR",
                    Message = "One or more validation errors occurred",
                    Details = string.Join("; ", exception.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")),
                    Timestamp = DateTime.UtcNow
                }
            };

            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await response.WriteAsync(jsonResponse);
        }

        /// <summary>
        /// Handle generic exceptions
        /// </summary>
        private static async Task HandleGenericExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";
            response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var errorResponse = new ErrorResponse
            {
                Error = new ErrorDetails
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An internal server error occurred",
                    Details = "Please contact support if the problem persists",
                    Timestamp = DateTime.UtcNow
                }
            };

            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await response.WriteAsync(jsonResponse);
        }
    }
}