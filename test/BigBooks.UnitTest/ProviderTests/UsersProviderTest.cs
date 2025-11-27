using BigBooks.API.Core;
using BigBooks.API.Entities;
using BigBooks.API.Models;
using BigBooks.API.Providers;
using BigBooks.UnitTest.Common;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using Moq;

namespace BigBooks.UnitTest.ProviderTests
{
    public class UsersProviderTest : BookStoreTest
    {
        private readonly UsersProvider _usersProvider;

        public UsersProviderTest()
        {
            var mockLogger = new Mock<ILogger<UsersProvider>>();
            _usersProvider = new UsersProvider(_ctx, mockLogger.Object);
        }

        [Theory]
        [InlineData("Some.User@demo.com", null, "Invalid user")]               // user does not exist
        [InlineData(CUSTOMER_2_EMAIL, 2, "")]
        public void ConfirmGetUserKeyFromToken(string userKeyValue, int? expectedKey, string expectedError)
        {
            // arrange
            InitializeDatabase();

            // act
            var observed = _usersProvider.GetUserKeyFromToken(userKeyValue);

            // assert
            Assert.Equal(expectedKey, observed.Key);

            if (observed.Key.HasValue)
            {
                Assert.Empty(observed.Error);
            }
            else
            {
                Assert.Contains(expectedError, observed.Error);
            }
        }

        [Fact]
        public void CheckUserTransactionsOrder()
        {
            // arrange
            const int USER_2_KEY = 2;

            var extraTransactions = new List<AccountTransaction>
            {
                new AccountTransaction
                {
                    Key = 3,
                    TransactionDate = DateTime.Parse("2025-03-15").Date,
                    UserKey = USER_2_KEY,
                    TransactionAmount = -26.23m,
                    TransactionConfirmation = Guid.Parse("1CC8F708-68A6-4998-AE07-92717392CD4F"),
                    BookKey = 1,
                    PurchaseQuantity = 2
                },
                new AccountTransaction
                {
                    Key = 4,
                    TransactionDate = DateTime.Parse("2025-03-16").Date,
                    UserKey = USER_2_KEY,
                    TransactionAmount = -17.17m,
                    TransactionConfirmation = Guid.Parse("AE8A1120-86E2-4CFE-8816-6EBBC273C458"),
                    BookKey = 2,
                    PurchaseQuantity = 1
                },
                new AccountTransaction
                {
                    Key = 5,
                    TransactionDate = DateTime.Parse("2025-04-11").Date,
                    UserKey = USER_2_KEY,
                    TransactionAmount = 80.00m,
                    TransactionConfirmation = Guid.Parse("637CFCF1-80EE-4B05-8F6C-8D03DD605A2C"),
                    BookKey = null,
                    PurchaseQuantity = null
                },
                new AccountTransaction
                {
                    Key = 6,
                    TransactionDate = DateTime.Parse("2025-03-11").Date,
                    UserKey = USER_2_KEY,
                    TransactionAmount = 10.00m,
                    TransactionConfirmation = Guid.Parse("68E82C2F-7239-429E-A765-33D65903A566"),
                    BookKey = null,
                    PurchaseQuantity = null
                }
            };

            // expect transations to be sorted with most recent first
            var expectedSortKeyOrder = new List<int> { 5, 4, 3, 6 };

            InitializeDatabase(extraTransactions: extraTransactions);

            // act
            var obs = _usersProvider.GetUserTransactions(USER_2_KEY);

            // assert
            var obsKeys = obs.Select(o => o.TransactionKey).ToList();

            Assert.Equal(expectedSortKeyOrder, obsKeys);
        }

        [Theory]
        [InlineData(CUSTOMER_2_EMAIL, "Duplicate UserEmail")]
        [InlineData("Wanda.Maximoff@demo.com", null)]
        public void CheckAddUser(string userEmail, string expectedError)
        {
            // arrange
            const int EXPECTED_NEXT_USER_KEY = 3;

            InitializeDatabase();

            var addDto = new UserAddUpdateDto
            {
                UserEmail = userEmail,
                UserName = "Wanda Maximoff",
                Password = "12341234",
                Wallet = 100m,
                Role = Role.Customer
            };

            // act
            var response = _usersProvider.AddUser(addDto);

            // assert
            if (string.IsNullOrEmpty(expectedError))
            {
                Assert.Equal(EXPECTED_NEXT_USER_KEY, response.Key);
                Assert.Empty(response.Error);
            }
            else
            {
                Assert.Null(response.Key);
                Assert.Contains(expectedError, response.Error);
            }
        }

        [Theory]
        [InlineData(1, CUSTOMER_2_EMAIL, "12345", "Duplicate UserEmail")]
        [InlineData(5, "SomeNew.Guy@test.com", "12345", "Account key 5 not found")]
        [InlineData(1, "SomeNew.Guy@test.com", "123", "field Password must be a")]
        [InlineData(1, "", "12345", "UserEmail field is required")]
        public void CheckUpdateAccountRejected(int key, string email, string password, string expectedError)
        {
            // arrange
            InitializeDatabase();

            var patchDoc = new JsonPatchDocument<UserAddUpdateDto>();
            patchDoc.Replace(p => p.UserEmail, email);
            patchDoc.Replace(p => p.Password, password);

            // act
            var obs = _usersProvider.UpdateAccount(key, patchDoc);

            // assert
            Assert.Null(obs.Key);
            Assert.Contains(expectedError, obs.Error);
        }

        [Theory]
        [InlineData("Some.Funny.Name@test.com", "SFName", 45.23)]
        [InlineData("SF.Name@test.com", "Some Very Long Name For Person", 545.23)]
        public void CheckUpdateAccountValid(string email, string userName, decimal wallet)
        {
            // arrange
            const int MODIFY_USER_KEY = 2;

            InitializeDatabase();

            var patchDoc = new JsonPatchDocument<UserAddUpdateDto>();
            patchDoc.Replace(p => p.UserEmail, email);
            patchDoc.Replace(p => p.UserName, userName);
            patchDoc.Replace(p => p.Wallet, wallet);

            // act
            var obs = _usersProvider.UpdateAccount(MODIFY_USER_KEY, patchDoc);
            var updatedUser = _usersProvider.GetUser(MODIFY_USER_KEY);

            // assert
            Assert.Equal(email, updatedUser.UserEmail);
            Assert.Equal(userName, updatedUser.UserName);
            Assert.Equal(wallet.ToString("C"), updatedUser.Wallet);
        }

        [Theory]
        [InlineData(null, new int[] { 1, 2, 3 })]
        [InlineData(true, new int[] { 1, 2 })]
        [InlineData(false, new int[] { 3 })]
        public void GetUserList(bool? activeFilter, int[] expectedKeys)
        {
            // arrange
            var extraUsers = new List<AppUser>
            {
                new AppUser
                {
                    Key = 3,
                    UserName = "Isaiah Chapman",
                    UserEmail = "Isaiah.Chapman@test.com",
                    Password = ApplicationConstant.USER_PASSWORD,
                    IsActive = false,
                    Role = Role.Customer,
                    Wallet = 45.31m
                }
            };

            InitializeDatabase(extraAppUsers: extraUsers);

            // act
            var obs = _usersProvider.GetUsers(activeFilter);
            var obsUserKeys = obs.Select(u => u.Key).ToList();

            // assert
            Assert.Equal(expectedKeys.ToList(), obsUserKeys);
        }

        [Theory]
        [InlineData(1, "User01DetailsDto.json")]
        [InlineData(6, null)]
        public void CheckGetUser(int userKey, string referenceFile)
        {
            // arrange
            InitializeDatabase();

            // act
            var observed = _usersProvider.GetUser(userKey);

            // assert
            if (referenceFile != null)
            {
                var expected = GetObjectFromSupportJsonFile<UserDetailsDto>(referenceFile);
                CheckObjectsEquivalent(expected, observed);
            }
            else
            {
                // invalid user
                Assert.Null(observed);
            }
        }

        [Fact]
        public void CheckGetCurrentUserException()
        {
            // arrange
            var expectedError = @"Invalid user Some.Guy@test.com";

            var invalidUserEmail = "Some.Guy@test.com";

            // act & assert
            var ex = Assert.Throws<Exception>(() => _usersProvider.GetCurrentUserDetails(invalidUserEmail));

            Assert.Equal(expectedError, ex.Message);
        }
    }
}
