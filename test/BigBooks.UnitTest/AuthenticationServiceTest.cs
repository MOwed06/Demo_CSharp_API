using BigBooks.API.Authentication;
using BigBooks.API.Core;
using BigBooks.API.Services;
using BigBooks.UnitTest.Common;
using Microsoft.Extensions.Logging;
using Moq;

namespace BigBooks.UnitTest
{
    public class AuthenticationServiceTest : BookStoreTest
    {
        private readonly AuthService _authService;

        public AuthenticationServiceTest() : base()
        {
            var mockLogger = new Mock<ILogger<AuthService>>();

            var mockCfg = new MockConfiguration();

            _authService = new AuthService(
                config: mockCfg.Config,
                ctx: _ctx,
                logger: mockLogger.Object);
        }

        [Theory]
        [InlineData("Bruce.Banner@test.com", ApplicationConstant.USER_PASSWORD, null)]
        [InlineData("Bruce.Banner@test.com", "SomePassword", "Invalid password")]
        [InlineData("Some.Person@test.com", ApplicationConstant.USER_PASSWORD, "User not found")]
        public void GenerateTokenCheck(string userId, string password, string? expectedError)
        {
            // arrange
            InitializeDatabase();

            var authRequest = new AuthRequest
            {
                UserId = userId,
                Password = password
            };

            // act
            var response = _authService.GenerateToken(authRequest);

            // assert
            if (string.IsNullOrEmpty(expectedError))
            {
                Assert.NotNull(response.Token);
                Assert.True(response.Token.Length > 400);
                Assert.Empty(response.Error);
            }
            else
            {
                Assert.Null(response.Token);
                Assert.Contains(expectedError, response.Error);
            }
        }
    }
}
