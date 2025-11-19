using BigBooks.API.Core;
using BigBooks.API.Entities;
using BigBooks.API.Interfaces;
using BigBooks.API.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace BigBooks.API.Providers 
{
    public class BooksProvider(BigBookDbContext ctx, ILogger<BooksProvider> logger) : BaseProvider, IBooksProvider
    {
        public bool BookExists(int key)
        {
            logger.LogDebug($"BookExists, {key}");

            return ctx.Books.Any(b => b.Key == key);
        }

        public BookDetailsDto? GetBook(int key)
        {
            logger.LogDebug($"GetBook, {key}");

            var book = ctx.Books
                .Include(b => b.Reviews)
                .AsNoTracking()
                .FirstOrDefault(b => b.Key == key);

            if (book == null)
            {
                return null;
            }

            double? bookRating = CalculateBookRating(book.Reviews);

            return new BookDetailsDto
            {
                Key = key,
                Title = book.Title,
                Author = book.Author,
                Isbn = book.Isbn.ToString("D").ToUpper(),
                Description = book.Description,
                Genre = book.Genre.ToString(),
                Price = book.Price.ToString("C"),
                InStock = book.StockQuantity > 0,
                Rating = bookRating.HasValue
                    ? (double?)Math.Round(bookRating.Value, 2)
                    : null
            };
        }

        public List<BookOverviewDto> GetBooksByGenre(Genre genre)
        {
            logger.LogDebug($"GetBooksByGenre, {genre}");

            var books = ctx.Books
                .Where(b => b.Genre == genre)
                .Include(b => b.Reviews)
                .AsNoTracking()
                .ToList();

            return books.Select(b => new BookOverviewDto
            {
                Key = b.Key,
                Title = b.Title,
                Author = b.Author,
                Genre = b.Genre.ToString(),
                Rating = CalculateBookRating(b.Reviews)
            })
            .OrderByDescending(b => b.Rating)
            .ToList();
        }

        public List<BookOverviewDto> GetBooksByAuthor(string author)
        {
            logger.LogDebug($"GetBooksByAuthor, {author}");

            var books = ctx.Books
                .Where(b => b.Author.ToLower().Contains(author.ToLower()))
                .Include(b => b.Reviews)
                .AsNoTracking()
                .ToList();

            return books.Select(b => new BookOverviewDto
            {
                Key = b.Key,
                Title = b.Title,
                Author = b.Author,
                Genre = b.Genre.ToString(),
                Rating = CalculateBookRating(b.Reviews)
            })
            .OrderByDescending(b => b.Rating)
            .ToList();
        }

        public ProviderKeyResponse AddBook(BookAddUpdateDto dto)
        {
            logger.LogDebug($"AddBook, {dto.Title}");

            if (IsDuplicateIsbn(dto.Isbn, null))
            {
                return new ProviderKeyResponse(null, $"Duplicate ISBN {dto.Isbn}");
            }

            var addedBook = new Book
            {
                Title = dto.Title,
                Author = dto.Author,
                Isbn = dto.Isbn,
                Description = dto.Description,
                Genre = dto.Genre,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity
            };

            ctx.Books.Add(addedBook);
            ctx.SaveChanges();

            return new ProviderKeyResponse(addedBook.Key, string.Empty);
        }

        public ProviderKeyResponse UpdateBook(int key, JsonPatchDocument<BookAddUpdateDto> patchDoc)
        {
            logger.LogDebug($"UpdateBook, {key}");

            var existingBook = ctx.Books
                .AsNoTracking()
                .FirstOrDefault(b => b.Key == key);

            if (existingBook == null)
            {
                return new ProviderKeyResponse(null, $"Book key {key} not found");
            }

            // transform from entity to dto
            var updateDto = new BookAddUpdateDto
            {
                Title = existingBook.Title,
                Author = existingBook.Author,
                Isbn = existingBook.Isbn,
                Description = existingBook.Description,
                Genre = existingBook.Genre,
                Price = existingBook.Price,
                StockQuantity = existingBook.StockQuantity
            };

            // transform existing object according to json patch
            patchDoc.ApplyTo(updateDto);
            // confirm transformed object obeys dto rules
            var validationCheck = ValidateDto(updateDto);
            if (!validationCheck.Valid)
            {
                return new ProviderKeyResponse(null, validationCheck.Error);
            }

            if (IsDuplicateIsbn(updateDto.Isbn, key))
            {
                return new ProviderKeyResponse(null, $"Duplicate ISBN {updateDto.Isbn}");
            }

            var modifiedBook = ctx.Books.Single(b => b.Key == key);
            modifiedBook.Title = updateDto.Title;
            modifiedBook.Author = updateDto.Author;
            modifiedBook.Isbn = updateDto.Isbn;
            modifiedBook.Genre = updateDto.Genre;
            modifiedBook.Price = updateDto.Price;
            modifiedBook.StockQuantity = updateDto.StockQuantity;

            ctx.SaveChanges();
            return new ProviderKeyResponse(key, string.Empty);
        }

        /// <summary>
        /// Remove designated quantity of books from available stock
        /// </summary>
        /// <param name="bookKey"></param>
        /// <param name="requestedQuantity"></param>
        /// <returns>
        /// True if operation successful, stock available
        /// False if operation fail, stock unavailable
        /// </returns>
        public bool RemoveFromStock(int bookKey, int requestedQuantity)
        {
            logger.LogDebug($"RemoveFromStock, {bookKey}, {requestedQuantity}");

            var selectedBook = ctx.Books
                .Single(b => b.Key == bookKey);

            if (selectedBook.StockQuantity >= requestedQuantity)
            {
                selectedBook.StockQuantity -= requestedQuantity;
                ctx.SaveChanges();
                return true;
            }

            // insufficient quantity
            return false;
        }

        private double? CalculateBookRating(ICollection<BookReview> reviews)
        {
            return reviews.Any()
                ? reviews.Average(r => r.Score)
                : null;
        }

        private bool IsDuplicateIsbn(Guid isbnValue, int? existingBookKey)
        {
            // for update request, exclude existing isbn in duplicate check
            return ctx.Books.Where(b => b.Key != existingBookKey).Any(b => b.Isbn == isbnValue);
        }
    }
}
