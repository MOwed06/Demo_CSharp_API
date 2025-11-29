using BigBooks.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyConsole2
{
    public class DemoService : IDemoService
    {
        private readonly BigBookDbContext _ctx;

        public DemoService(BigBookDbContext ctx)
        {
            _ctx = ctx;
        }

        public void DoSomething()
        {
            Console.WriteLine("I am doing something!");

            var firstBook = _ctx.Books.First();
            Console.WriteLine(firstBook.Title);
        }
    }
}
