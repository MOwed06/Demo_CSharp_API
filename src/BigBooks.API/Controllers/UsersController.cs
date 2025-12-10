using BigBooks.API.Interfaces;
using BigBooks.API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BigBooks.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IUsersProvider usersProvider,
        ILogger<UsersController> logger) : BigBooksController(logger)
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
                // extract appUser email from active user claims 
                // corresponds to JwtRegisteredClaimNames.Sub value
                var currentUserKeyValue = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

                var userDto = usersProvider.GetCurrentUserDetails(currentUserKeyValue);
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return FailedRequest("GetCurrentUser", ex);
            }
        }
    }
}
