using BigBooks.API.Authentication;
using BigBooks.API.Core;
using BigBooks.API.Services;
using BigBooks.UnitTest.Common;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;

namespace BigBooks.UnitTest.AuthenticationTests
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
                dbContextFactory: TestContextFactory,
                logger: mockLogger.Object);
        }

        [Theory]
        [InlineData("Bruce.Banner@test.com", ApplicationConstant.USER_PASSWORD, null)]
        [InlineData("Bruce.Banner@test.com", "SomePassword", "Invalid password")]
        [InlineData("Some.Person@test.com", ApplicationConstant.USER_PASSWORD, "User not found")]
        [InlineData(null, ApplicationConstant.USER_PASSWORD, "User not found")]
        [InlineData("", ApplicationConstant.USER_PASSWORD, "User not found")]
        public void GenerateTokenCheck(string userId, string password, string expectedError)
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

        [Fact]
        public void CheckTokenAuthentication()
        {
            // arrange
            InitializeDatabase();

            var authRequest = new AuthRequest
            {
                UserId = "Bruce.Banner@test.com",
                Password = ApplicationConstant.USER_PASSWORD
            };

            // act
            var response = _authService.GenerateToken(authRequest);

            // assert
            SecurityToken validatedToken;
            var tokenParams = GetValidationParameters();
            var tokenHandler = new JwtSecurityTokenHandler();

            // if validate token fails, exception thrown, test implicitly fails
            var principal = tokenHandler.ValidateToken(token: response.Token,
                validationParameters: tokenParams,
                out validatedToken);

            Assert.True(principal.Identity.IsAuthenticated);
        }

        private TokenValidationParameters GetValidationParameters(double? clockOffsetHours = null)
        {
            TimeSpan clockSkew = clockOffsetHours.HasValue
                ? TimeSpan.FromHours(clockOffsetHours.Value)
                : TimeSpan.Zero;

            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(MockConfiguration.SECRET_KEY)),
                ValidateIssuer = true,
                ValidIssuer = MockConfiguration.ISSUER,
                ValidateAudience = true,
                ValidAudience = MockConfiguration.AUDIENCE,
                ValidateLifetime = true,
                ClockSkew = clockSkew
            };
        }
    }
}
