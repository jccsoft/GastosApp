namespace Gastos.Api.Features.Stats;

public static class StatEndpoints
{
    public static IEndpointRouteBuilder MapStatEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet(GastosApiEndpoints.Stats.GetAll, async (HttpContext httpContext,
            [AsParameters] StatParameters parameters,
            [FromServices] IStatRepository statRepo,
            [FromServices] ILoggerFactory loggerFactory,
            CancellationToken token) =>
        {
            try
            {
                var stats = await statRepo.GetStatsAsync(httpContext.GetUserId(), parameters, token);

                return TypedResults.Ok(stats.Select(s => s.ToDto()));
            }
            catch (Exception ex)
            {
                var logger = CreateLogger(loggerFactory);
                logger.LogError(ex, "Error en {Method} para el usuario {User} con parámetros Period: {Period}, DateStartUtc: {DateStartUtc}, DateEndUtc: {DateEndUtc}",
                    nameof(GastosApiEndpoints.Stats.GetAll),
                    httpContext.GetUserId(),
                    parameters.Period,
                    parameters.DateStartUtc,
                    parameters.DateEndUtc);
                return Results.Problem();
            }
        });

        return app;
    }

    private static ILogger CreateLogger(ILoggerFactory loggerFactory)
    {
        return loggerFactory.CreateLogger(nameof(StatEndpoints));
    }
}