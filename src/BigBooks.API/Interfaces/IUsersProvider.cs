using BigBooks.API.Models;
using BigBooks.API.Providers;
using Microsoft.AspNetCore.JsonPatch;

namespace BigBooks.API.Interfaces
{
    public interface IUsersProvider
    {
        System.Threading.Tasks.Task<UserDetailsDto> GetUser(int key);
        System.Threading.Tasks.Task<List<UserOverviewDto>> GetUsers(bool? activeStatus);
        System.Threading.Tasks.Task<ProviderKeyResponse> AddUser(UserAddUpdateDto dto);
        System.Threading.Tasks.Task<ProviderKeyResponse> GetUserKeyFromToken(string currentUserValue);
        System.Threading.Tasks.Task<UserDetailsDto> GetCurrentUserDetails(string currentUserValue);
        System.Threading.Tasks.Task<ProviderKeyResponse> UpdateAccount(int key, JsonPatchDocument<UserAddUpdateDto> patchDoc);
        System.Threading.Tasks.Task<bool> IsUserActive(int key);
    }
}