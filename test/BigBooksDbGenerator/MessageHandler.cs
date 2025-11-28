using BigBooks.API.Authentication;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace BigBooksDbGenerator
{
    internal abstract class MessageHandler
    {
        protected const string ACCOUNTS_URI = @"https://localhost:7119/api/accounts";
        protected const string BOOKS_URI = @"https://localhost:7119/api/books";
        protected const string AUTH_URI = @"https://localhost:7119/api/authentication/authenticate";
        protected const string TRANSACTIONS_URI = @"https://localhost:7119/api/transactions/purchase";
        protected async Task<string> GetAuthToken(HttpClient client, AuthRequest authRequest)
        {
            var reqBody = JsonConvert.SerializeObject(authRequest);

            var message = new HttpRequestMessage(method: HttpMethod.Post,
                requestUri: AUTH_URI);
            message.Content = new StringContent(reqBody, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(message);

            var responseBody = await response.Content.ReadAsStringAsync();

            var authResponse = JsonConvert.DeserializeObject<AuthResponse>(responseBody);

            return authResponse.Token;
        }

        protected async Task<T> SendMessage<T>(HttpClient client,
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

            var response = await client.SendAsync(message);

            if (HttpStatusCode.OK != response.StatusCode)
            {
                var errorMsg = $"Message Failure, {uri}";

                if (body != null)
                {
                    var reqBody = JsonConvert.SerializeObject(body);
                    errorMsg += "\n" + reqBody.ToString();
                }

                throw new Exception(errorMsg);
            }

            var responseData = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(responseData);
        }
    }
}
