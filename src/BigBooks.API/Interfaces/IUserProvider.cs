using BigBooks.API.Models;
using BigBooks.API.Providers;

namespace BigBooks.API.Interfaces
{
    public interface IUserProvider
    {
        UserDetailsDto GetUser(int key);
        public ProviderKeyResponse PurchaseBooks(string currentUserKeyValue, BookPurchaseRequestDto dto);
        public List<UserOverviewDto> GetUsers();
    }
}