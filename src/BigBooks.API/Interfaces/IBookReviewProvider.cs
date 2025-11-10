using BigBooks.API.Models;

namespace BigBooks.API.Interfaces
{
    public interface IBookReviewProvider
    {
        List<BookReviewDto> GetBookReviews(int bookKey);
    }
}