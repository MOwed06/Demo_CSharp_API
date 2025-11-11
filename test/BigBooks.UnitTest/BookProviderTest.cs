using BigBooks.API.Core;
using BigBooks.API.Models;
using BigBooks.API.Providers;
using BigBooks.UnitTest.Common;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using Moq;

namespace BigBooks.UnitTest
{
    public class BookProviderTest : BookStoreTest
    {
        private BookProvider _bookPrv;

        public BookProviderTest() : base()
        {
            var mockLogger = new Mock<ILogger<BookProvider>>();
            _bookPrv = new BookProvider(_ctx, mockLogger.Object);
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
        [InlineData(NEW_BOOK_GUID, null)]  // valid Guid, no error expected
        [InlineData(BOOK2_GUID, "Duplicate ISBN")]
        [InlineData("80F4-403C-B7E5-860BA52B8F99", "invalid ISBN value")]
        public void BookAddDtoGuidCheck(string isbn, string? expectedError)
        {
            // arrange
            InitializeDatabase();

            var addDto = new BookAddUpdateDto
            {
                Title = "Some Cool Idea",
                Author = "Some Person",
                Isbn = isbn,
                Description = null,
                Genre = Genre.Childrens,
                Price = 17.63f,
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
        public void BookUpdateDtoTitle(string? updateValue, string? expectedError)
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
        [InlineData(null, "The Isbn field is required.")]
        [InlineData("", "The Isbn field is required.")]
        [InlineData("A", "invalid ISBN value")]
        [InlineData(BOOK1_GUID, null)] // overwrite isbn with original value
        [InlineData(BOOK2_GUID, "Duplicate ISBN")]
        [InlineData("4BB8-8486-07AE475D80B5", "invalid ISBN value")]
        public void BookUpdateDtoIsbn(string? updateValue, string? expectedError)
        {
            const int BOOK_KEY = 1;

            // arrange
            InitializeDatabase();

            var patchDoc = new JsonPatchDocument<BookAddUpdateDto>();
            patchDoc.Replace(p => p.Isbn, updateValue);

            ExecuteUpdateTest(BOOK_KEY, patchDoc, expectedError);
        }

        private void ExecuteUpdateTest(int bookKey, JsonPatchDocument<BookAddUpdateDto> patchDoc, string? expectedError)
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
