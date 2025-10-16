namespace Gastos.Api.Features.Products;

public sealed class ProductServerValidator : ProductClientValidator
{
    private readonly IProductRepository _productRepo;

    public ProductServerValidator(LocalizationService loc, IProductRepository productRepo) : base(loc)
    {
        _productRepo = productRepo;

        RuleFor(x => x)
            .MustAsync(ValidateUnique)
            .WithMessage(loc.Get(RS.ValidProductUnique));
    }

    private async Task<bool> ValidateUnique(ProductDto dto, CancellationToken token)
    {
        return !await _productRepo.Exists(dto.UserId, dto.Name, dto.UnitsPack, dto.SizingId, dto.SizingValue, dto.Id, token);
    }
}

