using BigBooks.API.Core;
using BigBooks.API.Entities;
using BigBooks.API.Models;
using BigBooks.API.Providers;
using Microsoft.AspNetCore.JsonPatch;

namespace BigBooks.API.Interfaces
{
    public interface IBooksProvider
    {
        Task<ProviderKeyResponse> AddBook(BookAddUpdateDto dto);
        Task<bool> BookExists(int key);
        Task<BookDetailsDto> GetBook(int key);
        Task<List<BookOverviewDto>> GetBooks(string author);
        Task<List<BookOverviewDto>> GetBooksByGenre(Genre genre);
        Task<ProviderKeyResponse> UpdateBook(int key, JsonPatchDocument<BookAddUpdateDto> patchDoc);
        bool RemoveFromStock(BigBookDbContext ctx, int bookKey, int requestedQuantity);
        Task<List<AuthorInfoDto>> GetBookAuthors();
    }
}
