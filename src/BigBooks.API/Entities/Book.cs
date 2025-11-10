using BigBooks.API.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BigBooks.API.Entities
{
    public class Book
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Key { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Author { get; set; } = string.Empty;

        [Required]
        public Guid Isbn { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public Genre Genre { get; set; }

        [Required]
        public float Price { get; set; }

        [Required]
        public int StockQuantity { get; set; }

        public ICollection<BookReview> Reviews { get; set; } = new List<BookReview>();
    }
}
