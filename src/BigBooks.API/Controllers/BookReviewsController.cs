using BigBooks.API.Interfaces;
using BigBooks.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace BigBooks.API.Controllers
{
    [Route("api/books/{book}/reviews")]
    [ApiController]
    [Authorize]
    public class BookReviewsController(IBookReviewsProvider bookReviewPrv,
        IBooksProvider bookPrv,
        ILogger<BookReviewsController> logger) : BigBooksController(logger)
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
        public async Task<ActionResult<IEnumerable<BookReviewDto>>> GetBookReviews(int book)
        {
            var statusMsg = $"GetBookReviews {book}";
            logger.LogTrace(statusMsg);

            try
            {
                if (!await bookPrv.BookExists(book))
                {
                    return InvalidRequest(statusCode: HttpStatusCode.NotFound,
                        errorMessage: $"No book key {book}");
                }

                var reviewDtos = await bookReviewPrv.GetBookReviews(book);

                return Ok(reviewDtos);
            }
            catch (Exception ex)
            {
                return FailedRequest(statusMsg, ex);
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
        public async Task<ActionResult<BookReviewDto>> AddBookReview(int book, BookReviewAddDto dto)
        {
            var statusMsg = $"AddBookReview {book}";
            logger.LogTrace(statusMsg);

            try
            {
                if (!await bookPrv.BookExists(book))
                {
                    return InvalidRequest(statusCode: HttpStatusCode.NotFound,
                        errorMessage: $"No book key {book}");
                }

                // extract appUser key from active user claims
                var currentUserValue = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

                var response = await bookReviewPrv.AddBookReview(currentUserValue, book, dto);

                if (response.Key == null)
                {
                    return InvalidRequest(statusCode: HttpStatusCode.BadRequest,
                        errorMessage: response.Error);
                }

                var reviewDto = await bookReviewPrv.GetBookReview(response.Key.Value);
                return Ok(reviewDto);
            }
            catch (Exception ex)
            {
                return FailedRequest(statusMsg, ex);
            }
        }
    }
}
