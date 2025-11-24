using BigBooks.API.Models;
using BigBooks.API.Providers;

namespace BigBooks.API.Interfaces
{
    public interface ITransactionsProvider
    {
        ProviderKeyResponse PurchaseBooks(string currentUserValue, PurchaseRequestDto dto);
        ProviderKeyResponse Deposit(string currentUserValue, AccountDepositDto dto);
    }
}