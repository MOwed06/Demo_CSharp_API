namespace BigBooks.API.Models
{
    /// <summary>
    /// Book detail information
    /// </summary>
    public class BookDetailsDto
    {
        public int Key { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;

        public string Isbn { get; set; } = string.Empty;
        public string Description { get; set; }
        public string Genre { get; set; }
        public string Price { get; set; } = string.Empty;
        public bool InStock { get; set; }
        public double? Rating { get; set; }
    }
}
