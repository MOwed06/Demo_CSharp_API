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
    public class TransactionsController(ITransactionsProvider transactionsProvider,
        IUsersProvider userProvider,
        ILogger<TransactionsController> logger) : ControllerBase
    {
        /// <summary>
        /// Purchase books for logged in user
        /// </summary>
        /// <remarks>
        /// Purchase is made for logged in user based on token claims
        /// Purchase is refused if stock unavailable or user wallet is insufficient
        /// </remarks>
        /// <param name="dto">book key and quantity</param>
        /// <returns>updated user info</returns>
        [Route("purchase")]
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

                var response = transactionsProvider.PurchaseBooks(currentUserKeyValue, dto);

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

        /// <summary>
        /// Add funds to user wallet
        /// </summary>
        /// <remarks>
        /// Deposit is made for logged in user based on token claims
        /// </remarks>
        /// <param name="dto">ammount and confirmation code</param>
        /// <returns>updated user info</returns>
        [Route("deposit")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<UserDetailsDto> Deposit(AccountDepositDto dto)
        {
            logger.LogTrace($"Deposit, {dto.Amount}");

            try
            {
                // extract appUser key from active user claims
                var currentUserKeyValue = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

                var response = transactionsProvider.Deposit(currentUserKeyValue, dto);

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
                logger.LogCritical($"Deposit, {dto.Amount}", ex);
                return BadRequest();
            }
        }
    }
}
