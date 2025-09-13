namespace Gastos.Api.Features.Products;

public sealed class ProductServerValidator : ProductClientValidator
{
    private readonly IProductRepository _productRepo;

    public ProductServerValidator(LocalizationService loc, IProductRepository productRepo) : base(loc)
    {
        _productRepo = productRepo;

        RuleFor(x => x)
            .MustAsync(ValidateUniqueNameAndUnitsPack)
            .WithMessage(loc.Get(RS.ValidProductUniqueNameAndUnitsPack));
    }

    private async Task<bool> ValidateUniqueNameAndUnitsPack(ProductDto dto, CancellationToken token)
    {
        return !await _productRepo.ExistsByNameAndUnitsPack(dto.UserId, dto.Name, dto.UnitsPack, dto.Id, token);
    }
}

