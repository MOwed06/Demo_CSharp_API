using BigBooks.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DummyConsole2
{
    class Program
    {
        static async Task Main(string[] args)
        {
            const string DB_CONNECTION = @"Data Source=C:\GitHub\Demo_CSharp_API\src\BigBooks.API\BigBooks.db";

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<BigBookDbContext>(
                        dbContextOptions => dbContextOptions.UseSqlite(DB_CONNECTION));

                    services.AddTransient<IDemoService, DemoService>();
                    services.AddHostedService<ConsoleAppService>();
                })
                .Build();

            await host.RunAsync(); // This will start the hosted service
        }
    }
}