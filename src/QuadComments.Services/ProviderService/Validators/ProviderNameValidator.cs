using System.Threading.Tasks;
using FluentValidation;

namespace QuadComments.Services.ProviderService.Validators
{
  public class ProviderNameValidator : AbstractValidator<ProviderNameValidator.FluentValidatableString>
  {
    public class FluentValidatableString
    {
      public string name { get; }
      public FluentValidatableString(string name)
      {
        this.name = name;
      }
    }
    
    private ProviderNameValidator()
    {
      RuleFor(x => x.name)
        .NotEmpty()
        .MaximumLength(50);
      RuleFor(x => x.name)
        .Matches("^[a-zA-Z0-9_-]*$")
        .WithMessage("Allowed only characters, numbers and '_' or '-'.");
    }

    public static async Task ValidateAndThrowAsync(string name)
    {
      var validator = new ProviderNameValidator();
      var validatableString = new FluentValidatableString(name);
      await validator.ValidateAndThrowAsync(validatableString);
    }
  }
}
