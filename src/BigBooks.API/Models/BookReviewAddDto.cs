using System.ComponentModel.DataAnnotations;

namespace BigBooks.API.Models
{
    public class BookReviewAddDto
    {
        [Required]
        [Range(0, 10)]
        public int Score { get; set; }

        public bool IsAnonymous { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }
    }
}
