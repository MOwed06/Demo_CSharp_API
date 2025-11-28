using BigBooks.API.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Collections;
using System.Net.Http.Headers;
using System.Text;

namespace BigBooks.IntegrationTest.Common
{
    public abstract class BookIntegrationTest : IDisposable
    {
        protected const string AUTH_URI = @"/api/authentication/authenticate";
        protected const string ACCOUNT_LIST_URI = @"/api/accounts/list";
        protected const string BOOKS_URI = @"/api/books";
        protected const string BOOK_3_GET_URI = @"/api/books/3";

        // hard timeout enforced on message response instead of cancelation token
        private const double MESSAGE_TIMEOUT_SEC = 2.5;

        // this database content established by BigBooksDbContent.cs seed data
        protected const string ADMIN_USER_EMAIL = "Bruce.Wayne@demo.com";
        protected const string CUSTOMER_USER_EMAIL = "Celeste.Cadwell@demo.com";
        protected const string BOOK_3_TITLE = "Too Many Frogs";

        protected readonly BigBookWebAppFactory _appFactory;
        protected readonly HttpClient _client;

        private readonly ITestOutputHelper _output;

        protected BookIntegrationTest(ITestOutputHelper output)
        {
            _output = output;

            _appFactory = new BigBookWebAppFactory();
            _client = _appFactory
                .CreateClient(new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false
                });
            _client.Timeout = TimeSpan.FromSeconds(MESSAGE_TIMEOUT_SEC);
        }

        protected async Task<string> GetAuthToken(AuthRequest authRequest)
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

        protected async Task<HttpResponseMessage> SendMessage(
            string uri,
            HttpMethod method,
            string token,
            object body)
        {
            var message = new HttpRequestMessage(method: method,
                requestUri: uri);
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (body != null)
            {
                var reqBody = JsonConvert.SerializeObject(body);
                message.Content = new StringContent(reqBody, Encoding.UTF8, "application/json");
            }

            return await _client.SendAsync(message);
        }

        protected async Task<T> ReadResponseContent<T>(HttpResponseMessage response)
        {
            var responseData = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseData);
        }

        protected void WriteToOutput(object dataValue)
        {
            var dataOuptut = (dataValue is IEnumerable)
                ? string.Join(", ", dataValue)
                : dataValue.ToString();

            _output.WriteLine(dataOuptut);
        }

        public void Dispose()
        {
            _client?.Dispose();
            _appFactory.Dispose();
        }
    }
}
