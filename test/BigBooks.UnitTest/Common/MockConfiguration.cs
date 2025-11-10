using Microsoft.Extensions.Configuration;

namespace BigBooks.UnitTest.Common
{
    public class MockConfiguration
    {
        public IConfiguration Config { get; }

        public MockConfiguration()
        {
            var configData = new Dictionary<string, string>
            {
                { "Authentication:SecretForKey", "z3f8zmAGOIbmUggCAi20xNIctPmrUw2OSPI269pieM4=" },
                { "Authentication:Issuer", "test"},
                { "Authentication:Audience", "BigBooksApi" }
            };

            Config = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();
        }
    }
}
