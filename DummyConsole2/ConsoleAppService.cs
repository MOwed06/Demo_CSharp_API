using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyConsole2
{
    internal class ConsoleAppService : IHostedService
    {
        private readonly IDemoService _demoService;
        private readonly IHostApplicationLifetime _appLifetime;

        public ConsoleAppService(IDemoService demoService, IHostApplicationLifetime appLifetime)
        {
            _demoService = demoService;
            _appLifetime = appLifetime;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Hello, World");
            _demoService.DoSomething();
            _appLifetime.StopApplication();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Goodbye, Cruel World");
            return Task.CompletedTask;
        }
    }
}
