using BigBooks.API.Entities;
using BigBooks.API.Interfaces;
using BigBooks.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BigBooks.API.Providers
{
    public class UserProvider(BigBookDbContext ctx, ILogger<UserProvider> logger) : BookStoreProvider, IUserProvider
    {
        public UserDetailsDto? GetUser(int key)
        {
            logger.LogDebug($"GetUser, {key}");

            var appUser = ctx.AppUsers
                .AsNoTracking()
                .SingleOrDefault(u => u.Key == key);

            if (appUser == null)
            {
                return null;
            }

            var userBooks = ctx.Books
                .Where(b => appUser.UserBookIds.Contains(b.Key))
                .Select(b => b.Title)
                .ToList();

            return new UserDetailsDto
            {
                Key = key,
                UserEmail = appUser.UserEmail,
                UserName = appUser.UserName,
                Role = appUser.Role.ToString(),
                Wallet = appUser.Wallet,
                Books = userBooks
            };
        }
    }
}
