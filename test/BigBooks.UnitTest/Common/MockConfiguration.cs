using Microsoft.Extensions.Configuration;

namespace BigBooks.UnitTest.Common
{
    public class MockConfiguration
    {
        public IConfiguration Config { get; }

        public const string SECRET_KEY = @"z3f8zmAGOIbmUggCAi20xNIctPmrUw2OSPI269pieM4=";
        public const string ISSUER = "unittest";
        public const string AUDIENCE = "BigBooksApi";

        public MockConfiguration()
        {
            var configData = new Dictionary<string, string>
            {
                { "Authentication:SecretForKey", SECRET_KEY },
                { "Authentication:Issuer", ISSUER},
                { "Authentication:Audience", AUDIENCE }
            };

            Config = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();
        }
    }
}
