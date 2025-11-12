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
        /// Get user info by key.
        /// </summary>
        /// <remarks>
        /// Requires authenticated user with Admin role
        /// </remarks>
        /// <param name="key">user account key</param>
        /// <returns>user details</returns>
        [HttpGet("{key}", Name = "GetUserInfo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<UserDetailsDto> GetUserInfo(int key)
        {
            logger.LogTrace($"GetUserInfo, {key}");

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
                logger.LogCritical($"GetUserInfo, {key}", ex);
                return BadRequest();
            }
        }

        /// <summary>
        /// Get all users
        /// </summary>
        /// <remarks>
        /// Requires authenticated user with Admin role
        /// </remarks>
        /// <returns>overview of each user</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<IEnumerable<UserOverviewDto>> GetUsers()
        {
            logger.LogTrace("GetUsers");

            try
            {
                var userDtos = userProvider.GetUsers();
                return Ok(userDtos);
            }
            catch (Exception ex)
            {
                logger.LogCritical("GetUsers", ex);
                return BadRequest();
            }
        }

        /// <summary>
        /// Add new user
        /// </summary>
        /// <remarks>
        /// Requires authenticated user with Admin role
        /// </remarks>         
        /// <param name="dto">user parameters</param>
        /// <returns>user details</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<UserDetailsDto> AddUser(UserAddUpdateDto dto)
        {
            logger.LogTrace($"AddUser, {dto.UserEmail}");

            try
            {
                var response = userProvider.AddUser(dto);

                if (response.Key == null)
                {
                    throw new Exception(response.Error);
                }

                return GetUserInfo(response.Key.Value);
            }
            catch (Exception ex)
            {
                logger.LogError($"AddUser, {dto.UserEmail}", ex);
                return BadRequest();
            }
        }
    }
}
