namespace Gastos.Pwa.Features;

/// <summary>
/// API interface for receipt operations
/// </summary>
public interface IReceiptApi
{
    [Get(GastosApiEndpoints.Receipts.GetAll)]
    Task<ApiResponse<ApiPagedResponse<ReceiptDto>>> GetAllAsync([Query] ReceiptParameters parameters, CancellationToken token = default);

    [Get(GastosApiEndpoints.Receipts.Get)]
    Task<ApiResponse<ReceiptDto>> GetByIdAsync(Guid id);

    [Post(GastosApiEndpoints.Receipts.Create)]
    Task<ApiResponse<ReceiptDto>> CreateAsync([Body] ReceiptDto newReceipt);

    [Put(GastosApiEndpoints.Receipts.Update)]
    Task<ApiResponse<bool>> UpdateAsync([Body] ReceiptDto updatedReceipt);

    [Delete(GastosApiEndpoints.Receipts.Delete)]
    Task<ApiResponse<bool>> DeleteAsync(Guid id);

    [Get(GastosApiEndpoints.Receipts.ExistsBySourceId)]
    Task<ApiResponse<bool>> ExistsBySourceIdAsync(Guid sourceId, Guid? receiptIdToExclude = null);

    [Get(GastosApiEndpoints.Receipts.ExistsByStoreIdAndDate)]
    Task<ApiResponse<bool>> ExistsByStoreIdAndDate(Guid storeId, DateTime transactionDateUtc, Guid? receiptIdToExclude = null);

    [Get(GastosApiEndpoints.Receipts.ExistsByStoreSourceNameAndDate)]
    Task<ApiResponse<bool>> ExistsByStoreSourceNameAndDate(string storeSourceName, DateTime transactionDateUtc);

    [Get(GastosApiEndpoints.Receipts.GetProductIdBySourceDescription)]
    Task<ApiResponse<ProductDto?>> GetProductBySourceDescription(string description);
}