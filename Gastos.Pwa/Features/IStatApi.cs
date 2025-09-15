namespace Gastos.Pwa.Features;

public interface IStatApi
{
    [Get(GastosApiEndpoints.Stats.GetAll)]
    Task<ApiResponse<List<StatDto>>> GetStatsAsync(StatParameters parameters, CancellationToken token = default);
}
