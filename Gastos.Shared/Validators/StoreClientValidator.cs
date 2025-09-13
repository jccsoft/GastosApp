namespace Gastos.Shared.Validators;

public class StoreClientValidator : AbstractValidator<StoreDto>
{
    public StoreClientValidator(LocalizationService loc)
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .NotEqual(Guid.Empty)
            .WithMessage(loc.Get(RS.ValidStoreId));

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(loc.Get(RS.ValidStoreNameMandatory))
            .MaximumLength(100)
            .WithMessage(string.Format(loc.Get(RS.ValidStoreNameTooLong0), 100));

    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<StoreDto>.CreateWithOptions((StoreDto)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return [];

        return result.Errors.Select(e => e.ErrorMessage);
    };
}
