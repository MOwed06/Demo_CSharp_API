namespace BigBooks.API.Models
{
    public class TransactionOverviewDto
    {
        public int TransactionKey { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; }
        public decimal TransactionAmount { get; set; }
        public string PurchaseBook { get; set; }
        public int? PurchaseQuantity { get; set; }
    }
}
