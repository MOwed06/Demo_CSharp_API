using BigBooks.API.Authentication;
using BigBooks.API.Core;
using BigBooks.API.Models;
using BigBooks.IntegrationTest.Common;
using DataMaker;
using System.Net;

namespace BigBooks.IntegrationTest
{
    public class MessageTest : BookIntegrationTest
    {
        public MessageTest(ITestOutputHelper h) : base(h)
        {
        }

        /// <summary>
        /// Request book details
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ConfirmGetBookRequest()
        {
            // arrange
            var authRequest = new AuthRequest
            {
                UserId = CUSTOMER6_EMAIL,
                Password = ApplicationConstant.USER_PASSWORD
            };

            string token = await GetAuthTokenAsync(authRequest);

            // act
            var response = await SendMessageAsync(uri: BOOK_3_GET_URI,
                method: HttpMethod.Get,
                token: token,
                body: null);

            var obs = await ReadResponseContent<BookDetailsDto>(response);

            // asset
            WriteToOutput(obs, true);
            Assert.NotNull(obs);
            Assert.Equal(BOOK_3_TITLE, obs.Title);
        }

        /// <summary>
        /// Confirm user with Admin role can get users
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ConfirmAdminRoleGetAccountsAccess()
        {
            // arrange
            const string EXPECTED_EMAIL = "Diana.Prince@demo.com";

            var authRequest = new AuthRequest
            {
                UserId = ADMIN_USER_EMAIL,
                Password = ApplicationConstant.USER_PASSWORD
            };

            string token = await GetAuthTokenAsync(authRequest);

            // act
            var response = await SendMessageAsync(uri: ACCOUNT_LIST_URI,
                method: HttpMethod.Get,
                token: token,
                body: null);

            // assert
            WriteToOutput(response.StatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseBody = await response.Content.ReadAsStringAsync();
            WriteToOutput(responseBody);
            Assert.Contains(EXPECTED_EMAIL, responseBody);
        }

        /// <summary>
        /// Confirm user with Customer role denied get users
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ConfirmCustomerRoleGetAccountsNoAccess()
        {
            // arrange
            var authRequest = new AuthRequest
            {
                UserId = CUSTOMER6_EMAIL,
                Password = ApplicationConstant.USER_PASSWORD
            };

            string token = await GetAuthTokenAsync(authRequest);

            // act
            var response = await SendMessageAsync(uri: ACCOUNT_LIST_URI,
                method: HttpMethod.Get,
                token: token,
                body: null);

            // assert
            WriteToOutput(response.StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        /// <summary>
        /// Confirm action of adding a new book
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ConfirmAddBook()
        {
            // arrange
            var authRequest = new AuthRequest
            {
                UserId = ADMIN_USER_EMAIL,
                Password = ApplicationConstant.USER_PASSWORD
            };

            // randomly generate book definition
            var bookAddDto = new BookAddUpdateDto
            {
                Title = RandomData.GenerateSentence(),
                Author = RandomData.GenerateFullName(),
                Isbn = Guid.NewGuid(),
                Description = null,
                Genre = Genre.Fiction,
                Price = RandomData.GenerateDecimal(9.0m, 21.2m, 2),
                StockQuantity = RandomData.GenerateInt(1, 13)
            };

            string token = await GetAuthTokenAsync(authRequest);

            // act
            var response = await SendMessageAsync(uri: BOOKS_URI,
                method: HttpMethod.Post,
                token: token,
                body: bookAddDto);

            // assert
            WriteToOutput(response.StatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var obs = await ReadResponseContent<BookDetailsDto>(response);

            Assert.Equal(bookAddDto.Title, obs.Title);
            Assert.Equal(bookAddDto.Author, obs.Author);
            Assert.Equal(bookAddDto.Isbn.ToString("D").ToUpper(), obs.Isbn);
        }

        /// <summary>
        /// This test has been designed to demonstrate the potential
        /// ways of leveraging the WebApplicationFactory.
        /// 
        /// Direct access to the dbContext may be atypical for a test of
        /// this kind. An additional API could have been made to inspect
        /// the available stock for the purchased book. This scenario was constructed
        /// to show that direct db query is possible in this test configuration.
        /// 
        /// The test makes an API call for a customer book purchase.
        /// The API return will be inspected to confirm the book is added to the customer.
        /// Then the database will be inspected directly to confirm the book has
        /// been removed from stock.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ConfirmBookPurchase()
        {
            // arrange
            const int PURCHASE_BOOK_KEY = 6;
            const int EXPECTED_BOOK_STOCK = 16; // available stock after user purchase
            const string EXPECTED_USER_WALLET = "$88.81"; // $100 - purchase $11.19

            var authRequest = new AuthRequest
            {
                UserId = CUSTOMER4_EMAIL,
                Password = ApplicationConstant.USER_PASSWORD
            };

            var purchaseDto = new PurchaseRequestDto
            {
                BookKey = PURCHASE_BOOK_KEY,
                RequestedQuantity = 1,
                TransactionConfirmation = Guid.NewGuid()
            };

            string token = await GetAuthTokenAsync(authRequest);

            // act
            var response = await SendMessageAsync(uri: PURCHASE_URI,
                method: HttpMethod.Post,
                token: token,
                body: purchaseDto);

            // assert
            WriteToOutput(response.StatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var obs = await ReadResponseContent<UserDetailsDto>(response);
            var obsTransaction = obs.Transactions
                .SingleOrDefault(t => t.PurchaseBookKey == PURCHASE_BOOK_KEY);

            Assert.Equal(EXPECTED_USER_WALLET, obs.Wallet); // confirm wallet adjusted
            Assert.NotNull(obsTransaction); // confirm book possessed by user

            using (var ctx = _appFactory.CreateTestDbContext())
            {
                var obsBook = ctx.Books.Single(b => b.Key == PURCHASE_BOOK_KEY);
                Assert.Equal(EXPECTED_BOOK_STOCK, obsBook.StockQuantity);
            }
        }

        /// <summary>
        /// Similar to ConfirmBookPurchase, this test will make an API call and
        /// then directly inspect the database content.
        /// 
        /// A purchase will be attempted and the rejection content will be confirmed.
        /// The user wallet and stock quantity will be confirmed as unchanged
        /// after the API call completes.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ConfirmRejectedBookPurchase()
        {
            // arrange
            const string EXPECTED_ERROR = "Insufficent funds in user wallet";
            const int CUSTOMER4_USER_KEY = 4;

            const int PURCHASE_BOOK_KEY = 6;
            const int EXPECTED_BOOK_STOCK = 17;
            const decimal EXPECTED_USER_WALLET = 100.0m;

            var authRequest = new AuthRequest
            {
                UserId = CUSTOMER4_EMAIL,
                Password = ApplicationConstant.USER_PASSWORD
            };

            var purchaseDto = new PurchaseRequestDto
            {
                BookKey = PURCHASE_BOOK_KEY,
                RequestedQuantity = 11, // 11 books x $11.19 > customer wallet
                TransactionConfirmation = Guid.NewGuid()
            };

            // act
            string token = await GetAuthTokenAsync(authRequest);

            // act
            var response = await SendMessageAsync(uri: PURCHASE_URI,
                method: HttpMethod.Post,
                token: token,
                body: purchaseDto);

            var responseBody = response.Content.ReadAsStringAsync()?.Result;

            // assert
            WriteToOutput(response.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            WriteToOutput(responseBody);
            Assert.Contains(EXPECTED_ERROR, responseBody);

            using (var ctx = _appFactory.CreateTestDbContext())
            {
                var obsBookQty = ctx.Books
                    .SingleOrDefault(b => b.Key == PURCHASE_BOOK_KEY)
                    ?.StockQuantity;
                var obsUserWallet = ctx.AppUsers
                    .SingleOrDefault(u => u.Key == CUSTOMER4_USER_KEY)?.Wallet;

                Assert.Equal(EXPECTED_BOOK_STOCK, obsBookQty);
                Assert.Equal(EXPECTED_USER_WALLET, obsUserWallet);
            }
        }
    }
}
