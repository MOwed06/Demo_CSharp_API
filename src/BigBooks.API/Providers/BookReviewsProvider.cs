using BigBooks.API.Entities;
using BigBooks.API.Interfaces;
using BigBooks.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BigBooks.API.Providers
{
    public class BookReviewsProvider(BigBookDbContext ctx, ILogger<BookReviewsProvider> logger) : BaseProvider, IBookReviewsProvider
    {
        private const string ANONYMOUS_USER = @"Anonymous";

        public List<BookReviewDto> GetBookReviews(int bookKey)
        {
            logger.LogDebug($"GetBookReviews {bookKey}");

            var reviews = ctx.BookReviews
                .AsNoTracking()
                .Where(r => r.BookKey == bookKey)
                .Include(r => r.Book)
                .Include(r => r.User)
                .OrderBy(r => r.ReviewDate)
                .ToList();

            return reviews.Select(r => new BookReviewDto
            {
                ReviewKey = r.Key,
                BookTitle = r.Book.Title,
                Score = r.Score,
                ReviewDate = r.ReviewDate,
                User = (r.User == null)
                    ? ANONYMOUS_USER
                    : r.User.UserEmail,
                Description = r.Description,
            })
            .ToList();
        }
    }
}
