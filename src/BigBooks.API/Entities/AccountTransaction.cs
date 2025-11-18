using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BigBooks.API.Entities
{
    public class AccountTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Key { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; }

        [Required]
        public decimal TransactionAmount { get; set; }

        [Required]
        public Guid TransactionConfirmation { get; set; }

        [ForeignKey("UserKey")]
        public AppUser? AppUser { get; set; }
        public int UserKey { get; set; }

        public int? BookKey { get; set; }
        public int? PurchaseQuantity { get; set; }
    }
}
