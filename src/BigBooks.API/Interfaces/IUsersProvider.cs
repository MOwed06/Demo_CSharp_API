using BigBooks.API.Models;
using BigBooks.API.Providers;

namespace BigBooks.API.Interfaces
{
    public interface IUsersProvider
    {
        UserDetailsDto GetUser(int key);
        List<UserOverviewDto> GetUsers();
        ProviderKeyResponse AddUser(UserAddUpdateDto dto);
        ProviderKeyResponse GetUserKeyFromToken(string currentUserValue);
        UserDetailsDto GetCurrentUserDetails(string currentUserValue);
    }
}