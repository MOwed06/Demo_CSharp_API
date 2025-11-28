// temporary app
// use web api calls to fill database with interesting data

using DummyConsole;

Console.WriteLine("Hello, World!");

var bookHandler = new BookHandler(20);

var bookDtos = bookHandler.BuildBookDtos(100);

await bookHandler.AddBooks(bookDtos);

Console.WriteLine("Goodbye, Cruel World!");
