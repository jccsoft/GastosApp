namespace Gastos.Pwa.Features;

/// <summary>
/// API interface for store operations
/// </summary>
public interface IStoreApi
{
    [Get(GastosApiEndpoints.Stores.GetAll)]
    Task<ApiResponse<ApiPagedResponse<StoreDto>>> GetAllAsync([Query] StoreParameters parameters, CancellationToken token = default);

    [Get(GastosApiEndpoints.Stores.Get)]
    Task<ApiResponse<StoreDto?>> GetByIdAsync(Guid id);

    [Post(GastosApiEndpoints.Stores.Create)]
    Task<ApiResponse<bool>> CreateAsync([Body] StoreDto newStore);

    [Put(GastosApiEndpoints.Stores.Update)]
    Task<ApiResponse<bool>> UpdateAsync([Body] StoreDto updatedStore);

    [Delete(GastosApiEndpoints.Stores.Delete)]
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
}