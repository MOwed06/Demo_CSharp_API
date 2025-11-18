namespace BigBooks.API.Models
{
    public class PurchaseRequestDto
    {
        public int BookKey { get; set; }
        public int RequestedQuantity { get; set; }
        public Guid TransactionConfirmation { get; set; }
    }
}
