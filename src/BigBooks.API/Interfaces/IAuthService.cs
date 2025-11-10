using BigBooks.API.Authentication;

namespace BigBooks.API.Interfaces
{
    public interface IAuthService
    {
        AuthResponse GenerateToken(AuthRequest request);
    }
}