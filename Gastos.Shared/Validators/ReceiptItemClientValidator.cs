namespace Gastos.Shared.Validators;

public sealed class ReceiptItemClientValidator : AbstractValidator<ReceiptItemDto>
{
    public ReceiptItemClientValidator(LocalizationService loc)
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .NotEqual(Guid.Empty)
            .WithMessage(loc.Get(RS.ValidProductId));

        RuleFor(x => x.Quantity)
            .NotEqual(0)
            .WithMessage(loc.Get(RS.ValidReceiptItemQuantity));

        RuleFor(x => x.Amount)
            .NotEqual(0)
            .WithMessage(loc.Get(RS.ValidReceiptItemAmount));
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<ReceiptItemDto>.CreateWithOptions((ReceiptItemDto)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return [];

        return result.Errors.Select(e => e.ErrorMessage);
    };
}
