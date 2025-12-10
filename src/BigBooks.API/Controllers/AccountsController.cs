using BigBooks.API.Interfaces;
using BigBooks.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BigBooks.API.Controllers
{
    /// <summary>
    /// Accounts controller shares provider with Users controller
    /// Authorization is intentionally different
    /// Accounts offers Admin high-level access to all users
    /// </summary>
    /// <param name="usersProvider"></param>
    /// <param name="logger"></param>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AccountAccess")]
    public class AccountsController(IUsersProvider usersProvider,
        ILogger<AccountsController> logger) : BigBooksController(logger)
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
            var statusMsg = $"GetUserInfo, {key}";
            logger.LogTrace(statusMsg);

            try
            {
                var userDto = usersProvider.GetUser(key);

                if (userDto == null)
                {
                    return InvalidRequest(statusCode: HttpStatusCode.NotFound,
                        errorMessage: $"No User {key}");
                }

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return FailedRequest(statusMsg, ex);
            }
        }

        /// <summary>
        /// Get all user acounts
        /// </summary>
        /// <remarks>
        /// Requires authenticated user with Admin role
        /// </remarks>
        /// <param name="active">filter by active status (1/0/null)</param>
        /// <returns>overview of each user</returns>
        [HttpGet("list/{active?}", Name = "GetAccountList")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<IEnumerable<UserOverviewDto>> GetAccounts(int? active)
        {
            logger.LogTrace("GetUsers");

            try
            {
                bool? activeFilter = active.HasValue
                    ? active.Value == 1
                    : null;

                var userDtos = usersProvider.GetUsers(activeFilter);
                return Ok(userDtos);
            }
            catch (Exception ex)
            {
                return FailedRequest("GetUsers", ex);
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
            var statusMsg = $"AddUser, {dto.UserEmail}";
            logger.LogTrace(statusMsg);

            try
            {
                var response = usersProvider.AddUser(dto);

                if (!response.Key.HasValue)
                {
                    return InvalidRequest(statusCode: HttpStatusCode.BadRequest,
                        errorMessage: response.Error);
                }

                return GetAccountInfo(response.Key.Value);
            }
            catch (Exception ex)
            {
                return FailedRequest(statusMsg, ex);
            }
        }

        /// <summary>
        /// Modify user account info
        /// </summary>
        /// <remarks>
        /// Book reviews are not modified
        /// </remarks>
        /// <param name="key"></param>
        /// <param name="patchDoc"></param>
        /// <returns></returns>
        [HttpPatch("{key}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<UserDetailsDto> UpdateAccount(int key, JsonPatchDocument<UserAddUpdateDto> patchDoc)
        {
            var statusMsg = $"UpdateAccount, {key}";
            logger.LogTrace(statusMsg);

            try
            {
                var response = usersProvider.UpdateAccount(key, patchDoc);

                if (!response.Key.HasValue)
                {
                    return InvalidRequest(statusCode: HttpStatusCode.BadRequest,
                        errorMessage: response.Error);
                }

                var userInfo = usersProvider.GetUser(response.Key.Value);
                return Ok(userInfo);
            }
            catch (Exception ex)
            {
                return FailedRequest(statusMsg, ex);
            }
        }
    }
}
