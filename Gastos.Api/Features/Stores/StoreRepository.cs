namespace Gastos.Api.Features.Stores;

public class StoreRepository(AppDbContext context) : IStoreRepository
{
    private readonly bool isInMemoryDatabase = context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";

    public async Task<ApiPagedResponse<Store>> GetAllAsync(string userId, StoreParameters parameters, CancellationToken token)
    {
        var query = context.Stores
            .Where(s => s.UserId == userId)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(parameters.SearchString))
        {
            var search = parameters.SearchString.Trim().ToLower().RemoveAccents();

            if (isInMemoryDatabase)
            {
                query = query.Where(s => s.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase));
            }
            else
            {
                query = query.Where(s =>
                    EF.Functions.Like(
                        EF.Functions.Unaccent(s.Name).ToLower(),
                        $"%{search}%"
                    )
                );
            }
        }

        query = query.OrderBy(s => s.Name);

        var pagedResponse = await ApiPagedResponse<Store>.CreateAsync(
            query,
            parameters.Page,
            parameters.PageSize);

        return pagedResponse;
    }

    public async Task<Store?> GetByIdAsync(string userId, Guid id, CancellationToken token)
    {
        return await context.Stores
            .Where(s => s.UserId == userId)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, token);
    }

    public async Task<RepoResult> CreateAsync(string userId, Store newStore, CancellationToken token)
    {
        newStore.UserId = userId;
        try
        {
            await context.Stores.AddAsync(newStore, token);
            int affectedRows = await context.SaveChangesAsync(token);
            return affectedRows > 0 ? RepoResult.Success : RepoResult.Error;
        }
        catch (Exception ex)
        {
            return ex.IsIntegrityConstraintViolation() ? RepoResult.Restricted : RepoResult.Error;
        }
    }

    public async Task<RepoResult> UpdateAsync(string userId, Store updatedStore, CancellationToken token)
    {
        try
        {
            var existingStore = await GetByIdAsync(userId, updatedStore.Id, token);
            if (existingStore is null) return RepoResult.NotFound;

            context.Stores.Update(updatedStore);
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
            var existingStore = await context.Stores
                .Where(s => s.UserId == userId)
                .FirstOrDefaultAsync(s => s.Id == id, token);
            if (existingStore is null) return RepoResult.NotFound;

            context.Stores.Remove(existingStore);
            int affectedRows = await context.SaveChangesAsync(token);
            return affectedRows > 0 ? RepoResult.Success : RepoResult.NotFound;
        }
        catch (Exception ex)
        {
            return ex.IsIntegrityConstraintViolation() ? RepoResult.Restricted : RepoResult.Error;
        }
    }

    public async Task<bool> ExistsByName(string userId, string name, Guid idToExclude, CancellationToken token)
    {
        if (isInMemoryDatabase)
        {
            return await context.Stores
                .Where(s => s.UserId == userId)
                .AsNoTracking()
                .AnyAsync(s => s.Id != idToExclude &&
                               s.Name.Trim().Equals(name.Trim(), StringComparison.CurrentCultureIgnoreCase), token);
        }
        else
        {
            return await context.Stores
                .Where(s => s.UserId == userId)
                .AsNoTracking()
                .AnyAsync(s => s.Id != idToExclude && EF.Functions.ILike(s.Name, name.Trim()), token);
        }

    }
}
