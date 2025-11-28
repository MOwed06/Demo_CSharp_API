using BigBooks.API.Authentication;
using BigBooks.API.Core;
using BigBooks.API.Models;
using DataMaker;

namespace BigBooksDbGenerator
{
    internal class AccountHandler : MessageHandler
    {
        public async Task<List<string>> GenerateUsers(int userCount)
        {
            var userAddDtos = BuildUserDtos(userCount);

            var userEmails = userAddDtos.Select(u => u.UserEmail).ToList();

            await CreateUsers(userAddDtos);

            return userEmails;
        }

        private async Task CreateUsers(List<UserAddUpdateDto> userAddDtos)
        {
            var authRequest = new AuthRequest
            {
                UserId = "Bruce.Wayne@demo.com",
                Password = ApplicationConstant.USER_PASSWORD
            };

            using (var client = new HttpClient())
            {
                var token = await GetAuthToken(client, authRequest);

                foreach (var dto in userAddDtos)
                {
                    var response = SendMessage<BookDetailsDto>(client: client,
                            uri: ACCOUNTS_URI,
                            method: HttpMethod.Post,
                            token: token,
                            body: dto);
                }
            }
        }

        private List<UserAddUpdateDto> BuildUserDtos(int userCount)
        {
            var userDtos = new List<UserAddUpdateDto>();

            for (int i = 0; i < userCount; i++)
            {
                var firstName = RandomData.GenerateFirstName();
                var familyName = RandomData.GenerateFamilyName();

                var nextUser = new UserAddUpdateDto
                {
                    UserEmail = $"{firstName}.{familyName}@demo.com",
                    UserName = $"{firstName.Substring(0, 1)}. {familyName}",
                    Password = ApplicationConstant.USER_PASSWORD,
                    IsActive = true,
                    Role = Role.Customer,
                    Wallet = RandomData.GenerateDecimal(155, 320, 2)
                };

                userDtos.Add(nextUser);
            }
            return userDtos;
        }
    }
}
