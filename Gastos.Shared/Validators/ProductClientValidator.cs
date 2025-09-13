namespace Gastos.Shared.Validators;

public class ProductClientValidator : AbstractValidator<ProductDto>
{
    public ProductClientValidator(LocalizationService loc)
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(loc.Get(RS.ValidProductId));

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(loc.Get(RS.ValidProductNameMandatory))
            .MaximumLength(100)
            .WithMessage(string.Format(loc.Get(RS.ValidProductNameTooLong0), 100));

        RuleFor(x => x.UnitsPack)
            .GreaterThan(0)
            .WithMessage(loc.Get(RS.ValidProductUnitsPack));
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<ProductDto>.CreateWithOptions((ProductDto)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return [];

        return result.Errors.Select(e => e.ErrorMessage);
    };
}
