using BigBooks.API.Authentication;
using BigBooks.API.Core;
using BigBooks.API.Models;
using DataMaker;

namespace BigBooksDbGenerator
{
    internal class AccountHandler : MessageHandler
    {
        private List<string> _userEmails;

        public async Task<List<string>> GenerateUsers(int userCount)
        {
            var userAddDtos = BuildUserDtos(userCount);

            _userEmails = userAddDtos.Select(u => u.UserEmail).ToList();

            await CreateUsers(userAddDtos);

            return _userEmails;
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

        internal async Task GeneratePurchases(List<int> bKeys, int maxPurchases)
        {
            foreach (var userEmail in _userEmails)
            {
                var userBookCount = RandomData.GenerateInt(0, maxPurchases+1);

                var authRequest = new AuthRequest
                {
                    UserId = userEmail,
                    Password = ApplicationConstant.USER_PASSWORD
                };

                var purchasedBookKeys = new HashSet<int>();

                using (var client = new HttpClient())
                {
                    var token = await GetAuthToken(client, authRequest);

                    for (int i = 0; i < userBookCount; i++)
                    {
                        var selectedBookKey = RandomData.SelectFromList(bKeys);

                        // allow user to purchase book only once
                        if (purchasedBookKeys.Contains(selectedBookKey))
                        {
                            break;
                        }

                        var purchaseDto = new PurchaseRequestDto
                        {
                            BookKey = selectedBookKey,
                            RequestedQuantity = 1,
                            TransactionConfirmation = Guid.NewGuid()
                        };

                        await SendMessage<BookDetailsDto>(client: client,
                                                uri: TRANSACTIONS_URI,
                                                method: HttpMethod.Post,
                                                token: token,
                                                body: purchaseDto);

                        Console.WriteLine($"User: {userEmail}, BookKey: {selectedBookKey}");

                        purchasedBookKeys.Add(selectedBookKey);
                    }
                }
            }
        }
    }
}
