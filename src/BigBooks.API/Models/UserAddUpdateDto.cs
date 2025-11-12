using BigBooks.API.Core;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace BigBooks.API.Models
{
    public class UserAddUpdateDto : IValidatableObject
    {
        [Required]
        [MinLength(1)]
        [MaxLength(100)]
        public string UserEmail { get; set; } = string.Empty;

        [Required]
        [MinLength(1)]
        [MaxLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [MinLength(4)]
        [MaxLength(20)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Range(1.00, 5000)]
        public decimal Wallet { get; set; }

        [Required]
        public Role Role { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var vResults = new List<ValidationResult>();

            if (!MailAddress.TryCreate(UserEmail, out _))
            {
                vResults.Add(new ValidationResult($"invalid UserEmail {UserEmail}"));
            }

            return vResults;
        }
    }
}
