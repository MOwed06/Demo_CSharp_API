namespace BigBooks.API.Models
{
    public class UserDetailsDto
    {
        public int Key { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public string Wallet { get; set; }
        public IList<TransactionOverviewDto> Transactions { get; set; }
    }
}
