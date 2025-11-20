using BigBooks.API.Core;
using BigBooks.API.Entities;
using BigBooks.API.Interfaces;
using BigBooks.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BigBooks.API.Providers
{
    public class UsersProvider(BigBookDbContext ctx,
        ILogger<UsersProvider> logger) : BaseProvider, IUsersProvider
    {
        public UserDetailsDto GetUser(int key)
        {
            logger.LogDebug("GetUser, {0}", key);

            var appUser = ctx.AppUsers
                .AsNoTracking()
                .Include(u => u.Transactions)
                .SingleOrDefault(u => u.Key == key);

            if (appUser == null)
            {
                return null;
            }

            // use hashset to prohibit duplicate entries
            var userBookKeys = appUser.Transactions
                .Where(u => u.BookKey != null)
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
                Transactions = GetUserTransactions(key)
            };
        }

        private List<TransactionOverviewDto> GetUserTransactions(int userKey)
        {
            var userTransactions = ctx.Transactions.Where(t => t.UserKey == userKey)
                .AsNoTracking()
                .Include(t => t.Book)
                .ToList();

            return userTransactions.Select(t => new TransactionOverviewDto
            {
                TransactionKey = t.Key,
                TransactionDate = t.TransactionDate,
                TransactionAmount = t.TransactionAmount,
                TransactionType = (t.TransactionAmount < 0)
                    ? TransactionType.Purchase.ToString()
                    : TransactionType.Deposit.ToString(),
                PurchaseBook = t.Book?.Title,
                PurchaseQuantity = t.PurchaseQuantity
            })
            .OrderByDescending(t => t.TransactionDate)
            .ToList();
        }

        public List<UserOverviewDto> GetUsers()
        {
            logger.LogDebug("GetUsers");

            var appUsers = ctx.AppUsers
                .AsNoTracking()
                .Include(u => u.Transactions)
                .ToList();

            return appUsers
                .Select(u => new UserOverviewDto
                {
                    Key = u.Key,
                    UserEmail = u.UserEmail,
                    Role = u.Role.ToString(),
                    BookCount = u.Transactions
                        .Where(t => t.BookKey != null)
                        .Select(t => t.BookKey)
                        .Distinct()
                        .Count()
                })
                .ToList();
        }

        /// <summary>
        /// Allow current user to view their full details
        /// </summary>
        /// <param name="currentUserValue"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public UserDetailsDto GetCurrentUserDetails(string currentUserValue)
        {
            logger.LogDebug("GetCurrentUser");

            var currentUser = GetUserKeyFromToken(currentUserValue);

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
                Transactions = new List<AccountTransaction>()
            };

            ctx.AppUsers.Add(nextUser);
            ctx.SaveChanges();

            return new ProviderKeyResponse(nextUser.Key, string.Empty);
        }

        public ProviderKeyResponse GetUserKeyFromToken(string currentUserValue)
        {
            logger.LogDebug($"GetUserKeyFromToken, {currentUserValue}");

            var appUser = ctx.AppUsers
                .AsNoTracking()
                .SingleOrDefault(u => u.UserEmail == currentUserValue);

            if (appUser == null)
            {
                return new ProviderKeyResponse(null, $"Invalid user {currentUserValue}");
            }

            return new ProviderKeyResponse(appUser.Key, string.Empty);
        }
    }
}
