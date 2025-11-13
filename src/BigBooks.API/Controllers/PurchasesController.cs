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
    public class PurchasesController(IPurchasesProvider purchaseProvider,
        IUsersProvider userProvider,
        ILogger<PurchasesController> logger) : ControllerBase
    {
        /// <summary>
        /// Purchase books for logged in user
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
        public ActionResult<UserDetailsDto> PurchaseBooks(PurchaseRequestDto dto)
        {
            logger.LogTrace($"PurchaseBooks, book: {dto.BookKey}, qty: {dto.RequestedQuantity}");

            try
            {
                // extract appUser key from active user claims
                var currentUserKeyValue = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

                var response = purchaseProvider.PurchaseBooks(currentUserKeyValue, dto);

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
