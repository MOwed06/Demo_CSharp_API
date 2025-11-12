using BigBooks.API.Models;
using BigBooks.API.Providers;

namespace BigBooks.API.Interfaces
{
    public interface IUserProvider
    {
        UserDetailsDto GetUser(int key);
        public List<UserOverviewDto> GetUsers();
        ProviderKeyResponse AddUser(UserAddUpdateDto dto);
    }
}