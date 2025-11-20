using BigBooks.API.Core;
using BigBooks.API.Models;
using BigBooks.UnitTest.Common;

namespace BigBooks.UnitTest.ModelTests
{
    public class UserAddUpdateDtoTest : BookStoreTest
    {
        private readonly UserAddUpdateDto _testObject;

        public UserAddUpdateDtoTest()
        {
            // initialize with all parameters valid
            _testObject = new UserAddUpdateDto
            {
                UserEmail = "Some.Person@demo.com",
                UserName = "Some Random Person",
                Password = "A0123B4056",
                Wallet = 82.76M,
                Role = Role.Customer
            };
        }

        [Theory]
        [InlineData("A.B@com", null)]
        [InlineData("Alice.Barbara.Claire.Diana.Eliana.Fiona.Gabriella.Hailey.Isabella.Jennifer.Katherine.Lilian@demo.com", null)]
        [InlineData(null, "UserEmail field is required")]
        [InlineData("", "UserEmail field is required")]
        [InlineData("A.B@", "invalid UserEmail")]
        [InlineData("Alice.Barbara.Claire.Diana.Eliana.Fiona.Gabriella.Hailey.Isabella.Jennifer.Katherine.Lillian@demo.com", "maximum length of '100'")]
        public void ConfirmUserEmailValidation(string emailInput, string expectedError)
        {
            // arrange
            _testObject.UserEmail = emailInput;

            // act
            var response = ValidateDto(_testObject);

            // assert
            if (string.IsNullOrEmpty(expectedError))
            {
                Assert.Empty(response.Error);
                Assert.True(response.Valid);
            }
            else
            {
                Assert.Contains(expectedError, response.Error);
            }
        }

        [Theory]
        [InlineData(null, "must be between 1 and 5000")]
        [InlineData(0.99, "must be between 1 and 5000")]
        [InlineData(1.00, null)]
        [InlineData(5000, null)]
        [InlineData(5001, "must be between 1 and 5000")]
        public void ConfirmWalletValidation(decimal walletInput, string expectedError)
        {
            // arrange
            _testObject.Wallet = walletInput;

            // act
            var response = ValidateDto(_testObject);

            // assert
            if (string.IsNullOrEmpty(expectedError))
            {
                Assert.Empty(response.Error);
                Assert.True(response.Valid);
            }
            else
            {
                Assert.Contains(expectedError, response.Error);
            }
        }

        [Theory]
        [InlineData(null, "UserName field is required")]
        [InlineData("", "UserName field is required")]
        [InlineData("A", null)]
        [InlineData(STRING_100_CHARS, null)]
        [InlineData(STRING_101_CHARS, "maximum length of '100'")]
        public void ConfirmUserNameValidation(string nameInput, string expectedError)
        {
            // arrange
            _testObject.UserName = nameInput;

            // act
            var response = ValidateDto(_testObject);

            // assert
            if (string.IsNullOrEmpty(expectedError))
            {
                Assert.Empty(response.Error);
                Assert.True(response.Valid);
            }
            else
            {
                Assert.Contains(expectedError, response.Error);
            }
        }
    }
}
