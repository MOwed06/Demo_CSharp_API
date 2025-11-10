using BigBooks.API.Authentication;
using BigBooks.API.Core;
using BigBooks.API.Entities;
using BigBooks.API.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BigBooks.IntegrationTest
{
    public class MessageTest : IDisposable
    {
        private readonly BigBookWebAppFactory _appFactory;
        private readonly HttpClient? _client;

        public MessageTest()
        {
            _appFactory = new BigBookWebAppFactory();
            _client = _appFactory
                .CreateClient(new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false
                });
        }

        [Fact]
        public async Task TestMe()
        {
            // arrange
            var authRequest = new AuthRequest
            {
                UserId = "Bella.Barnes@demo.com",
                Password = "N0tV3ryS3cret"
            };

            var jsonBody = JsonConvert.SerializeObject(authRequest);

            var messageContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/authentication/authenticate", messageContent);

            var responseBody = await response.Content.ReadAsStringAsync();

            var authResponse = JsonConvert.DeserializeObject<AuthResponse>(responseBody);

            var requestBook = new HttpRequestMessage(method: HttpMethod.Get,
                requestUri: "/api/book/3");
            requestBook.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.Token);

            var responseBook = await _client.SendAsync(requestBook);

            var responseBookBody = await responseBook.Content.ReadAsStringAsync();

            var bookDto = JsonConvert.DeserializeObject<BookDetailsDto>(responseBookBody);

            Assert.NotNull(responseBookBody);

            Assert.Equal("Too Many Frogs", bookDto.Title);

            // act


            // assert



        }

        public void Dispose()
        {
            _client?.Dispose();
            _appFactory.Dispose();
        }
    }
}
