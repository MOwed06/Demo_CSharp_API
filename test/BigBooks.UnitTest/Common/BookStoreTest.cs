using BigBooks.API.Core;
using BigBooks.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace BigBooks.UnitTest.Common
{
    public abstract class BookStoreTest : IDisposable
    {
        protected const string BOOK1_GUID = "AAFB14A1-676B-46D9-8485-394A189F6AE5";
        protected const string BOOK2_GUID = "D77D41ED-1A0D-4BB8-8486-07AE475D80B5";
        protected const string NEW_BOOK_GUID = "31D7C872-077A-4386-93E6-72175E23E84E";

        protected const string STRING_150_CHARS = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, q";
        protected const string STRING_151_CHARS = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, qu";

        protected readonly BigBookDbContext _ctx;

        protected void InitializeDatabase(List<Book>? extraBooks = null,
            List<BookReview>? extraBookReviews = null,
            List<AppUser>? extraAppUsers = null)
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
                    Score = 3
                },
                new BookReview
                {
                    Key = 2,
                    BookKey = 1,
                    Score = 6
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
                    UserEmail = "Jessica.Jones@test.com",
                    Password = ApplicationConstant.USER_PASSWORD,
                    Role = Role.Customer
                }
            };

            if (extraAppUsers != null)
            {
                users.AddRange(extraAppUsers);
            }

            _ctx.Books.AddRange(books);
            _ctx.BookReviews.AddRange(bookReviews);
            _ctx.AppUsers.AddRange(users);
            _ctx.SaveChanges();
        }

        protected BookStoreTest()
        {
            var options = new DbContextOptionsBuilder<BigBookDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _ctx = new BigBookDbContext(options);
        }

        public void Dispose()
        {
            _ctx.Dispose();
        }
    }
}
