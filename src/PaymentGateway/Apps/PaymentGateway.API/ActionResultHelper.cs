using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
    }


    public class ApiError
    {
        public int StatusCode { get; private set; }

        public string StatusDescription { get; private set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Message { get; private set; }

        public ApiError(int statusCode, string statusDescription)
        {
            this.StatusCode = statusCode;
            this.StatusDescription = statusDescription;
        }

        public ApiError(int statusCode, string statusDescription, string message)
            : this(statusCode, statusDescription)
        {
            this.Message = message;
        }
    }

    public class InternalServerError : ApiError
    {
        public InternalServerError()
            : base(500, HttpStatusCode.InternalServerError.ToString())
        {
        }


        public InternalServerError(string message)
            : base(500, HttpStatusCode.InternalServerError.ToString(), message)
        {
        }
    }
}