namespace Gastos.Api.Features.Receipts;

public interface IReceiptRepository
{
    Task<ApiPagedResponse<Receipt>> GetAllAsync(string userId, ReceiptParameters parameters, CancellationToken token);
    Task<Receipt?> GetByIdAsync(string userId, Guid id, CancellationToken token);
    Task<RepoResult> CreateAsync(string userId, Receipt newReceipt, CancellationToken token);
    Task<RepoResult> UpdateAsync(string userId, Receipt updatedReceipt, CancellationToken token);
    Task<RepoResult> DeleteAsync(string userId, Guid id, CancellationToken token);
    Task<bool> ExistsBySourceId(string userId, Guid sourceId, Guid? receiptIdToExclude, CancellationToken token);
    Task<bool> ExistsByStoreIdAndDate(string userId, Guid? storeId, DateTime? transactionDateUtc, Guid? receiptIdToExclude, CancellationToken token);
    Task<bool> ExistsByStoreSourceNameAndDate(string userId, string storeSourceName, DateTime transactionDateUtc, CancellationToken token);
    Task<Product?> GetProductBySourceDescription(string userId, string sourceDescription, CancellationToken token);
}
