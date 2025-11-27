using BigBooks.API.Entities;
using BigBooks.API.Interfaces;
using BigBooks.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BigBooks.API.Controllers
{
    [Route("api/books/{book}/reviews")]
    [ApiController]
    [Authorize]
    public class BookReviewsController(IBookReviewsProvider bookReviewPrv,
        IBooksProvider bookPrv,
        ILogger<BookReviewsController> logger) : ControllerBase
    {
        /// <summary>
        /// Get book reviews for designaged book
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<BookReviewDto>> GetBookReviews(int book)
        {
            logger.LogTrace("GetBookReviews {0}", book);

            try
            {
                if (!bookPrv.BookExists(book))
                {
                    var errorMsg = $"No book key {book}";
                    logger.LogDebug(errorMsg);
                    return NotFound(errorMsg);
                }

                var reviewDtos = bookReviewPrv.GetBookReviews(book);

                return Ok(reviewDtos);
            }
            catch (Exception ex)
            {
                var errorMsg = $"GetBookReviews {book}";
                logger.LogCritical(errorMsg, ex);
                return BadRequest();
            }
        }

        /// <summary>
        /// Add review for current user
        /// </summary>
        /// <param name="book">book key, taken from route</param>
        /// <param name="dto">book review details</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<BookReviewDto> AddBookReview(int book, BookReviewAddDto dto)
        {
            logger.LogTrace("AddBookReview {0}", book);

            try
            {
                if (!bookPrv.BookExists(book))
                {
                    var errorMsg = $"No book key {book}";
                    logger.LogDebug(errorMsg);
                    return NotFound(errorMsg);
                }

                // extract appUser key from active user claims
                var currentUserValue = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

                var response = bookReviewPrv.AddBookReview(currentUserValue, book, dto);

                if (response.Key == null)
                {
                    logger.LogError(response.Error);
                    return BadRequest();
                }

                var reviewDto = bookReviewPrv.GetBookReview(response.Key.Value);
                return Ok(reviewDto);
            }
            catch (Exception ex)
            {
                var errorMsg = $"AddBookReview {book}";
                logger.LogCritical(errorMsg, ex);
                return BadRequest();
            }
        }
    }
}
