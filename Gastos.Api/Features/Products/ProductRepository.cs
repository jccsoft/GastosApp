namespace Gastos.Api.Features.Products;

public class ProductRepository(AppDbContext context) : IProductRepository
{
    private readonly bool isInMemoryDatabase = context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";

    public async Task<ApiPagedResponse<Product>> GetAllAsync(string userId, ProductParameters parameters, CancellationToken token)
    {
        var query = context.Products
            .Where(p => p.UserId == userId)
            .Include(p => p.Sizing)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(parameters.SearchString))
        {
            var search = parameters.SearchString.Trim().ToLower().RemoveAccents();

            if (isInMemoryDatabase)
            {
                query = query.Where(p => p.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase));
            }
            else
            {
                query = query.Where(p =>
                    EF.Functions.Like(
                        EF.Functions.Unaccent(p.Name).ToLower(),
                        $"%{search}%"
                    )
                );
            }
        }

        query = query.OrderBy(p => p.Name);

        var pagedResponse = await ApiPagedResponse<Product>.CreateAsync(
            query,
            parameters.Page,
            parameters.PageSize);

        return pagedResponse;
    }

    public async Task<Product?> GetByIdAsync(string userId, Guid id, CancellationToken token)
    {
        return await context.Products
            .Where(p => p.UserId == userId)
            .AsNoTracking()
            .Include(p => p.Sizing)
            .FirstOrDefaultAsync(p => p.Id == id, token);
    }

    public async Task<RepoResult> CreateAsync(string userId, Product newProduct, CancellationToken token)
    {
        newProduct.UserId = userId;
        try
        {
            await context.Products.AddAsync(newProduct, token);
            int affectedRows = await context.SaveChangesAsync(token);
            return affectedRows > 0 ? RepoResult.Success : RepoResult.Error;
        }
        catch (Exception ex)
        {
            return ex.IsIntegrityConstraintViolation() ? RepoResult.Restricted : RepoResult.Error;
        }
    }

    public async Task<RepoResult> UpdateAsync(string userId, Product updatedProduct, CancellationToken token)
    {
        try
        {
            var existingProduct = await GetByIdAsync(userId, updatedProduct.Id, token);
            if (existingProduct is null) return RepoResult.NotFound;

            updatedProduct.Sizing = null;
            context.Products.Update(updatedProduct);
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
            var existingProduct = await context.Products
                .Where(p => p.UserId == userId)
                .Include(p => p.Sizing)
                .FirstOrDefaultAsync(p => p.Id == id, token);
            if (existingProduct is null) return RepoResult.NotFound;

            context.Products.Remove(existingProduct);

            int affectedRows = await context.SaveChangesAsync(token);
            return affectedRows > 0 ? RepoResult.Success : RepoResult.NotFound;
        }
        catch (Exception ex)
        {
            return ex.IsIntegrityConstraintViolation() ?
                RepoResult.Restricted :
                RepoResult.Error;
        }
    }

    public async Task<bool> ExistsByNameAndUnitsPack(string userId, string name, int unitsPack, Guid? productIdToExclude, CancellationToken token)
    {
        return await context.Products
            .Where(p => p.UserId == userId)
            .AsNoTracking()
            .AnyAsync(p => p.Id != productIdToExclude &&
                           EF.Functions.ILike(p.Name, name.Trim()) &&
                           p.UnitsPack == unitsPack, token);
    }
}
