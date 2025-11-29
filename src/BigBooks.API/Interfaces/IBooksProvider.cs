using BigBooks.API.Core;
using BigBooks.API.Models;
using BigBooks.API.Providers;
using Microsoft.AspNetCore.JsonPatch;

namespace BigBooks.API.Interfaces
{
    public interface IBooksProvider
    {
        ProviderKeyResponse AddBook(BookAddUpdateDto dto);
        bool BookExists(int key);
        BookDetailsDto GetBook(int key);
        List<BookOverviewDto> GetBooks(string author);
        List<BookOverviewDto> GetBooksByGenre(Genre genre);
        ProviderKeyResponse UpdateBook(int key, JsonPatchDocument<BookAddUpdateDto> patchDoc);
        bool RemoveFromStock(int bookKey, int requestedQuantity);
        List<AuthorInfoDto> GetBookAuthors();
    }
}
