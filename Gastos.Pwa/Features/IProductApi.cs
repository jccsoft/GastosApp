namespace Gastos.Pwa.Features;

/// <summary>
/// API interface for product operations
/// </summary>
public interface IProductApi
{
    [Get(GastosApiEndpoints.Products.GetAll)]
    Task<ApiResponse<ApiPagedResponse<ProductDto>>> GetAllAsync([Query] ProductParameters parameters, CancellationToken token = default);

    [Get(GastosApiEndpoints.Products.Get)]
    Task<ApiResponse<ProductDto?>> GetByIdAsync(Guid id);

    [Post(GastosApiEndpoints.Products.Create)]
    Task<ApiResponse<bool>> CreateAsync([Body] ProductDto newProduct);

    [Put(GastosApiEndpoints.Products.Update)]
    Task<ApiResponse<bool>> UpdateAsync([Body] ProductDto updatedProduct);

    [Delete(GastosApiEndpoints.Products.Delete)]
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
}