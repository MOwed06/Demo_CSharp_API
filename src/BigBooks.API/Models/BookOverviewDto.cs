namespace BigBooks.API.Models
{
    /// <summary>
    /// Simplified book overview
    /// </summary>
    public class BookOverviewDto
    {
        public int Key { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
    }
}
