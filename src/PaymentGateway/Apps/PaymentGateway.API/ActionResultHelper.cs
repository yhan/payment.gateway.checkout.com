using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Domain;

namespace PaymentGateway.API
{
    //[ExcludeFromCodeCoverage]
    public static class ActionResultHelper
    {
        public static ActionResult ToActionResult(InvalidCommandResult invalidCommandResult)
        {
            var problemDetails = new ProblemDetails
            {
                Type= "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid request",
                Detail = invalidCommandResult.Reason
            };
            return new BadRequestObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json", "application/problem+xml" }
            };
        }

        public static ActionResult ToActionResult(EntityNotFoundCommandResult entityNotFoundCommandResult)
        {
            var problemDetails = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Status = StatusCodes.Status404NotFound,
                Title = "Not found",
                Detail = entityNotFoundCommandResult.Reason
            };
            return new NotFoundObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json", "application/problem+xml" }
            };
        }

        public static ActionResult ToActionResult(EntityConflictCommandResult entityConflictCommandResult)
        {
            var problemDetails = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = entityConflictCommandResult.Reason
            };
            return new ConflictObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json", "application/problem+xml" }
            };
        }

        public static ActionResult ToActionResult(FailureCommandResult failureCommandResult)
        {
            var problemDetails = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Status = StatusCodes.Status500InternalServerError,
                Title = "Error",
                Detail = failureCommandResult.Reason
            };
            return new ObjectResult(problemDetails)
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                ContentTypes = { "application/problem+json", "application/problem+xml" }
            };
        }

        public static ActionResult BuildProblemDetailsResult(int statusCode, string title, string type = null, string detail = null, string instance = null)
        {
            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Type = type,
                Detail = detail,
                Instance = instance
            };
            return new ObjectResult(problemDetails)
            {
                StatusCode = statusCode,
                ContentTypes = { "application/problem+json", "application/problem+xml" }
            };
        }
    }
}