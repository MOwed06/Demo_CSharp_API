using BigBooks.API.Authentication;
using BigBooks.API.Core;
using BigBooks.API.Models;
using BigBooks.IntegrationTest.Common;
using DataMaker;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

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

            string responseToken = await GetAuthToken(authRequest);

            var requestBook = new HttpRequestMessage(method: HttpMethod.Get,
                requestUri: BOOK_3_GET_URI);
            requestBook.Headers.Authorization = new AuthenticationHeaderValue("Bearer", responseToken);

            // act
            var bookResponse = await _client.SendAsync(requestBook);

            var responseBookBody = await bookResponse.Content.ReadAsStringAsync();

            var bookDto = JsonConvert.DeserializeObject<BookDetailsDto>(responseBookBody);

            // asset
            WriteToOutput(responseBookBody);
            Assert.NotNull(responseBookBody);
            Assert.Equal(BOOK_3_TITLE, bookDto.Title);
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

            string responseToken = await GetAuthToken(authRequest);

            // act
            var response = await GetAccounts(responseToken);

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

            string responseToken = await GetAuthToken(authRequest);

            // act
            var response = await GetAccounts(responseToken);

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

            string responseToken = await GetAuthToken(authRequest);

            // act
            var response = await AddBookRequest(responseToken, bookAddDto);

            // assert
            WriteToOutput(response.StatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseBody = await response.Content.ReadAsStringAsync();
            WriteToOutput(responseBody);

            var observedBook = JsonConvert.DeserializeObject<BookDetailsDto>(responseBody);
            Assert.Equal(bookAddDto.Title, observedBook.Title);
            Assert.Equal(bookAddDto.Author, observedBook.Author);
            Assert.Equal(bookAddDto.Isbn.ToString("D").ToUpper(), observedBook.Isbn);
        }

        private async Task<HttpResponseMessage> AddBookRequest(string responseToken,
            BookAddUpdateDto dto)
        {
            var reqBody = JsonConvert.SerializeObject(dto);

            var message = new HttpRequestMessage(method: HttpMethod.Post,
                requestUri: BOOKS_URI);
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", responseToken);
            message.Content = new StringContent(reqBody, Encoding.UTF8, "application/json");

            return await _client.SendAsync(message);
        }

        private async Task<HttpResponseMessage> GetAccounts(string responseToken)
        {
            var requestUsers = new HttpRequestMessage(method: HttpMethod.Get,
                requestUri: ACCOUNT_LIST_URI);
            requestUsers.Headers.Authorization = new AuthenticationHeaderValue("Bearer", responseToken);

            return await _client.SendAsync(requestUsers);
        }
    }
}
