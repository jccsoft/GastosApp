namespace Gastos.Api.Features.Receipts;

public class ReceiptRepository(AppDbContext context) : IReceiptRepository
{
    private readonly bool isInMemoryDatabase = context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";

    public async Task<ApiPagedResponse<Receipt>> GetAllAsync(string userId, ReceiptParameters parameters, CancellationToken token)
    {
        IQueryable<Receipt>? query;

        if (parameters.ProductId.HasValue && parameters.ProductId.Value != Guid.Empty)
        {
            // When filtering by ProductId, we can optimize the query to only include receipts that have that product
            query = context.Receipts
                .Where(r => r.UserId == userId && r.Items.Any(ri => ri.ProductId == parameters.ProductId.Value))
                .Include(r => r.Store)
                .Include(r => r.Items.Where(ri => ri.ProductId == parameters.ProductId.Value))
                .ThenInclude(ri => ri.Product!)
                .ThenInclude(p => p.Sizing!.Parent)
                .AsNoTracking();
        }
        else
        {
            query = context.Receipts
                .Where(r => r.UserId == userId)
                .Include(r => r.Store)
                .Include(r => r.Items)
                .ThenInclude(ri => ri.Product!)
                .ThenInclude(p => p.Sizing!.Parent)
                .AsNoTracking();
        }

        if (!string.IsNullOrWhiteSpace(parameters.ProductName))
        {
            string search = parameters.ProductName.Trim().ToLower().RemoveAccents();
            if (isInMemoryDatabase)
            {
                query = query.Where(r => r.Items
                    .Any(ri => ri.Product != null &&
                         ri.Product.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase)));
            }
            else
            {
                query = query.Where(r => r.Items
                    .Any(ri => ri.Product != null &&
                    EF.Functions.ILike(
                        EF.Functions.Unaccent(ri.Product.Name).ToLower(),
                    $"%{search}%")));
            }
        }

        if (parameters.FromDateUtc.HasValue)
            query = query.Where(r => r.TransactionDateUtc >= parameters.FromDateUtc.Value);

        if (parameters.ToDateUtc.HasValue)
            query = query.Where(r => r.TransactionDateUtc <= parameters.ToDateUtc.Value);

        query = query.OrderByDescending(r => r.TransactionDateUtc);

        var pagedResponse = await ApiPagedResponse<Receipt>.CreateAsync(
            userId,
            query,
            parameters.Page,
            parameters.PageSize);

        return pagedResponse;
    }

    public async Task<Receipt?> GetByIdAsync(string userId, Guid id, CancellationToken token)
    {
        var receipt = await context.Receipts
            .Where(r => r.UserId == userId)
            .Include(r => r.Store)
            .Include(r => r.Items)
            .ThenInclude(ri => ri.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, token);

        return receipt;
    }

    public async Task<RepoResult> CreateAsync(string userId, Receipt newReceipt, CancellationToken token)
    {
        newReceipt.UserId = userId;
        newReceipt.Store = null; // Ensure Store is not set to avoid circular reference issues
        newReceipt.Items.RemoveAll(ri => ri.Quantity == 0); // Remove items with zero quantity
        try
        {
            await context.Receipts.AddAsync(newReceipt, token);

            int affectedRows = await context.SaveChangesAsync(token);

            return affectedRows > 0 ? RepoResult.Success : RepoResult.Error;
        }
        catch (Exception ex)
        {
            return ex.IsIntegrityConstraintViolation() ? RepoResult.Restricted : RepoResult.Error;
        }
    }

    public async Task<RepoResult> UpdateAsync(string userId, Receipt updatedReceipt, CancellationToken token)
    {
        try
        {
            var existingReceipt = await context.Receipts
                .Where(r => r.UserId == userId)
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == updatedReceipt.Id, token);

            if (existingReceipt is null) return RepoResult.NotFound;


            // Find items to delete (those present in DB but not in the updated list)
            var itemsToDelete = existingReceipt.Items
                .Where(dbItem => !updatedReceipt.Items.Any(ui => ui.Id == dbItem.Id))
                .ToList();

            foreach (var item in itemsToDelete)
            {
                context.ReceiptItems.Remove(item);
            }

            // Update existing items and add new ones
            foreach (var updatedItem in updatedReceipt.Items)
            {
                var existingItem = existingReceipt.Items.FirstOrDefault(i => i.Id == updatedItem.Id);
                if (existingItem != null)
                {
                    // Update properties as needed
                    context.Entry(existingItem).CurrentValues.SetValues(updatedItem);
                }
                else
                {
                    // New item
                    existingReceipt.Items.Add(updatedItem);
                }
            }

            // Update other receipt properties as needed
            context.Entry(existingReceipt).CurrentValues.SetValues(updatedReceipt);

            int affectedRows = await context.SaveChangesAsync(token);
            return affectedRows > 0 ? RepoResult.Success : RepoResult.NoChange;
        }
        catch (Exception ex)
        {
            return ex.IsIntegrityConstraintViolation() ? RepoResult.Restricted : RepoResult.Error;
        }
    }

    public async Task<RepoResult> DeleteAsync(string userId, Guid id, CancellationToken token)
    {
        try
        {
            var existingReceipt = await context.Receipts
                .Where(r => r.UserId == userId)
                .FirstOrDefaultAsync(r => r.Id == id, token);

            if (existingReceipt is null) return RepoResult.NotFound;


            context.Receipts.Remove(existingReceipt);

            int affectedRows = await context.SaveChangesAsync(token);

            return affectedRows > 0 ? RepoResult.Success : RepoResult.NotFound;
        }
        catch (Exception ex)
        {
            return ex.IsIntegrityConstraintViolation() ? RepoResult.Restricted : RepoResult.Error;
        }
    }

    public async Task<bool> ExistsBySourceId(string userId, Guid sourceId, Guid? receiptIdToExclude, CancellationToken token)
    {
        receiptIdToExclude ??= Guid.Empty;

        bool exists = await context.Receipts
            .Where(r => r.UserId == userId)
            .AsNoTracking()
            .AnyAsync(r => r.SourceId == sourceId && r.Id != receiptIdToExclude, token);

        return exists;
    }

    public async Task<bool> ExistsByStoreIdAndDate(string userId, Guid? storeId, DateTime? transactionDateUtc, Guid? receiptIdToExclude, CancellationToken token)
    {
        if (storeId is null || transactionDateUtc is null)
        {
            return false;
        }

        bool exists = await context.Receipts
            .Where(r => r.UserId == userId)
            .AsNoTracking()
            .AnyAsync(r => r.Id != receiptIdToExclude && r.StoreId == storeId && r.TransactionDateUtc == transactionDateUtc, token);

        return exists;
    }

    public async Task<bool> ExistsByStoreSourceNameAndDate(string userId, string storeSourceName, DateTime transactionDateUtc, CancellationToken token)
    {
        if (string.IsNullOrEmpty(storeSourceName))
        {
            return false;
        }

        bool exists = await context.Receipts
            .Where(r => r.UserId == userId)
            .Include(r => r.Store)
            .AsNoTracking()
            .AnyAsync(r =>
                r.Store != null && r.Store.SourceName != null &&
                r.Store.SourceName.ToLower() == storeSourceName.ToLower() &&
                r.TransactionDateUtc.HasValue &&
                r.TransactionDateUtc.Value == transactionDateUtc,
                token);

        return exists;
    }

    public async Task<Product?> GetProductBySourceDescription(string userId, string sourceDescription, CancellationToken token)
    {
        var product = await context.ReceiptItems
            .Include(ri => ri.Product)
            .Include(ri => ri.Receipt)
            .Where(ri => ri.Receipt.UserId == userId)
            .Where(ri => ri.SourceDescription != null &&
                         ri.SourceDescription.ToLower() == sourceDescription.ToLower())
            .AsNoTracking()
            .Select(ri => ri.Product)
            .FirstOrDefaultAsync(token);

        return product;
    }
}
