using BigBooks.API.Models;
using BigBooks.API.Providers;

namespace BigBooks.API.Interfaces
{
    public interface ITransactionsProvider
    {
        ProviderKeyResponse PurchaseBooks(string currentUserKeyValue, PurchaseRequestDto dto);
    }
}