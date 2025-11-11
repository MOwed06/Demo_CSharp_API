using BigBooks.API.Interfaces;
using BigBooks.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BigBooks.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
        [Authorize(Policy = "AccountAccess")]
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
        /// Purchase designated books for logged in user
        /// </summary>
        /// <remarks>
        /// Purchase is made for logged in user based on claims
        /// Purchase is refused if stock unavailable or user wallet is insufficient
        /// </remarks>
        /// <param name="dto">book key and quantity</param>
        /// <returns>updated user info</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<UserDetailsDto> PurchaseBooks(BookPurchaseDto dto)
        {
            logger.LogTrace($"PurchaseBooks, book: {dto.BookKey}, qty: {dto.RequestedQuantity}");

            try
            {
                var currentUserKeyValue = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

                var response = userProvider.PurchaseBooks(currentUserKeyValue, dto);

                if (response.Key == null)
                {
                    logger.LogError(response.Error);
                    return BadRequest();
                }

                var updatedUserDto = userProvider.GetUser(response.Key.Value);
                return Ok(updatedUserDto);
                
            }
            catch (Exception ex)
            {
                logger.LogCritical($"PurchaseBooks, book: {dto.BookKey}, qty: {dto.RequestedQuantity}", ex);
                return BadRequest();
            }
        }
    }
}
