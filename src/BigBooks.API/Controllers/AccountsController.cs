using BigBooks.API.Interfaces;
using BigBooks.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BigBooks.API.Controllers
{
    /// <summary>
    /// Accounts controller shares provider with Users controller
    /// Authorization is intentionally different
    /// Accounts offers Admin high-level access to all users
    /// </summary>
    /// <param name="users"></param>
    /// <param name="logger"></param>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AccountAccess")]
    public class AccountsController(IUsersProvider usersProvider, ILogger<AccountsController> logger) : ControllerBase
    {
        /// <summary>
        /// Get user account info by key.
        /// </summary>
        /// <remarks>
        /// Requires authenticated user with Admin role
        /// </remarks>
        /// <param name="key">user account key</param>
        /// <returns>user details</returns>
        [HttpGet("{key}", Name = "GetAccountDetails")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<UserDetailsDto> GetAccountInfo(int key)
        {
            logger.LogTrace($"GetUserInfo, {key}");

            try
            {
                var userDto = usersProvider.GetUser(key);

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
        /// Get all user acounts
        /// </summary>
        /// <remarks>
        /// Requires authenticated user with Admin role
        /// </remarks>
        /// <returns>overview of each user</returns>
        [HttpGet(Name = "GetAccountList")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<IEnumerable<UserOverviewDto>> GetAccounts()
        {
            logger.LogTrace("GetUsers");

            try
            {
                var userDtos = usersProvider.GetUsers();
                return Ok(userDtos);
            }
            catch (Exception ex)
            {
                logger.LogCritical("GetUsers", ex);
                return BadRequest();
            }
        }

        /// <summary>
        /// Add new user account
        /// </summary>
        /// <remarks>
        /// Requires authenticated user with Admin role
        /// </remarks>         
        /// <param name="dto">user parameters</param>
        /// <returns>user details</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<UserDetailsDto> AddAccount(UserAddUpdateDto dto)
        {
            logger.LogTrace($"AddUser, {dto.UserEmail}");

            try
            {
                var response = usersProvider.AddUser(dto);

                if (response.Key == null)
                {
                    return BadRequest(response.Error);
                }

                return GetAccountInfo(response.Key.Value);
            }
            catch (Exception ex)
            {
                logger.LogError($"AddUser, {dto.UserEmail}", ex);
                return BadRequest();
            }
        }
    }
}
