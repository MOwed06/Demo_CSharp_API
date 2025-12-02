using BigBooks.API.Core;
using BigBooks.API.Entities;
using BigBooks.API.Models;
using BigBooks.API.Providers;
using BigBooks.UnitTest.Common;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using Moq;

namespace BigBooks.UnitTest.ProviderTests
{
    public class BooksProviderTest : BookStoreTest
    {
        private BooksProvider _bookPrv;

        private readonly List<Book> _extraBooks;

        public BooksProviderTest() : base()
        {
            var mockLogger = new Mock<ILogger<BooksProvider>>();
            _bookPrv = new BooksProvider(_ctx, mockLogger.Object);

            _extraBooks = new List<Book>()
            {
                new Book
                {
                    Key = 3,
                    Title = "My Life as a Frog",
                    Author = "Kermit Muppet",
                    Isbn = Guid.Parse("C735CD21-8BCB-453E-A12B-E13569E4E470"),
                    Genre = Genre.Childrens,
                    Description = null,
                    Price = 10.23m,
                    StockQuantity = 3
                },
                new Book
                {
                    Key = 4,
                    Title = "My Life in the Garbage Can",
                    Author = "Oscar Muppet",
                    Isbn = Guid.Parse("A09CF646-C734-4D29-9A46-AE000774CCC1"),
                    Genre = Genre.Childrens,
                    Description = null,
                    Price = 10.23m,
                    StockQuantity = 3
                },
                new Book
                {
                    Key = 5,
                    Title = "Better Living Through Cookies",
                    Author = "C. Monster",
                    Isbn = Guid.Parse("4D976E7C-DBAD-4255-9C58-4C11C992316A"),
                    Genre = Genre.Childrens,
                    Description = null,
                    Price = 10.23m,
                    StockQuantity = 3
                },
                new Book
                {
                    Key = 6,
                    Title = "Still Green after all these Years",
                    Author = "Kermit Muppet",
                    Isbn = Guid.Parse("3C0F4670-A7F5-4769-BF1A-460B3B7A3312"),
                    Genre = Genre.Undefined,
                    Description = null,
                    Price = 10.23m,
                    StockQuantity = 3
                },
                new Book
                {
                    Key = 7,
                    Title = "Flying Fast and Building Things",
                    Author = "Tony Stark",
                    Isbn = Guid.Parse("25E14B0A-01B7-49C1-BD15-0A8A29315C73"),
                    Genre = Genre.Fantasy,
                    Description = null,
                    Price = 10.23m,
                    StockQuantity = 3
                }
            };
        }

        [Theory]
        [InlineData(-1, false)]
        [InlineData(0, false)]
        [InlineData(1, true)]
        [InlineData(11, false)]
        public void CheckBookExists(int bookKey, bool expected)
        {
            // arrange
            InitializeDatabase();

            // act
            var obs = _bookPrv.BookExists(bookKey);

            // assert
            Assert.Equal(expected, obs);
        }

        [Theory]
        [InlineData(new int[0], null)]
        [InlineData(new int[] { 3 }, 3.0)]
        [InlineData(new int[] { 3, 4 }, 3.5)]
        [InlineData(new int[] { 3, 4, 5, 6, 7, 8, 9, 10 }, 6.5)]
        public void BookRatingCalculation(int[] ratings, double? expectedRating)
        {
            const int BOOK_KEY = 3;

            // arrange
            var extraBooks = new List<Book>
            {
                new Book
                {
                    Key = 3,
                    Title = "The Firm",
                    Author = "John Grisham",
                    Isbn = Guid.Parse("C4E619C5-CFFF-46C9-8518-9DE8B58E4E0A"),
                    Genre = Genre.Mystery,
                    StockQuantity = 1,
                    Price = 1m
                }
            };

            var extraReviews = new List<BookReview>();

            int reviewKey = 3;
            foreach (var r in ratings)
            {
                extraReviews.Add(new BookReview
                {
                    Key = reviewKey++,
                    BookKey = BOOK_KEY,
                    Score = r
                });
            }

            InitializeDatabase(extraBooks: extraBooks,
                extraBookReviews: extraReviews);

            // act
            var obs = _bookPrv.GetBook(BOOK_KEY);

            // assert
            Assert.Equal(expectedRating, obs?.Rating);
        }

        [Theory]
        [InlineData(1, 4.5)]
        [InlineData(2, null)]
        public void GetBookCheckRating(int key, double? expectedRating)
        {
            // arrange
            InitializeDatabase();

            // act
            var obs = _bookPrv.GetBook(key);

            // assert
            Assert.Equal(expectedRating, obs?.Rating);
        }

        [Theory]
        [InlineData(BOOK2_GUID, "Duplicate ISBN")]
        [InlineData(NEW_BOOK_GUID, null)]
        public void BookAddDtoGuidCheck(string isbnValue, string expectedError)
        {
            // arrange
            InitializeDatabase();

            var addDto = new BookAddUpdateDto
            {
                Title = "Some Cool Idea",
                Author = "Some Person",
                Isbn = Guid.Parse(isbnValue),
                Description = null,
                Genre = Genre.Childrens,
                Price = 17.63m,
                StockQuantity = 5
            };

            // act
            var obs = _bookPrv.AddBook(addDto);

            // assert
            if (string.IsNullOrEmpty(expectedError))
            {
                // valid key value returned
                Assert.Equal(3, obs.Key);
                Assert.Empty(obs.Error);
            }
            else
            {
                Assert.Contains(expectedError, obs.Error);
                Assert.Null(obs.Key);
            }
        }

        [Theory]
        [InlineData(null, "The Title field is required")]
        [InlineData("", "The Title field is required")]
        [InlineData("A", null)]
        [InlineData(STRING_150_CHARS, null)]
        [InlineData(STRING_151_CHARS, "Title must be a string or array type with a maximum length of '150'")]
        public void BookUpdateDtoTitle(string updateValue, string expectedError)
        {
            const int BOOK_KEY = 1;

            // arrange
            InitializeDatabase();

            var patchDoc = new JsonPatchDocument<BookAddUpdateDto>();
            patchDoc.Replace(p => p.Title, updateValue);

            ExecuteUpdateTest(BOOK_KEY, patchDoc, expectedError);
        }

        [Theory]
        [InlineData(NEW_BOOK_GUID, null)] // overwrite isbn with valid valid
        [InlineData(BOOK1_GUID, null)] // overwrite isbn with original value
        [InlineData(BOOK2_GUID, "Duplicate ISBN")]
        [InlineData("00000000-0000-0000-0000-000000000000", "invalid ISBN value")]
        public void BookUpdateDtoIsbn(string updateValue, string expectedError)
        {
            const int BOOK_KEY = 1;

            // arrange
            InitializeDatabase();

            var updateGuid = Guid.Parse(updateValue);

            var patchDoc = new JsonPatchDocument<BookAddUpdateDto>();
            patchDoc.Replace(p => p.Isbn, updateGuid);

            ExecuteUpdateTest(BOOK_KEY, patchDoc, expectedError);
        }

        [Theory]
        [InlineData(1, "Book01DetailsDto.json")]
        [InlineData(11, null)]
        public void CheckGetBook(int bookKey, string referenceFile)
        {
            // arrange
            InitializeDatabase();

            // act
            var observed = _bookPrv.GetBook(bookKey);

            // assert
            if (referenceFile != null)
            {
                var expected = GetObjectFromSupportJsonFile<BookDetailsDto>(referenceFile);
                CheckObjectsEquivalent(expected, observed);
            }
            else
            {
                // invalid book
                Assert.Null(observed);
            }
        }

        [Theory]
        [InlineData(Genre.Childrens, new int[] { 1, 3, 4, 5 })]
        [InlineData(Genre.Romance, new int[0])]
        [InlineData(Genre.Fantasy, new int[] { 7 })]
        public void CheckGetBooksByGenre(Genre searchGenre, int[] expectedKeys)
        {
            // arrange
            InitializeDatabase(extraBooks: _extraBooks);

            // act
            var obs = _bookPrv.GetBooksByGenre(searchGenre);

            // assert
            var obsKeys = obs.Select(b => b.Key).ToList();

            Assert.Equal(expectedKeys, obsKeys);
        }

        [Theory]
        [InlineData("Muppet", new int[] { 3, 4, 6 })]
        [InlineData("Gonzo", new int[0])]
        [InlineData("", new int[] { 1, 2, 3, 4, 5, 6, 7 })]
        public void CheckGetBooksByAuthor(string searchAuthor, int[] expectedKeys)
        {
            // arrange
            InitializeDatabase(extraBooks: _extraBooks);

            // act
            var obs = _bookPrv.GetBooks(searchAuthor);

            // assert
            var obsKeys = obs.Select(b => b.Key).ToList();

            Assert.Equal(expectedKeys, obsKeys);
        }

        [Fact]
        public void CheckGetAuthors()
        {
            // arrange
            var expectedAuthorList = new List<AuthorInfoDto>
            {
                new AuthorInfoDto
                {
                    Author = "Kermit Muppet",
                    BookCount = 2
                },
                new AuthorInfoDto
                {
                    Author = "C. Monster",
                    BookCount = 1
                },
                new AuthorInfoDto
                {
                    Author = "Oscar Muppet",
                    BookCount = 1
                }
            };

            InitializeDatabase(extraBooks: _extraBooks);

            // act
            var obs = _bookPrv.GetBookAuthors();

            // assert
            foreach (var exp in expectedAuthorList)
            {
                var obsItem = obs.SingleOrDefault(a => a.Author == exp.Author);

                Assert.NotNull(obsItem);
                Assert.Equal(exp.BookCount, obsItem.BookCount);
            }
        }

        private void ExecuteUpdateTest(int bookKey, JsonPatchDocument<BookAddUpdateDto> patchDoc, string expectedError)
        {
            // act
            var obs = _bookPrv.UpdateBook(bookKey, patchDoc);

            // assert
            if (string.IsNullOrEmpty(expectedError))
            {
                Assert.Equal(bookKey, obs.Key);
                Assert.Empty(obs.Error);
            }
            else
            {
                Assert.Null(obs.Key);
                Assert.Contains(expectedError, obs.Error);
            }
        }
    }
}
