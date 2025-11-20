using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BigBooks.API.Entities
{
    public class BookReview
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Key { get; set; }

        [Required]
        [Range(0, 10)]
        public int Score { get; set; }

        [Required]
        public DateTime ReviewDate { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [ForeignKey("UserKey")]
        public AppUser User { get; set; }
        public int? UserKey { get; set; } // UserKey null for anonymous review

        [ForeignKey("BookKey")]
        public Book Book { get; set; }
        public int BookKey { get; set; }
    }
}
