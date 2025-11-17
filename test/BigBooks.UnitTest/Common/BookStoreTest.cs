using BigBooks.API.Core;
using BigBooks.API.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BigBooks.UnitTest.Common
{
    public abstract class BookStoreTest : IDisposable
    {
        protected const string BOOK1_GUID = "AAFB14A1-676B-46D9-8485-394A189F6AE5";
        protected const string BOOK2_GUID = "D77D41ED-1A0D-4BB8-8486-07AE475D80B5";
        protected const string NEW_BOOK_GUID = "31D7C872-077A-4386-93E6-72175E23E84E";

        protected const string STRING_100_CHARS = "ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dol";
        protected const string STRING_101_CHARS = "ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolo";

        protected const string STRING_150_CHARS = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, q";
        protected const string STRING_151_CHARS = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, qu";

        protected const string CUSTOMER_2_EMAIL = "Jessica.Jones@test.com";

        protected readonly BigBookDbContext _ctx;

        protected void InitializeDatabase(List<Book>? extraBooks = null,
            List<BookReview>? extraBookReviews = null,
            List<AppUser>? extraAppUsers = null,
            List<BookPurchase>? extraBookPurchases = null)
        {
            // two books
            var books = new List<Book>
            {
                new Book
                {
                    Key = 1,
                    Title = "Where the Wild Things Are",
                    Author = "Maurice Sendak",
                    Isbn = Guid.Parse(BOOK1_GUID),
                    Genre = Genre.Childrens,
                    Description = "Let the wild rumpus continue as this classic comes to life like never before with new reproductions of Maurice Sendak's artwork.",
                    Price = 11.42m,
                    StockQuantity = 17
                },
                new Book
                {
                    Key = 2,
                    Title = "Citizen Soldiers",
                    Author = "Stephen Ambrose",
                    Isbn = Guid.Parse(BOOK2_GUID),
                    Genre = Genre.History,
                    Description = "Citizen Soldiers opens at 0001 hours, June 7, 1944, on the Normandy beaches, and ends at 0245 hours, May 7, 1945, with the allied victory.",
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
                    UserEmail = "Bruce.Banner@test.com",
                    Password = ApplicationConstant.USER_PASSWORD,
                    Role = Role.Admin
                },
                new AppUser
                {
                    Key = 2,
                    UserName = "Jessica Jones",
                    UserEmail = CUSTOMER_2_EMAIL,
                    Password = ApplicationConstant.USER_PASSWORD,
                    Role = Role.Customer
                }
            };

            if (extraAppUsers != null)
            {
                users.AddRange(extraAppUsers);
            }

            // two book purchases for user 1
            var bookPurchases = new List<BookPurchase>
            {
                new BookPurchase
                {
                    Key = 1,
                    PurchaseDate = DateTime.Parse("2025-03-15").Date,
                    UserKey = 1,
                    BookKey = 1,
                    PurchaseQuantity = 2
                },
                new BookPurchase
                {
                    Key = 2,
                    PurchaseDate = DateTime.Parse("2025-03-16").Date,
                    UserKey = 1,
                    BookKey = 2,
                    PurchaseQuantity = 1
                }
            };

            if (extraBookPurchases != null)
            {
                bookPurchases.AddRange(extraBookPurchases);
            }

            _ctx.Books.AddRange(books);
            _ctx.BookReviews.AddRange(bookReviews);
            _ctx.AppUsers.AddRange(users);
            _ctx.BookPurchases.AddRange(bookPurchases);
            _ctx.SaveChanges();
        }

        protected BookStoreTest()
        {
            var options = new DbContextOptionsBuilder<BigBookDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _ctx = new BigBookDbContext(options);
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

        public void Dispose()
        {
            _ctx.Dispose();
        }
    }
}
