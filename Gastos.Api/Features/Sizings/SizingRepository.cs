namespace Gastos.Api.Features.Sizings;

public class SizingRepository(AppDbContext context) : ISizingRepository
{
    public async Task<List<Sizing>> GetAllAsync(CancellationToken token)
    {
        return await context.Sizings.AsNoTracking().ToListAsync(token);
    }

    public async Task<Sizing?> GetByIdAsync(int id, CancellationToken token)
    {
        return await context.Sizings.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, token);
    }
}
