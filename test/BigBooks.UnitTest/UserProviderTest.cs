using BigBooks.API.Core;
using BigBooks.API.Entities;
using BigBooks.API.Models;
using BigBooks.API.Providers;
using BigBooks.UnitTest.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace BigBooks.UnitTest
{
    public class UserProviderTest : BookStoreTest
    {
        private readonly UserProvider _userProvider;

        private const int CUSTOMER_KEY = 3;
        private const float CUSTOMER_WALLET = 40.0f;

        private const int RARE_BOOK_KEY = 3;
        private const int PRE_RELEASE_BOOK_KEY = 4;

        public UserProviderTest()
        {
            var mockBookPrvLogger = new Mock<ILogger<BookProvider>>();
            var mockUserPrvLogger = new Mock<ILogger<UserProvider>>();
            var bookPrv = new BookProvider(_ctx, mockBookPrvLogger.Object);

            _userProvider = new UserProvider(
                ctx: _ctx,
                bookProvider: bookPrv,
                logger: mockUserPrvLogger.Object);

            var extraBooks = new List<Book>()
            {
                new Book
                {
                    Key = RARE_BOOK_KEY,
                    Title = "Some Exclusive Title",
                    Author = "Some Fancy Person",
                    Isbn = Guid.Parse("C352E91B-80F4-403C-B7E5-860BA52B8F99"),
                    Genre = Genre.Childrens,
                    Price = 13.42f,
                    StockQuantity = 1
                },
                new Book
                {
                    Key = PRE_RELEASE_BOOK_KEY,
                    Title = "Some Future Title",
                    Author = "Some Fancy Person",
                    Isbn = Guid.Parse("C352E91B-80F4-403C-B7E5-860BA52B8F99"),
                    Genre = Genre.Childrens,
                    Price = 11.42f,
                    StockQuantity = 0
                }
            };

            var extraUsers = new List<AppUser>
            {
                new AppUser
                {
                    Key = CUSTOMER_KEY,
                    UserName = "Zachary Zimmer",
                    UserEmail = "Zachary.Zimmer@demo.com",
                    Wallet = CUSTOMER_WALLET,
                    Password = ApplicationConstant.USER_PASSWORD,
                    UserBookIds = new List<int>()
                }
            };

            InitializeDatabase(
                extraBooks: extraBooks,
                extraAppUsers: extraUsers);
        }

        /// <summary>
        /// note that book 2 has cost of 17.11 and stock of 6
        /// </summary>
        /// <param name="currentUserKey"></param>
        /// <param name="bookKey"></param>
        /// <param name="reqQuantity"></param>
        /// <param name="expectedError"></param>
        [Theory]
        [InlineData("three", 2, 1, "Invalid UserClaimId")]
        [InlineData("4", 2, 1, "Invalid user")]  // user 4 does not exist
        [InlineData("3", 5, 1, "Invalid book")]  // book 5 does not exist
        [InlineData("3", 2, 7, "Insufficent funds in user wallet")]
        [InlineData("3", RARE_BOOK_KEY, 2, "Insufficient book stock")] // only 1 book available
        [InlineData("3", PRE_RELEASE_BOOK_KEY, 1, "Insufficient book stock")] // no books in stock
        [InlineData("3", 2, 3, "Insufficent funds in user wallet")]
        public void CheckPurchaseBooksInvalid(string currentUserKey, int bookKey, int reqQuantity, string expectedError)
        {
            // arrange
            var purchaseDto = new BookPurchaseDto
            {
                BookKey = bookKey,
                RequestedQuantity = reqQuantity
            };

            // act
            var response = _userProvider.PurchaseBooks(currentUserKey, purchaseDto);

            // assert
            Assert.Contains(expectedError, response.Error);
            Assert.Null(response.Key);
        }

        [Theory]
        [InlineData(1, 1, 28.58f, 16)]  // book 1, stock = 17, cost = 11.42
        [InlineData(1, 2, 17.16f, 15)]    // book 1, stock = 17, cost = 11.42
        [InlineData(2, 1, 22.89f, 5)]     // book 2, stock = 6, cost = 17.11
        [InlineData(3, 1, 26.58f, 0)]     // book 3, stock = 1, cost = 13.42
        public void CheckPurchaseBooksValid(int bookKey, int reqQuantity, float expectedWallet, int expectedStock)
        {
            // arrange
            const string USER_KEY_VALUE = "3";

            var purchaseDto = new BookPurchaseDto
            {
                BookKey = bookKey,
                RequestedQuantity = reqQuantity
            };

            // act
            var response = _userProvider.PurchaseBooks(USER_KEY_VALUE, purchaseDto);

            var observedUser = _ctx.AppUsers
                .AsNoTracking()
                .Single(u => u.Key == response.Key.Value);

            var observedBook = _ctx.Books
                .AsNoTracking()
                .Single(b => b.Key == bookKey);

            // assert
            Assert.Contains(bookKey, observedUser.UserBookIds); // book now associated with user
            Assert.Equal(expectedStock, observedBook.StockQuantity);

            var adjustedObserved = Truncate2Digits(observedUser.Wallet);
            var adjustedExpected = Truncate2Digits(expectedWallet);
            Assert.Equal(adjustedExpected, adjustedObserved);
        }

        private float Truncate2Digits(float value)
        {
            return (float)Math.Truncate(value * 100) / 100f;
        }
    }
}
