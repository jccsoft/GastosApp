namespace Gastos.Api.Features.Sizings;

public static class SizingEndpoints
{
    public static IEndpointRouteBuilder MapSizingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet(GastosApiEndpoints.Sizings.GetAll, async (
            [FromServices] ISizingRepository sizingRepo,
            [FromServices] ILoggerFactory loggerFactory,
            CancellationToken token) =>
        {
            try
            {
                var sizings = await sizingRepo.GetAllAsync(token);

                return TypedResults.Ok(sizings.Select(s => s.ToDto()));
            }
            catch (Exception ex)
            {
                var logger = CreateLogger(loggerFactory);
                logger.LogError(ex, "Error en {Method}",
                    nameof(GastosApiEndpoints.Sizings.GetAll));
                return Results.Problem();
            }
        });

        app.MapGet(GastosApiEndpoints.Sizings.Get, async (
            int id,
            [FromServices] ISizingRepository sizingRepo,
            [FromServices] ILoggerFactory loggerFactory,
            CancellationToken token) =>
        {
            try
            {
                var sizing = await sizingRepo.GetByIdAsync(id, token);

                return sizing is null ? Results.NotFound() : TypedResults.Ok(sizing.ToDto());
            }
            catch (Exception ex)
            {
                var logger = CreateLogger(loggerFactory);
                logger.LogError(ex, "Error en {Method} con id {Id}",
                    nameof(GastosApiEndpoints.Sizings.Get),
                    id);
                return Results.Problem();
            }
        });

        return app;
    }

    private static ILogger CreateLogger(ILoggerFactory loggerFactory)
    {
        return loggerFactory.CreateLogger(nameof(SizingEndpoints));
    }
}