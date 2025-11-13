using BigBooks.API.Entities;
using BigBooks.API.Interfaces;
using BigBooks.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BigBooks.API.Providers
{
    public class UsersProvider(BigBookDbContext ctx,
        ILogger<UsersProvider> logger) : BaseProvider, IUsersProvider
    {
        public UserDetailsDto? GetUser(int key)
        {
            logger.LogDebug($"GetUser, {key}");

            var appUser = ctx.AppUsers
                .AsNoTracking()
                .Include(u => u.BookPurchases)
                .SingleOrDefault(u => u.Key == key);

            if (appUser == null)
            {
                return null;
            }

            // use hashset to prohibit duplicate entries
            var userBookKeys = appUser.BookPurchases
                .Select(u => u.BookKey)
                .ToHashSet();

            var userBooks = ctx.Books
                .Where(b => userBookKeys.Contains(b.Key))
                .Select(b => b.Title)
                .ToList();

            return new UserDetailsDto
            {
                Key = key,
                UserEmail = appUser.UserEmail,
                UserName = appUser.UserName,
                Role = appUser.Role.ToString(),
                Wallet = appUser.Wallet.ToString("C"),
                Books = userBooks
            };
        }

        public List<UserOverviewDto> GetUsers()
        {
            logger.LogDebug("GetUsers");

            var appUsers = ctx.AppUsers
                .AsNoTracking()
                .Include(u => u.BookPurchases)
                .ToList();

            return appUsers
                .Select(u => new UserOverviewDto
                {
                    Key = u.Key,
                    UserEmail = u.UserEmail,
                    Role = u.Role.ToString(),
                    Wallet = u.Wallet.ToString("C"),
                    BookCount = u.BookPurchases.Count()
                })
                .ToList();
        }

        /// <summary>
        /// Allow current user to view their full details
        /// </summary>
        /// <param name="currentUserKeyValue"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public UserDetailsDto GetCurrentUser(string currentUserKeyValue)
        {
            logger.LogDebug("GetCurrentUser");

            var currentUser = GetUserKeyFromToken(currentUserKeyValue);

            if (!currentUser.Key.HasValue)
            {
                throw new Exception(currentUser.Error);
            }

            return GetUser(currentUser.Key.Value);
        }

        public ProviderKeyResponse AddUser(UserAddUpdateDto dto)
        {
            logger.LogDebug($"AddUser {dto.UserEmail}");

            if (ctx.AppUsers.Any(u => u.UserEmail == dto.UserEmail))
            {
                // duplicate email
                return new ProviderKeyResponse(null, "Duplicate UserEmail");
            }

            var nextUser = new AppUser
            {
                UserEmail = dto.UserEmail,
                UserName = dto.UserName,
                Password = dto.Password,
                Wallet = dto.Wallet,
                Role = dto.Role,
                BookPurchases = new List<BookPurchase>()
            };

            ctx.AppUsers.Add(nextUser);
            ctx.SaveChanges();

            return new ProviderKeyResponse(nextUser.Key, string.Empty);
        }

        public ProviderKeyResponse GetUserKeyFromToken(string currentUserKeyValue)
        {
            logger.LogDebug($"GetUserKeyFromToken, {currentUserKeyValue}");

            var userKey = -1;

            if (!int.TryParse(currentUserKeyValue, out userKey))
            {
                return new ProviderKeyResponse(null, $"Invalid UserClaimId {currentUserKeyValue}");
            }

            var appUser = ctx.AppUsers
                .AsNoTracking()
                .SingleOrDefault(u => u.Key == userKey);

            if (appUser == null)
            {
                return new ProviderKeyResponse(null, $"Invalid user {userKey}");
            }

            return new ProviderKeyResponse(appUser.Key, string.Empty);
        }
    }
}
