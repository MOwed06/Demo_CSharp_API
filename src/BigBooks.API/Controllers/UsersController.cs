using BigBooks.API.Interfaces;
using BigBooks.API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BigBooks.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IUsersProvider usersProvider, ILogger<UsersController> logger) : ControllerBase
    {
        /// <summary>
        /// Get UserDetails for current user
        /// </summary>
        /// <remarks>
        /// Current user identified via token
        /// </remarks>
        /// <returns>user details</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<UserDetailsDto> GetCurrentUser()
        {
            logger.LogTrace("GetCurrentUser");

            try
            {
                // extract appUser key from active user claims
                var currentUserKeyValue = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

                var userDto = usersProvider.GetCurrentUser(currentUserKeyValue);
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                logger.LogCritical("GetCurrentUser", ex);
                return BadRequest();
            }
        }
    }
}
