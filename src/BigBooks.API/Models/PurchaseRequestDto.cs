using System.ComponentModel.DataAnnotations;

namespace BigBooks.API.Models
{
    public class PurchaseRequestDto : IValidatableObject
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int BookKey { get; set; }

        [Required]
        [Range(0, 100)]
        public int RequestedQuantity { get; set; }
        public Guid TransactionConfirmation { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var vResults = new List<ValidationResult>();

            // isbn value must be valid guid
            if (Guid.Empty.Equals(TransactionConfirmation))
            {
                vResults.Add(new ValidationResult($"invalid TransactionConfirmation value: {TransactionConfirmation}"));
            }

            return vResults;
        }
    }
}
