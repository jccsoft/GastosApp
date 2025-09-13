namespace Gastos.Api.Features.Sizings;

public interface ISizingRepository
{
    Task<List<Sizing>> GetAllAsync(CancellationToken token);
    Task<Sizing?> GetByIdAsync(int id, CancellationToken token);
}
