namespace BigBooks.API.Models
{
    public class BookPurchaseRequestDto
    {
        public int BookKey { get; set; }
        public int RequestedQuantity { get; set; }
    }
}
