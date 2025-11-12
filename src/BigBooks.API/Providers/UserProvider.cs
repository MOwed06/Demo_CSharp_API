using BigBooks.API.Entities;
using BigBooks.API.Interfaces;
using BigBooks.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BigBooks.API.Providers
{
    public class UserProvider(BigBookDbContext ctx,
        IBookProvider bookProvider,
        ILogger<UserProvider> logger) : BigBooksProvider, IUserProvider
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
        /// extract userKey from claim
        /// refer to AuthService.cs, GenerateToken(), user key embedded in token
        /// </summary>
        /// <param name="currentUserKeyValue"></param>
        /// <param name="bookKey"></param>
        /// <param name="requestedQuantity"></param>
        /// <returns>user key associated with purchase</returns>
        public ProviderKeyResponse PurchaseBooks(string currentUserKeyValue, BookPurchaseRequestDto dto)
        {
            logger.LogDebug($"PurchaseBooks, user: {currentUserKeyValue}, book: {dto.BookKey}, qty: {dto.RequestedQuantity}");

            var userKey = -1;

            if (!int.TryParse(currentUserKeyValue, out userKey))
            {
                return new ProviderKeyResponse(null, $"Invalid UserClaimId {currentUserKeyValue}");
            }

            var appUser = ctx.AppUsers
                .SingleOrDefault(u => u.Key == userKey);

            if (appUser == null)
            {
                return new ProviderKeyResponse(null, $"Invalid user {userKey}");
            }

            var book = ctx.Books
                .AsNoTracking()
                .SingleOrDefault(b => b.Key == dto.BookKey);

            if (book == null)
            {
                return new ProviderKeyResponse(null, $"Invalid book {dto.BookKey}");
            }

            var purchaseAmount = book.Price * dto.RequestedQuantity;

            if (appUser.Wallet < purchaseAmount)
            {
                return new ProviderKeyResponse(null, $"Insufficent funds in user wallet");
            }

            if (!bookProvider.RemoveFromStock(dto.BookKey, dto.RequestedQuantity))
            {
                return new ProviderKeyResponse(null, $"Insufficient book stock");
            }

            // valid purchase, stock available
            appUser.Wallet -= purchaseAmount;

            // TODO move to new provider ...

            appUser.BookPurchases.Add(new BookPurchase
            {
                PurchaseDate = DateTime.Today,
                PurchaseQuantity = dto.RequestedQuantity,
                BookKey = dto.BookKey
            });

            ctx.SaveChanges();

            return new ProviderKeyResponse(userKey, string.Empty);
        }
    }
}
