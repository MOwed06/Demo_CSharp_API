using BigBooks.API.Models;
using BigBooks.API.Providers;

namespace BigBooks.API.Interfaces
{
    public interface ITransactionsProvider
    {
        System.Threading.Tasks.Task<ProviderKeyResponse> PurchaseBooks(string currentUserValue, PurchaseRequestDto dto);
        System.Threading.Tasks.Task<ProviderKeyResponse> Deposit(string currentUserValue, AccountDepositDto dto);
    }
}