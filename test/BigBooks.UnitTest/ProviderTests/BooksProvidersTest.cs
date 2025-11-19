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
    public class BooksProvidersTest : BookStoreTest
    {
        private BooksProvider _bookPrv;

        public BooksProvidersTest() : base()
        {
            var mockLogger = new Mock<ILogger<BooksProvider>>();
            _bookPrv = new BooksProvider(_ctx, mockLogger.Object);
        }

        [Fact]
        public void GetBookCheckTitleAuthor()
        {
            // arrange
            InitializeDatabase();

            // act
            var obs = _bookPrv.GetBook(1);

            // assert
            Assert.Equal("Where the Wild Things Are", obs?.Title);
            Assert.Equal("Maurice Sendak", obs?.Author);
            Assert.Equal(BOOK1_GUID, obs?.Isbn);
        }

        [Fact]
        public void GetBookBadKey()
        {
            // arrange
            InitializeDatabase();

            // act
            var obs = _bookPrv.GetBook(5);

            // assert
            Assert.Null(obs);
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
        [InlineData(new int [0], null)]
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
