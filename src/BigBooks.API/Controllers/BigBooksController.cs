using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BigBooks.API.Controllers
{
    [ApiController]
    public abstract class BigBooksController(ILogger logger) : ControllerBase
    {
        protected ActionResult InvalidRequest(HttpStatusCode statusCode, 
            string errorMessage)
        {
            logger.LogDebug(errorMessage);

            switch (statusCode)
            {
                case HttpStatusCode.NotFound:
                    return NotFound(errorMessage);

                case HttpStatusCode.Unauthorized:
                    return Unauthorized(errorMessage);
                
                default:
                    return BadRequest(errorMessage);
            }
        }

        protected ActionResult FailedRequest(string statusMsg, Exception ex)
        {
            logger.LogCritical(message: statusMsg,
                exception: ex);
            return BadRequest(ex.Message);
        }
    }
}
