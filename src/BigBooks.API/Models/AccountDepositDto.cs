using System.ComponentModel.DataAnnotations;

namespace BigBooks.API.Models
{
    public class AccountDepositDto : IValidatableObject
    {
        [Required]
        [Range(10.0, 10000)]
        public decimal Amount { get; set; }

        [Required]
        public Guid Confirmation { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var vResults = new List<ValidationResult>();

            // TransactionConfirmation value must be valid guid
            if (Guid.Empty.Equals(Confirmation))
            {
                vResults.Add(new ValidationResult($"invalid TransactionConfirmation value: {Confirmation}"));
            }

            return vResults;
        }
    }
}
