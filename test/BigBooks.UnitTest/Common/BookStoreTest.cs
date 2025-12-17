using BigBooks.API.Core;
using BigBooks.API.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace BigBooks.UnitTest.Common
{
    public abstract class BookStoreTest
    {
        protected const int TEST_TIMEOUT_MS = 2000;
        protected const string BOOK1_GUID = "AAFB14A1-676B-46D9-8485-394A189F6AE5";
        protected const string BOOK2_GUID = "D77D41ED-1A0D-4BB8-8486-07AE475D80B5";
        protected const string NEW_BOOK_GUID = "31D7C872-077A-4386-93E6-72175E23E84E";

        protected const string STRING_100_CHARS = "ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dol";
        protected const string STRING_101_CHARS = "ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolo";

        protected const string STRING_150_CHARS = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, q";
        protected const string STRING_151_CHARS = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, qu";

        protected const string CUSTOMER_1_EMAIL = "Bruce.Banner@test.com";
        protected const string CUSTOMER_2_EMAIL = "Jessica.Jones@test.com";

        protected readonly IDbContextFactory<BigBookDbContext> TestContextFactory;

        protected void InitializeDatabase(List<Book> extraBooks = null,
            List<BookReview> extraBookReviews = null,
            List<AppUser> extraAppUsers = null,
            List<AccountTransaction> extraTransactions = null)
        {
            // two books
            var books = new List<Book>
            {
                new Book
                {
                    Key = 1,
                    Title = "See You Later, Alligator",
                    Author = "Sally Hopgood",
                    Isbn = Guid.Parse(BOOK1_GUID),
                    Genre = Genre.Childrens,
                    Description = "A departing tortoise has his bags packed and is almost ready to set out on an adventure, but he cannot leave until he says good-bye to each of his animal friends. The tortoise stops for every last timid mouse and bumblebee, shouting his rhyming good-byes, making you wonder if, perhaps, he’s stalling the start of his trip.",
                    Price = 11.42m,
                    StockQuantity = 17
                },
                new Book
                {
                    Key = 2,
                    Title = "Fierce Patriot: The Tangled Lives of William Tecumseh Sherman",
                    Author = "Robert L. O'Connell",
                    Isbn = Guid.Parse(BOOK2_GUID),
                    Genre = Genre.History,
                    Description = "America’s first “celebrity” general, William Tecumseh Sherman was a man of many faces. Some were exalted in the public eye, others known only to his intimates.",
                    Price = 17.11m,
                    StockQuantity = 6
                }
            };

            if (extraBooks != null)
            {
                books.AddRange(extraBooks);
            }

            // reviews for one book
            var bookReviews = new List<BookReview>
            {
                new BookReview
                {
                    Key = 1,
                    BookKey = 1,
                    ReviewDate = DateTime.Parse("2025-07-11"),
                    Score = 3
                },
                new BookReview
                {
                    Key = 2,
                    BookKey = 1,
                    ReviewDate = DateTime.Parse("2025-08-03"),
                    Score = 6,
                    UserKey = 1
                }
            };

            if (extraBookReviews != null)
            {
                bookReviews.AddRange(extraBookReviews);
            }

            // two users
            var users = new List<AppUser>
            {
                new AppUser
                {
                    Key = 1,
                    UserName = "Bruce Banner",
                    UserEmail = CUSTOMER_1_EMAIL,
                    Password = ApplicationConstant.USER_PASSWORD,
                    IsActive = true,
                    Role = Role.Admin,
                    Wallet = 1m
                },
                new AppUser
                {
                    Key = 2,
                    UserName = "Jessica Jones",
                    UserEmail = CUSTOMER_2_EMAIL,
                    IsActive = true,
                    Password = ApplicationConstant.USER_PASSWORD,
                    Role = Role.Customer,
                    Wallet = 100m
                }
            };

            if (extraAppUsers != null)
            {
                users.AddRange(extraAppUsers);
            }

            // two user transactions for user 1
            var accountTransactions = new List<AccountTransaction>
            {
                new AccountTransaction
                {
                    Key = 1,
                    TransactionDate = DateTime.Parse("2025-03-15").Date,
                    UserKey = 1,
                    TransactionAmount = -26.23m,
                    TransactionConfirmation = Guid.Parse("1CC8F708-68A6-4998-AE07-92717392CD4F"),
                    BookKey = 1,
                    PurchaseQuantity = 2
                },
                new AccountTransaction
                {
                    Key = 2,
                    TransactionDate = DateTime.Parse("2025-03-16").Date,
                    UserKey = 1,
                    TransactionAmount = -17.17m,
                    TransactionConfirmation = Guid.Parse("AE8A1120-86E2-4CFE-8816-6EBBC273C458"),
                    BookKey = 2,
                    PurchaseQuantity = 1
                }
            };

            if (extraTransactions != null)
            {
                accountTransactions.AddRange(extraTransactions);
            }

            using (var ctx = TestContextFactory.CreateDbContext())
            {
                ctx.Books.AddRange(books);
                ctx.BookReviews.AddRange(bookReviews);
                ctx.AppUsers.AddRange(users);
                ctx.Transactions.AddRange(accountTransactions);
                ctx.SaveChanges();
            }
        }

        protected BookStoreTest()
        {
            var options = new DbContextOptionsBuilder<BigBookDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var mockDbContextFactory = new Mock<IDbContextFactory<BigBookDbContext>>();
            mockDbContextFactory.Setup(m => m.CreateDbContext())
                .Returns(() => new BigBookDbContext(options));

            TestContextFactory = mockDbContextFactory.Object;
        }

        protected (bool Valid, string Error) ValidateDto(object dto)
        {
            var validationContext = new ValidationContext(dto);

            var validationResults = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(instance: dto,
                validationContext: validationContext,
                validationResults: validationResults,
                validateAllProperties: true);

            var errors = validationResults.Select(v => v.ErrorMessage).ToList();

            return (isValid, string.Join(", ", errors));
        }

        /// <summary>
        /// Simplified comparison of class properties
        /// Cast class to json string
        /// Compare expected and observed json strings
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="observed"></param>
        protected void CheckObjectsEquivalent(object expected, object observed)
        {
            var expectedJson = JsonConvert.SerializeObject(expected);
            var observedJson = JsonConvert.SerializeObject(observed);

            Assert.Equal(expectedJson, observedJson);
        }

        protected string ReadSupportFile(string fileName)
        {
            var fullName = Path.Combine(Directory.GetCurrentDirectory(), "SupportFiles", fileName);
            return File.ReadAllText(fullName);
        }

        protected T GetObjectFromSupportJsonFile<T>(string fileName)
        {
            var fileData = ReadSupportFile(fileName);
            return JsonConvert.DeserializeObject<T>(fileData);
        }
    }
}
