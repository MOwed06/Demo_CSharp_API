namespace BigBooks.API.Models
{
    public class UserOverviewDto
    {
        public int Key { get; set; }
        public string UserEmail { get; set; }
        public string Role { get; set; }
        public int BookCount { get; set; }
    }
}
