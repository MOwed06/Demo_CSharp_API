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
    public class BookController(IBookProvider bookProvider, ILogger<BookController> logger) : ControllerBase
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
            logger.LogTrace($"GetBook, {key}");

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
                logger.LogCritical($"GetBook, {key}", ex);
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
            logger.LogTrace($"GetBooksByGenre, {name}");

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
                logger.LogCritical($"GetBooksByGenre, {name}", ex);
                return BadRequest();
            }
        }

        /// <summary>
        /// Books by string match on author name
        /// </summary>
        /// <param name="name"></param>
        /// <returns>list of books</returns>
        [HttpGet("author", Name = "GetBooksByAuthor")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<IEnumerable<BookOverviewDto>> GetBooksByAuthor([FromQuery] string name)
        {
            logger.LogTrace($"GetBooksByAuthor, {name}");

            try
            {
                var bookDtos = bookProvider.GetBooksByAuthor(name);

                return Ok(bookDtos);
            }
            catch (Exception ex)
            {
                logger.LogCritical($"GetBooksByAuthor, {name}", ex);
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
            logger.LogTrace($"AddBook, {dto.Title}");

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
                logger.LogCritical($"AddBook, {dto.Title}", ex);
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
            logger.LogTrace($"UpdateBook, {key}");

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
                logger.LogCritical($"UpdateBook, {key}", ex);
                return BadRequest();
            }
        }
    }
}
