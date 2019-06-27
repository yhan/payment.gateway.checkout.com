using System.Net;
using Microsoft.AspNetCore.Mvc;
using NFluent;

namespace PaymentGateway.Tests
{
    public static class HttpResponseExtensions
    {
        public static void IsOk(this IActionResult response)
        {
            Check.That(response).IsInstanceOf<ObjectResult>();
            Check.That(((ObjectResult) response).StatusCode).IsEqualTo((int) HttpStatusCode.OK);
        }
    }
}