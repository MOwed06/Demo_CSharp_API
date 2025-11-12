using BigBooks.API.Core;
using System.ComponentModel.DataAnnotations;

namespace BigBooks.API.Models
{
    public class BookAddUpdateDto : IValidatableObject
    {
        [Required]
        [MinLength(1)]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MinLength(1)]
        [MaxLength(100)]
        public string Author { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Isbn { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public Genre Genre { get; set; } = Genre.Undefined;

        [Required]
        [Range(0.01, 1000)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, 1000)]
        public int StockQuantity { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var vResults = new List<ValidationResult>();

            // isbn value must be valid guid
            var isValid = Guid.TryParse(Isbn, out _);

            if (!isValid)
            {
                vResults.Add(new ValidationResult($"invalid ISBN value: {Isbn}"));
            }

            return vResults;
        }
    }
}
