using BigBooks.API.Authentication;
using BigBooks.API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BigBooks.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController(IAuthService authService,
        ILogger<AuthenticationController> logger) : BigBooksController(logger)
    {
        /// <summary>
        /// Request token for user.
        /// </summary>
        /// <remarks>
        /// Example customer user: "Bella.Barnes@demo.com", "N0tV3ryS3cret"; 
        /// admin user: "Clark.Kent@demo.com", "N0tV3ryS3cret"
        /// </remarks>
        /// <param name="authRequest"></param>
        /// <returns>authentication response</returns>
        [HttpPost("authenticate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Authenticate(AuthRequest authRequest)
        {
            var statusMsg = $"Authenticate, {authRequest.UserId}";
            logger.LogTrace(statusMsg);

            try
            {
                var response = authService.GenerateToken(authRequest);

                if (response.Token == null)
                {
                    return InvalidRequest(statusCode: HttpStatusCode.Unauthorized,
                        errorMessage: response.Error);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return FailedRequest(statusMsg, ex);
            }
        }
    }
}
