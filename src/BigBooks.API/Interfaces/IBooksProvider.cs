using BigBooks.API.Core;
using BigBooks.API.Entities;
using BigBooks.API.Models;
using BigBooks.API.Providers;
using Microsoft.AspNetCore.JsonPatch;

namespace BigBooks.API.Interfaces
{
    public interface IBooksProvider
    {
        System.Threading.Tasks.Task<ProviderKeyResponse> AddBook(BookAddUpdateDto dto);
        System.Threading.Tasks.Task<bool> BookExists(int key);
        System.Threading.Tasks.Task<BookDetailsDto> GetBook(int key);
        System.Threading.Tasks.Task<List<BookOverviewDto>> GetBooks(string author);
        System.Threading.Tasks.Task<List<BookOverviewDto>> GetBooksByGenre(Genre genre);
        System.Threading.Tasks.Task<ProviderKeyResponse> UpdateBook(int key, JsonPatchDocument<BookAddUpdateDto> patchDoc);
        System.Threading.Tasks.Task<bool> RemoveFromStock(BigBookDbContext ctx, int bookKey, int requestedQuantity);
        System.Threading.Tasks.Task<List<AuthorInfoDto>> GetBookAuthors();
    }
}
