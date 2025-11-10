using BigBooks.API.Interfaces;
using BigBooks.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BigBooks.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AccountAccess")]
    public class UserController(IUserProvider userProvider, ILogger<UserController> logger) : ControllerBase
    {
        /// <summary>
        /// Get user by key.
        /// </summary>
        /// <remarks>
        /// Requires authenticated user with Admin role
        /// </remarks>
        /// <param name="key">user account key</param>
        /// <returns>user details</returns>
        [HttpGet("{key}", Name = "GetUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<UserDetailsDto> GetUser(int key)
        {
            logger.LogTrace($"GetUser, {key}");

            try
            {
                var userDto = userProvider.GetUser(key);

                if (userDto == null)
                {
                    return NotFound($"User {key}");
                }

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                logger.LogCritical($"GetUser, {key}", ex);
                return BadRequest();
            }
        }
    }
}
