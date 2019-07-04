using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Domain;

namespace PaymentGateway
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
                Detail = invalidCommandResult.Reason,

            };
            return new BadRequestObjectResult(problemDetails)
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
    }
}