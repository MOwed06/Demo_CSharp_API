using BigBooks.API.Core;
using BigBooks.API.Interfaces;
using BigBooks.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace BigBooks.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class BooksController(IBooksProvider bookProvider, ILogger<BooksController> logger) : ControllerBase
    {
        /// <summary>
        /// Get book by key
        /// </summary>
        /// <param name="key">key of book to retrieve</param>
        /// <returns>details of selected book</returns>
        [HttpGet("{key}", Name = "GetBook")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<BookDetailsDto> GetBook(int key)
        {
            var statusMsg = $"GetBook, {key}";
            logger.LogTrace(statusMsg);

            try
            {
                var bookDto = bookProvider.GetBook(key);

                if (bookDto == null)
                {
                    return NotFound($"book {key}");
                }

                return Ok(bookDto);
            }
            catch (Exception ex)
            {
                logger.LogCritical(message: statusMsg,
                    exception: ex);
                return BadRequest();
            }
        }

        /// <summary>
        /// Get books by genre
        /// </summary>
        /// <remarks>
        /// Genre: Fiction, Childrens, Fantasy, Mystery, History, Romance
        /// </remarks>
        /// <param name="name">genre to retrieve</param>
        /// <returns>matched books</returns>
        /// <exception cref="ArgumentException"></exception>
        [HttpGet("genre", Name = "GetBooksByGenre")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<IEnumerable<BookOverviewDto>> GetBooksByGenre([FromQuery] string name)
        {
            var statusMsg = $"GetBooksByGenre, {name}";
            logger.LogTrace(statusMsg);

            try
            {
                Genre queryGenre = Genre.Undefined;

                var parseOk = Enum.TryParse(name, true, out queryGenre);

                if (!parseOk)
                {
                    var errorMsg = $"bad genre specification, {name}";
                    logger.LogDebug(errorMsg);
                    return BadRequest(errorMsg);
                }

                var bookDtos = bookProvider.GetBooksByGenre(queryGenre);

                return Ok(bookDtos);
            }
            catch (Exception ex)
            {
                logger.LogCritical(message: statusMsg,
                    exception: ex);
                return BadRequest();
            }
        }

        /// <summary>
        /// Books by string match on author name.
        /// If author omitted, return all books
        /// </summary>
        /// <param name="name"></param>
        /// <returns>list of books</returns>
        [HttpGet("author", Name = "GetBooksByAuthor")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<IEnumerable<BookOverviewDto>> GetBooksByAuthor([FromQuery] string name)
        {
            var statusMsg = $"GetBooksByAuthor, {name}";
            logger.LogTrace(statusMsg);

            try
            {
                var bookDtos = bookProvider.GetBooks(name);

                return Ok(bookDtos);
            }
            catch (Exception ex)
            {
                logger.LogCritical(message: statusMsg,
                    exception: ex);
                return BadRequest();
            }
        }

        /// <summary>
        /// Add new book.
        /// </summary>
        /// <remarks>
        /// Requires authenticated user with Admin role
        /// </remarks>
        /// <param name="dto">details of book to add</param>
        /// <returns>details of added book</returns>
        [HttpPost]
        [Authorize(Policy = "BookAccess")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<BookDetailsDto> AddBook(BookAddUpdateDto dto)
        {
            var statusMsg = $"AddBook, {dto.Title}";
            logger.LogTrace(statusMsg);

            try
            {
                var response = bookProvider.AddBook(dto);

                if (response.Key.HasValue)
                {
                    return GetBook(response.Key.Value);
                }

                throw new Exception(response.Error);
            }
            catch (Exception ex)
            {
                logger.LogCritical(message: statusMsg,
                    exception: ex);
                return BadRequest();
            }
        }

        /// <summary>
        /// Update properties of existing book.
        /// </summary>
        /// <remarks>
        /// Requires authenticated user with Admin role
        /// </remarks>
        /// <param name="key">key of book to modify</param>
        /// <param name="patchDoc">list of changes</param>
        /// <returns></returns>
        [HttpPatch("{key}")]
        [Authorize(Policy = "BookAccess")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BookDetailsDto> UpdateBook(int key, JsonPatchDocument<BookAddUpdateDto> patchDoc)
        {
            var statusMsg = $"UpdateBook, {key}";
            logger.LogTrace(statusMsg);

            try
            {
                if (!bookProvider.BookExists(key))
                {
                    var errorMsg = $"Invalid book key {key}";
                    logger.LogDebug(errorMsg);
                    return NotFound(errorMsg);
                }

                var response = bookProvider.UpdateBook(key, patchDoc);

                if (response.Key.HasValue)
                {
                    return GetBook(response.Key.Value);
                }

                throw new Exception(response.Error);
            }
            catch (Exception ex)
            {
                logger.LogCritical(message: statusMsg,
                    exception: ex);
                return BadRequest();
            }
        }
    }
}
