using System.ComponentModel.DataAnnotations;

namespace BigBooks.API.Providers
{
    public abstract class BigBooksProvider
    {
        protected (bool Valid, string Error) ValidateDto(object dto)
        {
            var validationContext = new ValidationContext(dto);

            var validationResults = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(instance: dto,
                validationContext: validationContext,
                validationResults: validationResults,
                validateAllProperties: true);

            var errors = validationResults.Select(v => v.ErrorMessage).ToList();

            return (isValid, string.Join(", ", errors));
        }
    }
}
