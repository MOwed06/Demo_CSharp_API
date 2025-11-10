using BigBooks.API.Authentication;
using BigBooks.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BigBooks.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController(IAuthService authService, ILogger<AuthenticationController> logger) : ControllerBase
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
            logger.LogTrace($"Authenticate, {authRequest.UserId}");

            try
            {
                var response = authService.GenerateToken(authRequest);

                if (response.Token == null)
                {
                    logger.LogDebug($"Authenticate, {authRequest.UserId}", response.Error);
                    return Unauthorized();
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogCritical($"Authenticate, {authRequest.UserId}", ex);
                return Unauthorized();
            }
        }
    }
}
