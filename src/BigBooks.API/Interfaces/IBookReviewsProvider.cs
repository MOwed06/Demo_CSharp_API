using BigBooks.API.Models;

namespace BigBooks.API.Interfaces
{
    public interface IBookReviewsProvider
    {
        List<BookReviewDto> GetBookReviews(int bookKey);
    }
}