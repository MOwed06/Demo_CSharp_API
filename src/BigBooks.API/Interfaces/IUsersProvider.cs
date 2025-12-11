using BigBooks.API.Models;
using BigBooks.API.Providers;
using Microsoft.AspNetCore.JsonPatch;

namespace BigBooks.API.Interfaces
{
    public interface IUsersProvider
    {
        Task<UserDetailsDto> GetUser(int key);
        Task<List<UserOverviewDto>> GetUsers(bool? activeStatus);
        Task<ProviderKeyResponse> AddUser(UserAddUpdateDto dto);
        Task<ProviderKeyResponse> GetUserKeyFromToken(string currentUserValue);
        Task<UserDetailsDto> GetCurrentUserDetails(string currentUserValue);
        Task<ProviderKeyResponse> UpdateAccount(int key, JsonPatchDocument<UserAddUpdateDto> patchDoc);
        Task<bool> IsUserActive(int key);
    }
}