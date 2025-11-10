namespace BigBooks.API.Models
{
    public class UserDetailsDto
    {
        public int Key { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public float Wallet { get; set; }
        public IList<string> Books { get; set; }
    }
}
