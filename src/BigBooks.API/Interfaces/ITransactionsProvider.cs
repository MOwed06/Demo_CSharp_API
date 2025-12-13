using BigBooks.API.Models;
using BigBooks.API.Providers;

namespace BigBooks.API.Interfaces
{
    public interface ITransactionsProvider
    {
        Task<ProviderKeyResponse> PurchaseBooks(string currentUserValue, PurchaseRequestDto dto);
        Task<ProviderKeyResponse> Deposit(string currentUserValue, AccountDepositDto dto);
    }
}