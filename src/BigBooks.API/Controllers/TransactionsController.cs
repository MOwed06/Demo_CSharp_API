using BigBooks.API.Interfaces;
using BigBooks.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace BigBooks.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransactionsController(ITransactionsProvider transactionsProvider,
        IUsersProvider userProvider,
        ILogger<TransactionsController> logger) : BigBooksController(logger)
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
            var statusMsg = $"PurchaseBooks, book: {dto.BookKey}, qty: {dto.RequestedQuantity}";
            logger.LogTrace(statusMsg);

            try
            {
                // extract appUser key from active user claims
                var currentUserValue = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

                var response = transactionsProvider.PurchaseBooks(currentUserValue, dto);

                if (!response.Key.HasValue)
                {
                    return InvalidRequest(statusCode: HttpStatusCode.BadRequest,
                        errorMessage: response.Error);
                }

                var updatedUserDto = userProvider.GetUser(response.Key.Value);
                return Ok(updatedUserDto);
            }
            catch (Exception ex)
            {
                return FailedRequest(statusMsg, ex);
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
            var statusMsg = $"Deposit, {dto.Amount}";
            logger.LogTrace(statusMsg);

            try
            {
                // extract appUser key from active user claims
                var currentUserValue = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

                var response = transactionsProvider.Deposit(currentUserValue, dto);

                if (!response.Key.HasValue)
                {
                    return InvalidRequest(statusCode: HttpStatusCode.BadRequest,
                        errorMessage: response.Error);
                }

                var updatedUserDto = userProvider.GetUser(response.Key.Value);
                return Ok(updatedUserDto);
            }
            catch (Exception ex)
            {
                return FailedRequest(statusMsg, ex);
            }
        }
    }
}
