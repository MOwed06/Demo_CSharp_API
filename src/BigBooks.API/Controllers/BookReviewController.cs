using BigBooks.API.Interfaces;
using BigBooks.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BigBooks.API.Controllers
{
    [Route("api/book/{book}/reviews")]
    [ApiController]
    [Authorize]
    public class BookReviewController(IBookReviewProvider bookReviewPrv,
        IBookProvider bookPrv,
        ILogger<BookReviewController> logger) : ControllerBase
    {

        [HttpGet]
        public ActionResult<IEnumerable<BookReviewDto>> GetBookReviews(int book)
        {
            logger.LogTrace($"GetBookReviews {book}");

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
                logger.LogCritical($"GetBookReviews, {book}", ex);
                return BadRequest();
            }
        }
    }
}
