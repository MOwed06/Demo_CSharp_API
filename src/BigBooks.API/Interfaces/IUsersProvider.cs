using BigBooks.API.Models;
using BigBooks.API.Providers;
using Microsoft.AspNetCore.JsonPatch;

namespace BigBooks.API.Interfaces
{
    public interface IUsersProvider
    {
        UserDetailsDto GetUser(int key);
        List<UserOverviewDto> GetUsers(bool? activeStatus);
        ProviderKeyResponse AddUser(UserAddUpdateDto dto);
        ProviderKeyResponse GetUserKeyFromToken(string currentUserValue);
        UserDetailsDto GetCurrentUserDetails(string currentUserValue);
        ProviderKeyResponse UpdateAccount(int key, JsonPatchDocument<UserAddUpdateDto> patchDoc);
        bool IsUserActive(int key);
    }
}