namespace Gastos.Api.Features.Stores;

public static class StoreEndpoints
{
    public static IEndpointRouteBuilder MapStoreEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet(GastosApiEndpoints.Stores.GetAll, async (HttpContext httpContext,
            [AsParameters] StoreParameters parameters,
            [FromServices] IStoreRepository storeRepo,
            [FromServices] ILoggerFactory loggerFactory,
            CancellationToken token) =>
        {
            var userId = httpContext.GetUserId();

            try
            {
                var pagedStores = await storeRepo.GetAllAsync(userId, parameters, token);

                var response = pagedStores.ToDto(store => store.ToDto());

                return TypedResults.Ok(response);
            }
            catch (Exception ex)
            {
                var logger = CreateLogger(loggerFactory);
                logger.LogError(ex, "Error en {Method} para el usuario {User} con parámetros SearchString: {SearchString}, Page: {Page}, PageSize: {PageSize}",
                    nameof(GastosApiEndpoints.Stores.GetAll),
                    userId,
                    parameters.SearchString,
                    parameters.Page,
                    parameters.PageSize);
                return Results.Problem();
            }
        });

        app.MapGet(GastosApiEndpoints.Stores.Get, async (HttpContext httpContext,
            Guid id,
            [FromServices] IStoreRepository storeRepo,
            [FromServices] ILoggerFactory loggerFactory,
            CancellationToken token) =>
        {
            var userId = httpContext.GetUserId();

            try
            {
                var store = await storeRepo.GetByIdAsync(userId, id, token);

                return store is null ? Results.NotFound() : TypedResults.Ok(store.ToDto());
            }
            catch (Exception ex)
            {
                var logger = CreateLogger(loggerFactory);
                logger.LogError(ex, "Error en {Method} para el usuario {User} con id {Id}",
                    nameof(GastosApiEndpoints.Stores.Get),
                    userId,
                    id);
                return Results.Problem();
            }
        });

        app.MapPost(GastosApiEndpoints.Stores.Create, async (HttpContext httpContext,
            [FromBody] StoreDto newStore,
            StoreServerValidator validator,
            [FromServices] IStoreRepository storeRepo,
            [FromServices] IOptions<GastosApiOptions> options,
            [FromServices] ILoggerFactory loggerFactory,
            CancellationToken token) =>
        {
            var userId = httpContext.GetUserId();

            try
            {
                if (options.Value.LockUnauthenticated && !httpContext.User.IsAuthenticated())
                    return Results.Unauthorized();

                var validationResult = await validator.ValidateAsync(newStore, token);
                if (!validationResult.IsValid)
                    return validationResult.ToResult();

                var result = await storeRepo.CreateAsync(userId, newStore.ToEntity(), token);

                return result switch
                {
                    RepoResult.Success => TypedResults.Ok(true),
                    RepoResult.Restricted => Results.Conflict(),
                    _ => Results.Problem()
                };
            }
            catch (Exception ex)
            {
                var logger = CreateLogger(loggerFactory);
                logger.LogError(ex, "Error en {Method} para el usuario {User} con StoreId: {StoreId}, Name: {Name}, SourceName: {SourceName}",
                    nameof(GastosApiEndpoints.Stores.Create),
                    userId,
                    newStore.Id,
                    newStore.Name,
                    newStore.SourceName);
                return Results.Problem();
            }
        });

        app.MapPut(GastosApiEndpoints.Stores.Update, async (HttpContext httpContext,
            [FromBody] StoreDto updatedStore,
            StoreServerValidator validator,
            [FromServices] IStoreRepository storeRepo,
            [FromServices] IOptions<GastosApiOptions> options,
            [FromServices] ILoggerFactory loggerFactory,
            CancellationToken token) =>
        {
            var userId = httpContext.GetUserId();

            try
            {
                if (options.Value.LockUnauthenticated && !httpContext.User.IsAuthenticated())
                    return Results.Unauthorized();

                var validationResult = await validator.ValidateAsync(updatedStore, token);
                if (!validationResult.IsValid)
                    return validationResult.ToResult();

                var result = await storeRepo.UpdateAsync(userId, updatedStore.ToEntity(), token);

                return result switch
                {
                    RepoResult.Success => TypedResults.Ok(true),
                    RepoResult.Restricted => Results.Conflict(),
                    RepoResult.NotFound => Results.NotFound(),
                    _ => Results.Problem()
                };
            }
            catch (Exception ex)
            {
                var logger = CreateLogger(loggerFactory);
                logger.LogError(ex, "Error en {Method} para el usuario {User} con StoreId: {StoreId}, Name: {Name}, SourceName: {SourceName}",
                    nameof(GastosApiEndpoints.Stores.Update),
                    userId,
                    updatedStore.Id,
                    updatedStore.Name,
                    updatedStore.SourceName);
                return Results.Problem();
            }
        });

        app.MapDelete(GastosApiEndpoints.Stores.Delete, async (HttpContext httpContext,
            Guid id,
            [FromServices] IStoreRepository storeRepo,
            [FromServices] IOptions<GastosApiOptions> options,
            [FromServices] ILoggerFactory loggerFactory,
            CancellationToken token) =>
        {
            var userId = httpContext.GetUserId();

            try
            {
                if (options.Value.LockUnauthenticated && !httpContext.User.IsAuthenticated())
                    return Results.Unauthorized();

                var result = await storeRepo.DeleteAsync(userId, id, token);

                return result switch
                {
                    RepoResult.Success => TypedResults.Ok(true),
                    RepoResult.Restricted => Results.Conflict(),
                    RepoResult.NotFound => Results.NotFound(),
                    _ => Results.Problem()
                };
            }
            catch (Exception ex)
            {
                var logger = CreateLogger(loggerFactory);
                logger.LogError(ex, "Error en {Method} para el usuario {User} con id {Id}",
                    nameof(GastosApiEndpoints.Stores.Delete),
                    userId,
                    id);
                return Results.Problem();
            }
        });

        return app;
    }

    private static ILogger CreateLogger(ILoggerFactory loggerFactory)
    {
        return loggerFactory.CreateLogger(nameof(StoreEndpoints));
    }
}
