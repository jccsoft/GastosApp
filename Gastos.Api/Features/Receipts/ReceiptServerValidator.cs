namespace Gastos.Api.Features.Receipts;

public sealed class ReceiptServerValidator : AbstractValidator<ReceiptDto>
{
    private readonly IReceiptRepository _receiptRepo;

    public ReceiptServerValidator(
        IReceiptRepository receiptRepo,
        LocalizationService loc)
    {
        _receiptRepo = receiptRepo;

        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(loc.Get(RS.ValidReceiptId));

        RuleFor(x => x.TransactionDateUtc)
            .NotNull()
            .WithMessage(loc.Get(RS.ValidReceiptTransactionDateUtc));

        RuleFor(x => x.SourceId)
            .NotEmpty()
            .WithMessage(loc.Get(RS.ValidReceiptSourceId));

        RuleFor(x => x.StoreId)
            .NotEmpty()
            .WithMessage(loc.Get(RS.ValidReceiptStoreId));

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage(loc.Get(RS.ValidReceiptItemsAtLeastOneItem))
            .Must(items => items.All(item => item.ProductId.HasValue && item.Quantity != 0 && item.Amount != 0))
            .WithMessage(loc.Get(RS.ValidReceiptItemsAllItemsFilled))
            .Must(items =>
            {
                var productIds = items.Where(i => i.ProductId.HasValue)
                .Select(i => new
                {
                    productId = i.ProductId!.Value,
                    quantitySign = i.Quantity >= 0,
                    amountSign = i.Amount >= 0
                });
                return productIds.Distinct().Count() == productIds.Count();
            })
            .WithMessage(loc.Get(RS.ValidReceiptItemsNoDuplicateProducts));

        RuleFor(x => x)
            .MustAsync(ValidateSourceId)
            .WithMessage(loc.Get(RS.ValidReceiptItemsValidateSourceId))
            .MustAsync(ValidateStoreIdAndDate)
            .WithMessage(loc.Get(RS.ValidReceiptItemsValidateStoreIdAndDate));
    }

    private async Task<bool> ValidateSourceId(ReceiptDto dto, CancellationToken token)
    {
        return !await _receiptRepo.ExistsBySourceId(dto.UserId, dto.Id, dto.SourceId, token);
    }

    private async Task<bool> ValidateStoreIdAndDate(ReceiptDto dto, CancellationToken token)
    {
        return !await _receiptRepo.ExistsByStoreIdAndDate(dto.UserId, dto.StoreId, dto.TransactionDateUtc, dto.Id, token);
    }
}
