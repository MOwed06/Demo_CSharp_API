using BigBooks.API.Entities;
using BigBooks.API.Interfaces;
using BigBooks.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BigBooks.API.Providers
{
    public class BookReviewsProvider(IDbContextFactory<BigBookDbContext> dbContextFactory,
        IUsersProvider usersProvider,
        ILogger<BookReviewsProvider> logger) : BaseProvider(dbContextFactory), IBookReviewsProvider
    {
        private const string ANONYMOUS_USER = @"Anonymous";

        public List<BookReviewDto> GetBookReviews(int bookKey)
        {
            logger.LogDebug("GetBookReviews {0}", bookKey);

            using (var ctx = dbContextFactory.CreateDbContext())
            {
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

        public BookReviewDto GetBookReview(int reviewKey)
        {
            logger.LogDebug("GetBookReview {0}", reviewKey);

            using (var ctx = dbContextFactory.CreateDbContext())
            {
                var review = ctx.BookReviews
                .AsNoTracking()
                .Include(r => r.Book)
                .Include(r => r.User)
                .SingleOrDefault(r => r.Key == reviewKey);

                if (review == null)
                {
                    return null;
                }

                return new BookReviewDto
                {
                    ReviewKey = reviewKey,
                    BookTitle = review.Book.Title,
                    Score = review.Score,
                    ReviewDate = review.ReviewDate,
                    User = (review.User == null)
                        ? ANONYMOUS_USER
                        : review.User.UserEmail,
                    Description = review.Description,
                };
            }
        }

        public ProviderKeyResponse AddBookReview(string currentUserValue, int bookKey, BookReviewAddDto dto)
        {
            logger.LogDebug("AddBookReview {0}, {1}", currentUserValue, bookKey);

            var userMatch = usersProvider.GetUserKeyFromToken(currentUserValue);

            if (!userMatch.Key.HasValue)
            {
                // no match to user
                return new ProviderKeyResponse(null, userMatch.Error);
            }

            if (!usersProvider.IsUserActive(userMatch.Key.Value))
            {
                // inactive user, review denied
                return new ProviderKeyResponse(null, "User is deactivated");
            }

            using (var ctx = dbContextFactory.CreateDbContext())
            {
                if (!ctx.Books.Any(b => b.Key == bookKey))
                {
                    return new ProviderKeyResponse(null, $"Invalid Book Key {bookKey}");
                }

                if (ctx.BookReviews.Any(r => (r.UserKey == userMatch.Key.Value)
                        && (r.BookKey == bookKey)))
                {
                    // user has previously reviewed book
                    return new ProviderKeyResponse(null, "Existing BookReview by user");
                }

                int? userLink = dto.IsAnonymous
                    ? null
                    : userMatch.Key.Value;

                var addedReview = new BookReview
                {
                    Score = dto.Score,
                    ReviewDate = DateTime.Now,
                    Description = dto.Description,
                    UserKey = userLink,
                    BookKey = bookKey
                };

                ctx.BookReviews.Add(addedReview);
                ctx.SaveChanges();

                return new ProviderKeyResponse(addedReview.Key, string.Empty);
            }
        }
    }
}
