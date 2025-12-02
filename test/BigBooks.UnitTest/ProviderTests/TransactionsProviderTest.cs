using BigBooks.API.Core;
using BigBooks.API.Entities;
using BigBooks.API.Models;
using BigBooks.API.Providers;
using BigBooks.UnitTest.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace BigBooks.UnitTest.ProviderTests
{
    public class TransactionsProviderTest : BookStoreTest
    {
        private readonly TransactionsProvider _transactionsPrv;
        private readonly UsersProvider _usersPrv;

        private const int ACTIVE_CUSTOMER_KEY = 3;
        private const int INACTIVE_CUSTOMER_KEY = 4;
        private const string ACTIVE_CUSTOMER_EMAIL = "Zachary.Zimmer@demo.com";
        private const string INACTIVE_CUSTOMER_EMAIL = "Matthew.Ferguson@demo.com";
        private const decimal CUSTOMER_WALLET = 40.0m;

        private const int RARE_BOOK_KEY = 3;
        private const int PRE_RELEASE_BOOK_KEY = 4;

        public TransactionsProviderTest()
        {
            var mockPurchasePrvLogger = new Mock<ILogger<TransactionsProvider>>();
            var mockBookPrvLogger = new Mock<ILogger<BooksProvider>>();
            var mockUsersPrvLogger = new Mock<ILogger<UsersProvider>>();

            var booksPrv = new BooksProvider(_ctx, mockBookPrvLogger.Object);
            _usersPrv = new UsersProvider(_ctx, mockUsersPrvLogger.Object);

            _transactionsPrv = new TransactionsProvider(
                ctx: _ctx,
                booksProvider: booksPrv,
                usersProvider: _usersPrv,
                logger: mockPurchasePrvLogger.Object);

            var extraBooks = new List<Book>()
            {
                new Book
                {
                    Key = RARE_BOOK_KEY,
                    Title = "Some Exclusive Title",
                    Author = "Some Fancy Person",
                    Isbn = Guid.Parse("C352E91B-80F4-403C-B7E5-860BA52B8F99"),
                    Genre = Genre.Childrens,
                    Price = 13.42m,
                    StockQuantity = 1
                },
                new Book
                {
                    Key = PRE_RELEASE_BOOK_KEY,
                    Title = "Some Future Title",
                    Author = "Some Fancy Person",
                    Isbn = Guid.Parse("C352E91B-80F4-403C-B7E5-860BA52B8F99"),
                    Genre = Genre.Childrens,
                    Price = 11.42m,
                    StockQuantity = 0
                }
            };

            var extraUsers = new List<AppUser>
            {
                new AppUser
                {
                    Key = ACTIVE_CUSTOMER_KEY,
                    UserName = "Zachary Zimmer",
                    UserEmail = ACTIVE_CUSTOMER_EMAIL,
                    IsActive = true,
                    Wallet = CUSTOMER_WALLET,
                    Password = ApplicationConstant.USER_PASSWORD
                },
                new AppUser
                {
                    Key = INACTIVE_CUSTOMER_KEY,
                    UserName = "M. Ferguson",
                    UserEmail = INACTIVE_CUSTOMER_EMAIL,
                    IsActive = false,
                    Wallet = CUSTOMER_WALLET,
                    Password = ApplicationConstant.USER_PASSWORD
                }
            };

            var extraTransactions = new List<AccountTransaction>
            {
                new AccountTransaction
                {
                    Key = 3,
                    TransactionDate = DateTime.Parse("2025-03-15").Date,
                    UserKey = ACTIVE_CUSTOMER_KEY,
                    TransactionAmount = 25.00m,
                    TransactionConfirmation = Guid.Parse("A139F559-742A-4ED1-8B0F-F9C9D3A2264A"),
                    BookKey = null,
                    PurchaseQuantity = null
                }
            };

            InitializeDatabase(
                extraBooks: extraBooks,
                extraAppUsers: extraUsers,
                extraTransactions: extraTransactions);
        }

        /// <summary>
        /// note that book 2 has cost of 17.11 and stock of 6
        /// </summary>
        /// <param name="bookKey"></param>
        /// <param name="reqQuantity"></param>
        /// <param name="expectedError"></param>
        [Theory]
        [InlineData(2, 7, "Insufficent funds in user wallet")]
        [InlineData(RARE_BOOK_KEY, 2, "Insufficient book stock")] // only 1 book available
        [InlineData(PRE_RELEASE_BOOK_KEY, 1, "Insufficient book stock")] // no books in stock
        [InlineData(2, 3, "Insufficent funds in user wallet")]
        [InlineData(5, 1, "Invalid book")]  // book 5 does not exist
        public void CheckPurchaseBooksInvalid(int bookKey, int reqQuantity, string expectedError)
        {
            // arrange
            var purchaseDto = new PurchaseRequestDto
            {
                BookKey = bookKey,
                RequestedQuantity = reqQuantity
            };

            // act
            var response = _transactionsPrv.PurchaseBooks(ACTIVE_CUSTOMER_EMAIL, purchaseDto);

            // assert
            Assert.Contains(expectedError, response.Error);
            Assert.Null(response.Key);
        }

        [Fact]
        public void CheckInactiveUserPurchaseDenied()
        {
            // arrange
            const string EXPECTED_ERROR = "User is deactivated";

            var purchaseDto = new PurchaseRequestDto
            {
                BookKey = 1,
                RequestedQuantity = 1
            };

            // act
            var response = _transactionsPrv.PurchaseBooks(INACTIVE_CUSTOMER_EMAIL, purchaseDto);

            // assert
            Assert.Contains(EXPECTED_ERROR, response.Error);
            Assert.Null(response.Key);
        }

        [Theory]
        [InlineData(1, 1, 28.58, 16)]  // book 1, stock = 17, cost = 11.42
        [InlineData(1, 2, 17.16, 15)]    // book 1, stock = 17, cost = 11.42
        [InlineData(2, 1, 22.89, 5)]     // book 2, stock = 6, cost = 17.11
        [InlineData(3, 1, 26.58, 0)]     // book 3, stock = 1, cost = 13.42
        public void CheckPurchaseBooksValid(int bookKey, int reqQuantity, decimal expectedWallet, int expectedStock)
        {
            // arrange
            var purchaseDto = new PurchaseRequestDto
            {
                BookKey = bookKey,
                RequestedQuantity = reqQuantity
            };

            // act
            var response = _transactionsPrv.PurchaseBooks(ACTIVE_CUSTOMER_EMAIL, purchaseDto);

            var observedUser = _ctx.AppUsers
                .AsNoTracking()
                .Include(u => u.Transactions)
                .Single(u => u.Key == response.Key.Value);

            var observedBook = _ctx.Books
                .AsNoTracking()
                .Single(b => b.Key == bookKey);

            // assert
            var obsUserBookKeys = observedUser.Transactions
                .Where(u => u.BookKey != null)
                .Select(u => u.BookKey)
                .ToHashSet();

            Assert.Contains(bookKey, obsUserBookKeys); // book now associated with user
            Assert.Equal(expectedStock, observedBook.StockQuantity);
            Assert.Equal(expectedWallet, observedUser.Wallet);
        }

        [Fact]
        public void CheckPurchaseTransactionCreated()
        {
            // arrange
            var expectedAmount = -34.22m;   // $17.11 x 2 books

            var purchaseDto = new PurchaseRequestDto
            {
                BookKey = 2,
                RequestedQuantity = 2
            };

            // act
            var response = _transactionsPrv.PurchaseBooks(ACTIVE_CUSTOMER_EMAIL, purchaseDto);
            var obsUser = _usersPrv.GetUser(response.Key.Value); // get user info
            var obsTransaction = obsUser.Transactions.First();  // expect most recent is first

            // assert
            Assert.Equal(expectedAmount, obsTransaction.TransactionAmount);
        }

        [Theory]
        [InlineData("Some.Random.Person@test.com", "Invalid user")]
        [InlineData(INACTIVE_CUSTOMER_EMAIL, "User is deactivated")]
        public void CheckDepositError(string currentUserValue, string expectedError)
        {
            // arrange
            var dto = new AccountDepositDto
            {
                Amount = 75.0m,
                Confirmation = Guid.Parse("7DEAF897-921D-461A-B1E1-E1CDC51E11CC")
            };

            // act
            var obs = _transactionsPrv.Deposit(currentUserValue, dto);

            // assert
            Assert.Null(obs.Key);
            Assert.Contains(expectedError, obs.Error);
        }

        [Fact]
        public void CheckDepositValid()
        {
            // arrange
            const decimal EXPECTED_WALLET = 115m; // $40 + $75 deposit

            var dto = new AccountDepositDto
            {
                Amount = 75.0m,
                Confirmation = Guid.Parse("7DEAF897-921D-461A-B1E1-E1CDC51E11CC")
            };

            // act
            var obs = _transactionsPrv.Deposit(ACTIVE_CUSTOMER_EMAIL, dto);
            var obsWallet = _ctx.AppUsers
                .SingleOrDefault(u => u.Key == obs.Key)?.Wallet;

            // assert
            Assert.Equal(ACTIVE_CUSTOMER_KEY, obs.Key);
            Assert.Empty(obs.Error);
            Assert.Equal(EXPECTED_WALLET, obsWallet);
        }
    }
}
