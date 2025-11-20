using BigBooks.API.Entities;
using BigBooks.API.Interfaces;
using BigBooks.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BigBooks.API.Providers
{
    public class TransactionsProvider(BigBookDbContext ctx,
        IBooksProvider booksProvider,
        IUsersProvider usersProvider,
        ILogger<TransactionsProvider> logger) : BaseProvider, ITransactionsProvider
    {
        /// <summary>
        /// extract userKey from claim
        /// refer to AuthService.cs, GenerateToken(), user key embedded in token
        /// </summary>
        /// <param name="currentUserValue">user email from token</param>
        /// <param name="dto">purchase params</param>
        /// <returns>user key associated with purchase</returns>
        public ProviderKeyResponse PurchaseBooks(string currentUserValue, PurchaseRequestDto dto)
        {
            logger.LogDebug("PurchaseBooks, user: {0}, book: {1}, qty: {2}",
                currentUserValue,
                dto.BookKey,
                dto.RequestedQuantity);

            var userMatch = usersProvider.GetUserKeyFromToken(currentUserValue);

            if (!userMatch.Key.HasValue)
            {
                // no match to user
                return new ProviderKeyResponse(null, userMatch.Error);
            }

            var book = ctx.Books
                .AsNoTracking()
                .SingleOrDefault(b => b.Key == dto.BookKey);

            if (book == null)
            {
                return new ProviderKeyResponse(null, $"Invalid book {dto.BookKey}");
            }

            var purchaseAmount = book.Price * dto.RequestedQuantity;

            var currentUser = ctx.AppUsers
                .Include(u => u.Transactions)
                .Single(u => u.Key == userMatch.Key);

            if (currentUser.Wallet < purchaseAmount)
            {
                return new ProviderKeyResponse(null, $"Insufficent funds in user wallet");
            }

            if (!booksProvider.RemoveFromStock(dto.BookKey, dto.RequestedQuantity))
            {
                return new ProviderKeyResponse(null, $"Insufficient book stock");
            }

            // valid purchase, stock available
            currentUser.Wallet -= purchaseAmount;

            currentUser.Transactions.Add(new AccountTransaction
            {
                TransactionAmount = -purchaseAmount,
                TransactionDate = DateTime.Now,
                TransactionConfirmation = dto.TransactionConfirmation,
                BookKey = dto.BookKey,
                PurchaseQuantity = dto.RequestedQuantity
            });

            ctx.SaveChanges();

            return new ProviderKeyResponse(currentUser.Key, string.Empty);
        }

        public ProviderKeyResponse Deposit(string currentUserValue, AccountDepositDto dto)
        {
            logger.LogDebug("Deposit, user: {0}, Amount: {2}",
                currentUserValue,
                dto.Amount);

            var userMatch = usersProvider.GetUserKeyFromToken(currentUserValue);

            if (!userMatch.Key.HasValue)
            {
                // no match to user
                return new ProviderKeyResponse(null, userMatch.Error);
            }

            var currentUser = ctx.AppUsers
                .Include(u => u.Transactions)
                .Single(u => u.Key == userMatch.Key);

            currentUser.Wallet += dto.Amount;

            currentUser.Transactions.Add(new AccountTransaction
            {
                TransactionDate = DateTime.Now,
                TransactionConfirmation = dto.Confirmation,
                TransactionAmount = dto.Amount,
                BookKey = null,
                PurchaseQuantity = null
            });

            ctx.SaveChanges();

            return new ProviderKeyResponse(currentUser.Key, string.Empty);
        }
    }
}
