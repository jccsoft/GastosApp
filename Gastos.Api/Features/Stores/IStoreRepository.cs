namespace Gastos.Api.Features.Stores;

public interface IStoreRepository
{
    Task<ApiPagedResponse<Store>> GetAllAsync(string userId, StoreParameters parameters, CancellationToken token);
    Task<Store?> GetByIdAsync(string userId, Guid id, CancellationToken token);
    Task<RepoResult> CreateAsync(string userId, Store newStore, CancellationToken token);
    Task<RepoResult> UpdateAsync(string userId, Store updatedStore, CancellationToken token);
    Task<RepoResult> DeleteAsync(string userId, Guid id, CancellationToken token);
    Task<bool> ExistsByName(string userId, string name, Guid idToExclude, CancellationToken token);
}
