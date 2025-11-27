using BigBooks.API.Models;
using BigBooks.API.Providers;

namespace BigBooks.API.Interfaces
{
    public interface IBookReviewsProvider
    {
        List<BookReviewDto> GetBookReviews(int bookKey);

        BookReviewDto GetBookReview(int reviewKey);
        ProviderKeyResponse AddBookReview(string currentUserValue, int bookKey, BookReviewAddDto dto);
    }
}