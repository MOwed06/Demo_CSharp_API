using BigBooks.API.Core;
using BigBooks.API.Interfaces;
using BigBooks.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BigBooks.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class BooksController(IBooksProvider bookProvider,
        ILogger<BooksController> logger) : BigBooksController(logger)
    {
        /// <summary>
        /// Get book by key
        /// </summary>
        /// <param name="key">key of book to retrieve</param>
        /// <returns>details of selected book</returns>
        [HttpGet("{key}", Name = "GetBook")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BookDetailsDto>> GetBook(int key)
        {
            var statusMsg = $"GetBook, {key}";
            logger.LogTrace(statusMsg);

            try
            {
                var bookDto = await bookProvider.GetBook(key);

                if (bookDto == null)
                {
                    return InvalidRequest(statusCode: HttpStatusCode.NotFound,
                        errorMessage: $"No book key {key}");
                }

                return Ok(bookDto);
            }
            catch (Exception ex)
            {
                return FailedRequest(statusMsg, ex);
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
        public async Task<ActionResult<IEnumerable<BookOverviewDto>>> GetBooksByGenre([FromQuery] string name)
        {
            var statusMsg = $"GetBooksByGenre, {name}";
            logger.LogTrace(statusMsg);

            try
            {
                Genre queryGenre = Genre.Undefined;
                var parseOk = Enum.TryParse(name, true, out queryGenre);

                if (!parseOk)
                {
                    return InvalidRequest(statusCode: HttpStatusCode.BadRequest,
                        errorMessage: $"bad genre specification, {name}");
                }

                var bookDtos = await bookProvider.GetBooksByGenre(queryGenre);

                return Ok(bookDtos);
            }
            catch (Exception ex)
            {
                return FailedRequest(statusMsg, ex);
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
        public async Task<ActionResult<IEnumerable<BookOverviewDto>>> GetBooksByAuthor([FromQuery] string name)
        {
            var statusMsg = $"GetBooksByAuthor, {name}";
            logger.LogTrace(statusMsg);

            try
            {
                var bookDtos = await bookProvider.GetBooks(name);
                return Ok(bookDtos);
            }
            catch (Exception ex)
            {
                return FailedRequest(statusMsg, ex);
            }
        }

        /// <summary>
        /// Retrieve list of authors and number of books
        /// </summary>
        /// <returns>author info list</returns>
        [HttpGet("authorlist")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<AuthorInfoDto>>> GetAuthorList()
        {
            logger.LogTrace("GetAuthorList");

            try
            {
                var authorDtos = await bookProvider.GetBookAuthors();
                return Ok(authorDtos);
            }
            catch (Exception ex)
            {
                return FailedRequest("GetAuthorList", ex);
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
        public async Task<ActionResult<BookDetailsDto>> AddBook(BookAddUpdateDto dto)
        {
            var statusMsg = $"AddBook, {dto.Title}";
            logger.LogTrace(statusMsg);

            try
            {
                var response = await bookProvider.AddBook(dto);

                if (!response.Key.HasValue)
                {
                    return InvalidRequest(statusCode: HttpStatusCode.BadRequest,
                        errorMessage: response.Error);
                }

                return await GetBook(response.Key.Value);
            }
            catch (Exception ex)
            {
                return FailedRequest(statusMsg, ex);
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
        public async Task<ActionResult<BookDetailsDto>> UpdateBook(int key, JsonPatchDocument<BookAddUpdateDto> patchDoc)
        {
            var statusMsg = $"UpdateBook, {key}";
            logger.LogTrace(statusMsg);

            try
            {
                if (!await bookProvider.BookExists(key))
                {
                    return InvalidRequest(statusCode: HttpStatusCode.NotFound,
                        errorMessage: $"No book key {key}");
                }

                var response = await bookProvider.UpdateBook(key, patchDoc);

                if (!response.Key.HasValue)
                {
                    return InvalidRequest(statusCode: HttpStatusCode.BadRequest,
                        errorMessage: response.Error);
                }

                return await GetBook(response.Key.Value);
            }
            catch (Exception ex)
            {
                return FailedRequest(statusMsg, ex);
            }
        }
    }
}
