namespace Gastos.Api.Features.Products;

public interface IProductRepository
{
    Task<ApiPagedResponse<Product>> GetAllAsync(string userId, ProductParameters parameters, CancellationToken token);
    Task<Product?> GetByIdAsync(string userId, Guid id, CancellationToken token);
    Task<RepoResult> CreateAsync(string userId, Product newProduct, CancellationToken token);
    Task<RepoResult> UpdateAsync(string userId, Product updatedProduct, CancellationToken token);
    Task<RepoResult> DeleteAsync(string userId, Guid id, CancellationToken token);
    Task<bool> Exists(string userId, string name, int unitsPack, int? sizingId, decimal? sizingValue, Guid? productIdToExclude, CancellationToken token);
}
