using BigBooks.API.Models;
using BigBooks.API.Providers;

namespace BigBooks.API.Interfaces
{
    public interface IBookReviewsProvider
    {
        Task<List<BookReviewDto>> GetBookReviews(int bookKey);

        Task<BookReviewDto> GetBookReview(int reviewKey);
        Task<ProviderKeyResponse> AddBookReview(string currentUserValue, int bookKey, BookReviewAddDto dto);
    }
}