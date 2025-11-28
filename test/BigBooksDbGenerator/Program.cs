// this exists only for filling the BigBooks db with interesting values

using BigBooksDbGenerator;

const int AUTHOR_COUNT = 10;
const int BOOK_COUNT = 50;
const int USER_COUNT = 20;
const int USER_MAX_BOOKS = 4;
const int USER_MAX_REVIEWS = 3;

Console.WriteLine("Hello, World!");

try
{
    var booksHandler = new BookHandler(AUTHOR_COUNT);
    var bKeys = await booksHandler.GenerateBooks(BOOK_COUNT);

    Console.WriteLine($"Books: {bKeys.Count()}, MaxKey: {bKeys.Max()}\n");

    var acctsHandler = new AccountHandler();
    var userEmails = await acctsHandler.GenerateUsers(USER_COUNT);

    Console.WriteLine($"Users: {userEmails.Count()}, LastUser: {userEmails.Last()}\n");

    await acctsHandler.GeneratePurchases(bKeys, USER_MAX_BOOKS);
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}

Console.WriteLine("Goodby, Cruel World!");
