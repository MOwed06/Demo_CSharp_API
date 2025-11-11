using BigBooks.API.Entities;
using BigBooks.API.Interfaces;
using BigBooks.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Claims;

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

        /// <summary>
        /// extract userKey from claim
        /// refer to AuthService.cs, GenerateToken(), user key embedded in token
        /// </summary>
        /// <param name="currentUserKeyValue"></param>
        /// <param name="bookKey"></param>
        /// <param name="requestedQuantity"></param>
        /// <returns></returns>
        public ProviderKeyResponse PurchaseBooks(string currentUserKeyValue, BookPurchaseDto dto)
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
            appUser.UserBookIds.Add(dto.BookKey);
            ctx.SaveChanges();

            return new ProviderKeyResponse(userKey, string.Empty);
        }
    }
}
