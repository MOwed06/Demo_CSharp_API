using BigBooks.API.Models;
using BigBooks.API.Providers;

namespace BigBooks.API.Interfaces
{
    public interface IBookReviewsProvider
    {
        System.Threading.Tasks.Task<List<BookReviewDto>> GetBookReviews(int bookKey);

        System.Threading.Tasks.Task<BookReviewDto> GetBookReview(int reviewKey);
        System.Threading.Tasks.Task<ProviderKeyResponse> AddBookReview(string currentUserValue, int bookKey, BookReviewAddDto dto);
    }
}