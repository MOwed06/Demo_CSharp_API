using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BigBooks.API.Entities
{
    public class BookPurchase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Key { get; set; }

        [Required]
        public DateTime PurchaseDate { get; set; }

        public int PurchaseQuantity { get; set; }

        [ForeignKey("UserKey")]
        public AppUser? AppUser { get; set; }
        public int UserKey { get; set; }

        [ForeignKey("BookKey")]
        public Book? Book { get; set; }
        public int BookKey { get; set; }
    }
}
