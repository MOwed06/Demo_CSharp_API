using BigBooks.API.Authentication;

namespace BigBooks.API.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> GenerateToken(AuthRequest request);
    }
}