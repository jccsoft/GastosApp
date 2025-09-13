namespace Gastos.Api.Features.Stats;

public interface IStatRepository
{
    Task<List<Stat>> GetStatsAsync(string userId, StatParameters parameters, CancellationToken token);
}