using BigBooks.API.Core;
using BigBooks.API.Entities;
using BigBooks.API.Interfaces;
using BigBooks.API.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace BigBooks.API.Providers
{
    public class BooksProvider(IDbContextFactory<BigBookDbContext> dbContextFactory,
        ILogger<BooksProvider> logger) : BaseProvider, IBooksProvider
    {
        public async Task<bool> BookExists(int key)
        {
            logger.LogDebug("BookExists, {0}", key);

            using (var ctx = dbContextFactory.CreateDbContext())
            {
                return await ctx.Books.AnyAsync(b => b.Key == key);
            }    
        }

        public async Task<BookDetailsDto> GetBook(int key)
        {
            logger.LogDebug("GetBook, {0}", key);

            using (var ctx = dbContextFactory.CreateDbContext())
            {
                var book = await ctx.Books
                .Include(b => b.Reviews)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Key == key);

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
                        : null,
                    Reviews = book.Reviews.Count()
                };
            }
        }

        public async Task<List<BookOverviewDto>> GetBooksByGenre(Genre genre)
        {
            logger.LogDebug("GetBooksByGenre, {0}", genre);

            using (var ctx = dbContextFactory.CreateDbContext())
            {
                var books = await ctx.Books
                .Where(b => b.Genre == genre)
                .Include(b => b.Reviews)
                .AsNoTracking()
                .ToListAsync();

                return books.Select(b => new BookOverviewDto
                {
                    Key = b.Key,
                    Title = b.Title,
                    Author = b.Author,
                    Genre = b.Genre.ToString(),
                    Rating = CalculateBookRating(b.Reviews),
                    Reviews = b.Reviews.Count()
                })
                .OrderByDescending(b => b.Rating)
                .ToList();
            }
        }

        /// <summary>
        /// Return all books
        /// Filter by author if author provided
        /// Sort by book rating
        /// </summary>
        /// <param name="author"></param>
        /// <returns></returns>
        public async Task<List<BookOverviewDto>> GetBooks(string author)
        {
            logger.LogDebug("GetBooks, {0}", author);

            using (var ctx = dbContextFactory.CreateDbContext())
            {
                // if author empty, then return all books
                var books = string.IsNullOrEmpty(author)
                ? await ctx.Books
                    .AsNoTracking()
                    .Include(b => b.Reviews)
                    .ToListAsync()
                : await ctx.Books
                    .Where(b => b.Author.ToLower().Contains(author.ToLower()))
                    .Include(b => b.Reviews)
                    .AsNoTracking()
                    .ToListAsync();

                return books.Select(b => new BookOverviewDto
                {
                    Key = b.Key,
                    Title = b.Title,
                    Author = b.Author,
                    Genre = b.Genre.ToString(),
                    Rating = CalculateBookRating(b.Reviews),
                    Reviews = b.Reviews.Count()
                })
                .OrderByDescending(b => b.Rating)
                .ToList();
            }
        }

        public async Task<ProviderKeyResponse> AddBook(BookAddUpdateDto dto)
        {
            logger.LogDebug("AddBook, {0}", dto.Title);

            using (var ctx = dbContextFactory.CreateDbContext())
            {
                if (IsDuplicateIsbn(ctx, dto.Isbn, null))
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
                await ctx.SaveChangesAsync();

                return new ProviderKeyResponse(addedBook.Key, string.Empty);
            }
        }

        public async Task<ProviderKeyResponse> UpdateBook(int key, JsonPatchDocument<BookAddUpdateDto> patchDoc)
        {
            logger.LogDebug("UpdateBook, {0}", key);

            using (var ctx = dbContextFactory.CreateDbContext())
            {
                var existingBook = await ctx.Books
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Key == key);

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

                if (IsDuplicateIsbn(ctx, updateDto.Isbn, key))
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

                await ctx.SaveChangesAsync();
                return new ProviderKeyResponse(key, string.Empty);
            }
        }

        /// <summary>
        /// Remove designated quantity of books from available stock
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="bookKey"></param>
        /// <param name="requestedQuantity"></param>
        /// <returns>
        /// True if operation successful, stock available
        /// False if operation fail, stock unavailable
        /// </returns>
        public bool RemoveFromStock(BigBookDbContext ctx,
            int bookKey,
            int requestedQuantity)
        {
            logger.LogDebug("RemoveFromStock, {0}, {1}",
                bookKey,
                requestedQuantity);

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

        public async Task<List<AuthorInfoDto>> GetBookAuthors()
        {
            using (var ctx = dbContextFactory.CreateDbContext())
            {
                return await ctx.Books
                .GroupBy(b => b.Author)
                .Select(g => new AuthorInfoDto
                {
                    Author = g.Key,
                    BookCount = g.Count()
                })
                .OrderByDescending(b => b.BookCount)
                .ToListAsync();
            }
        }

        private double? CalculateBookRating(ICollection<BookReview> reviews)
        {
            double? score = reviews.Any()
                ? reviews.Average(r => r.Score)
                : null;

            return score.HasValue
                // truncate to 2 decimal places
                ? Math.Truncate(100 * score.Value) / 100
                : null;
        }

        private bool IsDuplicateIsbn(BigBookDbContext ctx,
            Guid isbnValue,
            int? existingBookKey)
        {
            // for update request, exclude existing isbn in duplicate check
            return ctx.Books.Where(b => b.Key != existingBookKey).Any(b => b.Isbn == isbnValue);
        }
    }
}
