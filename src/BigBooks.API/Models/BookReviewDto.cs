namespace BigBooks.API.Models
{
    public class BookReviewDto
    {
        public int ReviewKey { get; set; }
        public string BookTitle { get; set; }
        public int Score { get; set; }
        public DateTime ReviewDate { get; set; }
        public string User { get; set; }
        public string? Description { get; set; }
    }
}
