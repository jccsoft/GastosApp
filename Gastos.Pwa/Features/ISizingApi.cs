namespace Gastos.Pwa.Features;

public interface ISizingApi
{
    [Get(GastosApiEndpoints.Sizings.GetAll)]
    Task<ApiResponse<List<SizingDto>>> GetAllAsync();
}
