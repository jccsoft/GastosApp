namespace Gastos.Api.Features.Stores;

public sealed class StoreServerValidator : StoreClientValidator
{
    private readonly IStoreRepository _StoreRepo;

    public StoreServerValidator(LocalizationService loc, IStoreRepository StoreRepo) : base(loc)
    {
        _StoreRepo = StoreRepo;

        RuleFor(x => x)
            .MustAsync(ValidateUniqueName)
            .WithMessage(loc.Get(RS.ValidStoreUniqueName));
    }

    private async Task<bool> ValidateUniqueName(StoreDto dto, CancellationToken token)
    {
        return !await _StoreRepo.ExistsByName(dto.UserId, dto.Name, dto.Id, token);
    }
}

