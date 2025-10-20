namespace Gastos.Shared.Validators;

public sealed class ReceiptClientValidator : AbstractValidator<ReceiptDto>
{
    public ReceiptClientValidator(LocalizationService loc)
    {
        RuleFor(x => x.StoreId)
            .NotEmpty()
            .NotEqual(Guid.Empty)
            .WithMessage(loc.Get(RS.ValidReceiptId));

        RuleFor(x => x.TransactionDateUtc)
            .NotEmpty()
            .WithMessage(loc.Get(RS.ValidReceiptTransactionDateUtc));

        RuleFor(x => x.Discount)
            .LessThanOrEqualTo(0)
            .WithMessage(loc.Get(RS.ValidReceiptDiscount));
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<ReceiptDto>.CreateWithOptions((ReceiptDto)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return [];

        return result.Errors.Select(e => e.ErrorMessage);
    };
}
