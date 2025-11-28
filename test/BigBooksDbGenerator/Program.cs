// this exists only for filling the BigBooks db with interesting values

using BigBooksDbGenerator;

const int AUTHOR_COUNT = 20;
const int BOOK_COUNT = 100;
const int USER_COUNT = 50;

Console.WriteLine("Hello, World!");

try
{
    var booksHandler = new BookHandler(AUTHOR_COUNT);
    var bKeys = await booksHandler.GenerateBooks(BOOK_COUNT);

    Console.WriteLine($"Books: {bKeys.Count()}, MaxKey: {bKeys.Max()}\n");

    var acctsHandler = new AccountHandler();
    var userEmails = await acctsHandler.GenerateUsers(USER_COUNT);

    Console.WriteLine($"Users: {userEmails.Count()}, LastUser: {userEmails.Last()}\n");
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}

Console.WriteLine("Goodby, Cruel World!");
