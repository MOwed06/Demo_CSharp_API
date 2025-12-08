using BigBooks.API.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BigBooks.IntegrationTest.Common
{
    public class BigBookWebAppFactory : WebApplicationFactory<Program>
    {
        /// <summary>
        /// Caller methods are responsible for disposing the returned DbContext
        /// </summary>
        /// <returns></returns>
        public BigBookDbContext CreateTestDbContext()
        {
            var scope = base.Services.CreateScope();
            var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BigBookDbContext>>();
            return dbFactory.CreateDbContext();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Remove existing configuration
                config.Sources.Clear();

                // Add in-memory configuration
                var inMemoryConfig = new Dictionary<string, string>
                    {
                        {"Authentication:SecretForKey", "hWrtw4e5yIlh3zvUpHzh/GdOJptjin4dIBiUfmV0ZEU="},
                        {"Authentication:Issuer", "IntegrationTest"},
                        {"Authentication:Audience", "BigBooksApiTest"}
                    };
                config.AddInMemoryCollection(inMemoryConfig);
            });

            builder.ConfigureServices(services =>
            {
                // remove standard db context
                var applicationDbContext = services
                    .SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<BigBookDbContext>));
                services.Remove(applicationDbContext);

                // build in-memory db context
                services.AddDbContextFactory<BigBookDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryTestDb");
                });

                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var dbContext = scopedServices.GetRequiredService<BigBookDbContext>();

                    // force remove any prior data
                    dbContext.Database.EnsureDeleted();
                    // Ensure the database is created (for in-memory, this initializes it)
                    dbContext.Database.EnsureCreated();
                    dbContext.SaveChanges();
                }
            });
        }
    }
}
