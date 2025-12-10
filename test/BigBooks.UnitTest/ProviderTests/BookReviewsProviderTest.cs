using BigBooks.API.Core;
using BigBooks.API.Interfaces;
using BigBooks.API.Models;
using BigBooks.API.Providers;
using BigBooks.UnitTest.Common;
using Microsoft.Extensions.Logging;
using Moq;

namespace BigBooks.UnitTest.ProviderTests
{
    public class BookReviewsProviderTest : BookStoreTest
    {
        private readonly BookReviewsProvider _bookReviewPrv;
        private readonly Mock<IUsersProvider> _mockUserPrv;

        public BookReviewsProviderTest()
        {
            var logger = new Mock<ILogger<BookReviewsProvider>>().Object;

            _mockUserPrv = new Mock<IUsersProvider>();

            _bookReviewPrv = new BookReviewsProvider(TestContextFactory, _mockUserPrv.Object, logger);
        }

        /// <summary>
        /// This test was partially generated via GitHub Copilot using previously created unit tests as models.
        /// This unit test mocks the UserProvider instead of using a test instance of the UserProvider.
        /// </summary>
        [Fact]
        public void AddBookReview_InvalidUser_ReturnsError()
        {
            // arrange
            InitializeDatabase();
            const int BOOK_KEY = 1; // set by db initialization

            _mockUserPrv.Setup(u => u.GetUserKeyFromToken(It.IsAny<string>()))
                .Returns(new ProviderKeyResponse(null, "Invalid user"));

            var dto = new BookReviewAddDto { Score = 5, Description = "good", IsAnonymous = false };

            // act
            var obs = _bookReviewPrv.AddBookReview("noone@demo.com", BOOK_KEY, dto);

            // assert
            Assert.Null(obs.Key);
            Assert.Contains("Invalid user", obs.Error);
        }

        /// <summary>
        /// This test was partially generated via GitHub Copilot using previously created unit tests as models.
        /// This unit test mocks the UserProvider instead of using a test instance of the UserProvider.
        /// </summary>
        [Fact]
        public void AddBookReview_InactiveUser_ReturnsError()
        {
            // arrange
            InitializeDatabase();
            const int BOOK_KEY = 1; // set by db initialization
            const string DEACTIVATED_USER_EMAIL = "James.Barnes@test.com";
            const int DEACTIVATED_USER_KEY = 202;

            // always return for deactivated user key (regardless of current user email from token)
            _mockUserPrv.Setup(u => u.GetUserKeyFromToken(It.IsAny<string>()))
                .Returns(new ProviderKeyResponse(DEACTIVATED_USER_KEY, string.Empty));
            // return false when status check for deactivated user
            _mockUserPrv.Setup(u => u.IsUserActive(DEACTIVATED_USER_KEY)).Returns(false);

            var dto = new BookReviewAddDto { Score = 4, Description = "meh", IsAnonymous = false };

            // act
            var obs = _bookReviewPrv.AddBookReview(DEACTIVATED_USER_EMAIL, BOOK_KEY, dto);

            // assert
            Assert.Null(obs.Key);
            Assert.Contains("User is deactivated", obs.Error);
        }

        /// <summary>
        /// This test was partially generated via GitHub Copilot using previously created unit tests as models.
        /// This unit test mocks the UserProvider instead of using a test instance of the UserProvider.
        /// </summary>
        [Fact]
        public void AddBookReview_InvalidBookKey_ReturnsError()
        {
            // arrange
            InitializeDatabase();
            const int INVALID_BOOK_KEY = 401; // *NOT* set by db initialization
            const string VALID_USER_EMAIL = "James.Barnes@test.com";
            const int VALID_USER_KEY = 202;

            // if given valid specific user email, return valid user key
            _mockUserPrv.Setup(u => u.GetUserKeyFromToken(VALID_USER_EMAIL))
                .Returns(new ProviderKeyResponse(VALID_USER_KEY, string.Empty));
            // return true when status check for valid user
            _mockUserPrv.Setup(u => u.IsUserActive(VALID_USER_KEY)).Returns(true);

            var dto = new BookReviewAddDto { Score = 3, Description = "ok", IsAnonymous = false };

            // act
            var obs = _bookReviewPrv.AddBookReview(VALID_USER_EMAIL, INVALID_BOOK_KEY, dto);

            // assert
            Assert.Null(obs.Key);
            Assert.Contains("Invalid Book Key", obs.Error);
        }

        /// <summary>
        /// Refer to BookStoreTest.InitializeDatabase() which established book reviews for
        /// books 1 and 2 by user 1.
        /// Moq setup was altered to demonstrate alternate paradigm (in constrast to 
        /// specific mocking setup in AddBookReview_InvalidUser_ReturnsError and 
        /// AddBookReview_InvalidBookKey_ReturnsError).
        /// </summary>
        [Fact]
        public void AddBookReview_UserAlreadyReviewed_ReturnsError()
        {
            // arrange
            const int BOOK_KEY = 1;
            const int USER_KEY_1 = 1; // note that this user has existing book reviews
            InitializeDatabase();

            // regardless of user email received, return user key 1
            _mockUserPrv.Setup(u => u.GetUserKeyFromToken(It.IsAny<string>()))
                .Returns(new ProviderKeyResponse(USER_KEY_1, string.Empty));
            // regardless of user key received, return active status
            _mockUserPrv.Setup(u => u.IsUserActive(It.IsAny<int>())).Returns(true);

            var dto = new BookReviewAddDto { Score = 8, Description = "lovely", IsAnonymous = false };

            // act
            var obs = _bookReviewPrv.AddBookReview(CUSTOMER_1_EMAIL, BOOK_KEY, dto);

            // assert
            Assert.Null(obs.Key);
            Assert.Contains("Existing BookReview by user", obs.Error);
        }

        /// <summary>
        /// This test confirms that review is added to the db
        /// by retrieving the review dto for the new entry
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AddBookReview_AddsReviewDetails(bool isAnonymous)
        {
            // arrange
            const int USER_KEY_2 = 2;
            const int BOOK_KEY_2 = 2; // set by db initialization
            const int EXPECTED_REVIEW_SCORE = 8;
            InitializeDatabase();

            _mockUserPrv.Setup(u => u.GetUserKeyFromToken(It.IsAny<string>()))
                .Returns(new ProviderKeyResponse(USER_KEY_2, string.Empty));
            _mockUserPrv.Setup(u => u.IsUserActive(USER_KEY_2)).Returns(true);

            var dto = new BookReviewAddDto
            {
                Score = EXPECTED_REVIEW_SCORE,
                Description = "excellent",
                IsAnonymous = isAnonymous
            };

            // act
            var obs = _bookReviewPrv.AddBookReview(CUSTOMER_2_EMAIL, BOOK_KEY_2, dto);

            // assert
            Assert.NotNull(obs.Key);
            Assert.Empty(obs.Error);

            var obsReviewDto = _bookReviewPrv.GetBookReview(obs.Key.Value);
            Assert.Equal(dto.Score, obsReviewDto.Score);
            Assert.Equal("Fierce Patriot: The Tangled Lives of William Tecumseh Sherman", obsReviewDto.BookTitle);

            if (isAnonymous)
            {
                Assert.Equal(ApplicationConstant.ANONYMOUS_USER, obsReviewDto.User);
            }
            else
            {
                Assert.Equal(CUSTOMER_2_EMAIL, obsReviewDto.User);
            }
        }
    }
}
