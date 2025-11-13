using BigBooks.API.Authentication;
using BigBooks.API.Core;
using BigBooks.API.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace BigBooks.IntegrationTest
{
    public class MessageTest : IDisposable
    {
        private readonly BigBookWebAppFactory _appFactory;
        private readonly HttpClient? _client;

        private const string AUTH_URI = @"/api/authentication/authenticate";
        private const string USERS_URI = @"/api/accounts";
        private const string BOOK_3_GET_URI = @"/api/books/3";

        private const double MESSAGE_TIMEOUT_SEC = 2.5;       

        // this database content established by BigBooksDbContent.cs seed data
        private const string ADMIN_USER_EMAIL = "Bruce.Wayne@demo.com";
        private const string CUSTOMER_USER_EMAIL = "Celeste.Cadwell@demo.com";
        private const string BOOK_3_TITLE = "Too Many Frogs";

        public MessageTest()
        {
            _appFactory = new BigBookWebAppFactory();
            _client = _appFactory
                .CreateClient(new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false
                });
            _client.Timeout = TimeSpan.FromSeconds(MESSAGE_TIMEOUT_SEC);
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
            Assert.NotNull(responseBookBody);
            Assert.Equal(BOOK_3_TITLE, bookDto.Title);
        }

        /// <summary>
        /// Confirm user with Admin role can get users
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ConfirmAdminRoleGetUsersAccess()
        {
            // arrange
            var authRequest = new AuthRequest
            {
                UserId = ADMIN_USER_EMAIL,
                Password = ApplicationConstant.USER_PASSWORD
            };

            string responseToken = await GetAuthToken(authRequest);

            // act
            var response = await GetUserList(responseToken);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        /// <summary>
        /// Confirm user with Customer role denied get users
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ConfirmCustomerRoleGetUsersNoAccess()
        {
            // arrange
            var authRequest = new AuthRequest
            {
                UserId = CUSTOMER_USER_EMAIL,
                Password = ApplicationConstant.USER_PASSWORD
            };

            string responseToken = await GetAuthToken(authRequest);

            // act
            var response = await GetUserList(responseToken);

            // assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        private async Task<HttpResponseMessage> GetUserList(string responseToken)
        {
            var requestUsers = new HttpRequestMessage(method: HttpMethod.Get,
                requestUri: USERS_URI);
            requestUsers.Headers.Authorization = new AuthenticationHeaderValue("Bearer", responseToken);

            return await _client.SendAsync(requestUsers);
        }

        private async Task<string> GetAuthToken(AuthRequest authRequest)
        {
            var reqBody = JsonConvert.SerializeObject(authRequest);

            var message = new HttpRequestMessage(method: HttpMethod.Post,
                requestUri: AUTH_URI);
            message.Content = new StringContent(reqBody, Encoding.UTF8, "application/json");

            var response = await _client.SendAsync(message);

            var responseBody = await response.Content.ReadAsStringAsync();

            var authResponse = JsonConvert.DeserializeObject<AuthResponse>(responseBody);

            return authResponse.Token;
        }

        public void Dispose()
        {
            _client?.Dispose();
            _appFactory.Dispose();
        }
    }
}
