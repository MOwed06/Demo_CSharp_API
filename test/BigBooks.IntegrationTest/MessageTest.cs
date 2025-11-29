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
                UserId = CUSTOMER_USER_EMAIL,
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
            WriteToOutput(obs);
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
                UserId = CUSTOMER_USER_EMAIL,
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
    }
}
