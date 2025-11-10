using BigBooks.API.Models;

namespace BigBooks.API.Interfaces
{
    public interface IUserProvider
    {
        UserDetailsDto GetUser(int key);
    }
}