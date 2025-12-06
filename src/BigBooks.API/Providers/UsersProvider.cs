using BigBooks.API.Core;
using BigBooks.API.Entities;
using BigBooks.API.Interfaces;
using BigBooks.API.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace BigBooks.API.Providers
{
    public class UsersProvider(IDbContextFactory<BigBookDbContext> dbContextFactory,
        ILogger<UsersProvider> logger) : BaseProvider(dbContextFactory), IUsersProvider
    {
        public UserDetailsDto GetUser(int key)
        {
            logger.LogDebug("GetUser, {0}", key);

            using (var ctx = dbContextFactory.CreateDbContext())
            {
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

                return new UserDetailsDto
                {
                    Key = key,
                    UserEmail = appUser.UserEmail,
                    UserName = appUser.UserName,
                    IsActive = appUser.IsActive,
                    Role = appUser.Role.ToString(),
                    Wallet = appUser.Wallet.ToString("C"),
                    Transactions = GetUserTransactions(key)
                };
            }
        }

        internal List<TransactionOverviewDto> GetUserTransactions(int userKey)
        {
            using (var ctx = dbContextFactory.CreateDbContext())
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
                    PurchaseBookKey = t.Book?.Key,
                    PurchaseQuantity = t.PurchaseQuantity
                })
                .OrderByDescending(t => t.TransactionDate)
                .ToList();
            }
        }

        public List<UserOverviewDto> GetUsers(bool? activeStatus)
        {
            logger.LogDebug("GetUsers");

            using (var ctx = dbContextFactory.CreateDbContext())
            {
                var appUsers = activeStatus.HasValue
                ? ctx.AppUsers // sort by active status
                    .AsNoTracking()
                    .Where(u => u.IsActive == activeStatus.Value)
                    .Include(u => u.Transactions)
                    .ToList()
                : ctx.AppUsers // retrieve all
                    .AsNoTracking()
                    .Include(u => u.Transactions)
                    .ToList();

                return appUsers
                    .Select(u => new UserOverviewDto
                    {
                        Key = u.Key,
                        UserEmail = u.UserEmail,
                        Role = u.Role.ToString(),
                        IsActive = u.IsActive,
                        BookCount = u.Transactions
                            .Where(t => t.BookKey != null)
                            .Select(t => t.BookKey)
                            .Distinct()
                            .Count()
                    })
                    .ToList();
            }
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
            logger.LogDebug("AddUser {0}", dto.UserEmail);

            using (var ctx = dbContextFactory.CreateDbContext())
            {
                if (IsDuplicateEmail(ctx, dto.UserEmail, null))
                {
                    // duplicate email
                    return new ProviderKeyResponse(null, $"Duplicate UserEmail {dto.UserEmail}");
                }

                var nextUser = new AppUser
                {
                    UserEmail = dto.UserEmail,
                    UserName = dto.UserName,
                    Password = dto.Password,
                    IsActive = dto.IsActive,
                    Wallet = dto.Wallet,
                    Role = dto.Role,
                    Transactions = new List<AccountTransaction>()
                };

                ctx.AppUsers.Add(nextUser);
                ctx.SaveChanges();

                return new ProviderKeyResponse(nextUser.Key, string.Empty);
            }
        }

        public ProviderKeyResponse GetUserKeyFromToken(string currentUserValue)
        {
            logger.LogDebug("GetUserKeyFromToken, {0}", currentUserValue);

            using (var ctx = dbContextFactory.CreateDbContext())
            {
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

        public ProviderKeyResponse UpdateAccount(int key, JsonPatchDocument<UserAddUpdateDto> patchDoc)
        {
            logger.LogDebug("UpdateUser, {0}", key);

            using (var ctx = dbContextFactory.CreateDbContext())
            {
                var existingAccount = ctx.AppUsers
                .AsNoTracking()
                .FirstOrDefault(b => b.Key == key);

                if (existingAccount == null)
                {
                    return new ProviderKeyResponse(null, $"Account key {key} not found");
                }

                // transform from entity to dto
                var updateDto = new UserAddUpdateDto
                {
                    UserEmail = existingAccount.UserEmail,
                    UserName = existingAccount.UserName,
                    IsActive = existingAccount.IsActive,
                    Password = existingAccount.Password,
                    Wallet = existingAccount.Wallet,
                    Role = existingAccount.Role
                };

                // transform existing object according to json patch
                patchDoc.ApplyTo(updateDto);
                // confirm transformed object obeys dto rules
                var validationCheck = ValidateDto(updateDto);
                if (!validationCheck.Valid)
                {
                    return new ProviderKeyResponse(null, validationCheck.Error);
                }

                if (IsDuplicateEmail(ctx, updateDto.UserEmail, key))
                {
                    return new ProviderKeyResponse(null, $"Duplicate UserEmail {updateDto.UserEmail}");
                }

                // apply updates
                var modifiedAccount = ctx.AppUsers.Single(u => u.Key == key);
                modifiedAccount.UserEmail = updateDto.UserEmail;
                modifiedAccount.UserName = updateDto.UserName;
                modifiedAccount.IsActive = updateDto.IsActive;
                modifiedAccount.Password = updateDto.Password;
                modifiedAccount.Wallet = updateDto.Wallet;
                modifiedAccount.Role = updateDto.Role;

                ctx.SaveChanges();
                return new ProviderKeyResponse(key, string.Empty);
            }
        }

        private bool IsDuplicateEmail(BigBookDbContext ctx,
            string emailValue,
            int? existingUserKey)
        {
            // for update request, exclude existing user's email in duplicate check
            return ctx.AppUsers
                .Where(u => u.Key != existingUserKey)
                .Any(u => u.UserEmail.ToLower() == emailValue.ToLower());
        }

        public bool IsUserActive(int key)
        {
            using (var ctx = dbContextFactory.CreateDbContext())
            {
                return ctx.AppUsers.Single(u => u.Key == key).IsActive;
            }
        }
    }
}
