using BigBooks.API.Models;
using BigBooks.API.Providers;

namespace BigBooks.API.Interfaces
{
    public interface IPurchasesProvider
    {
        ProviderKeyResponse PurchaseBooks(string currentUserKeyValue, PurchaseRequestDto dto);
    }
}