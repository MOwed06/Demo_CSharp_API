using BigBooks.API.Providers;
using BigBooks.UnitTest.Common;
using Microsoft.Extensions.Logging;
using Moq;

namespace BigBooks.UnitTest.ProviderTests
{
    public class UsersProvidersTest : BookStoreTest
    {
        private readonly UsersProvider _usersProvider;

        public UsersProvidersTest()
        {
            var mockUsersPrvLogger = new Mock<ILogger<UsersProvider>>();

            _usersProvider = new UsersProvider(
                ctx: _ctx,
                logger: mockUsersPrvLogger.Object);

            InitializeDatabase();
        }

        [Theory]
        [InlineData("Some.User@demo.com", null, "Invalid user")]               // user does not exist
        [InlineData(CUSTOMER_2_EMAIL, 2, "")]
        public void ConfirmGetUserKeyFromToken(string userKeyValue, int? expectedKey, string expectedError)
        {
            // arrange

            // act
            var observed = _usersProvider.GetUserKeyFromToken(userKeyValue);

            // assert
            Assert.Equal(expectedKey, observed.Key);

            if (observed.Key.HasValue)
            {
                Assert.Empty(observed.Error);
            }
            else
            {
                Assert.Contains(expectedError, observed.Error);
            }
        }
    }
}
