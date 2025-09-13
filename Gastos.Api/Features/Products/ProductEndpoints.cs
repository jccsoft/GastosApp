namespace Gastos.Api.Features.Products;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet(GastosApiEndpoints.Products.GetAll, async (HttpContext httpContext,
            [AsParameters] ProductParameters parameters,
            [FromServices] IProductRepository productRepo,
            [FromServices] ILoggerFactory loggerFactory,
            CancellationToken token) =>
        {
            try
            {
                var pagedProducts = await productRepo.GetAllAsync(httpContext.GetUserId(), parameters, token);

                var response = pagedProducts.ToDto(product => product.ToDto());

                return TypedResults.Ok(response);
            }
            catch (Exception ex)
            {
                var logger = CreateLogger(loggerFactory);
                logger.LogError(ex, "Error en {Method} para el usuario {User} con parámetros SearchString: {SearchString}, Page: {Page}, PageSize: {PageSize}",
                    nameof(GastosApiEndpoints.Products.GetAll),
                    httpContext.GetUserId(),
                    parameters.SearchString,
                    parameters.Page,
                    parameters.PageSize);
                return Results.Problem();
            }
        });

        app.MapGet(GastosApiEndpoints.Products.Get, async (HttpContext httpContext,
            Guid id,
            [FromServices] IProductRepository productRepo,
            [FromServices] ILoggerFactory loggerFactory,
            CancellationToken token) =>
        {
            try
            {
                var product = await productRepo.GetByIdAsync(httpContext.GetUserId(), id, token);

                return product is null ? Results.NotFound() : TypedResults.Ok(product.ToDto());
            }
            catch (Exception ex)
            {
                var logger = CreateLogger(loggerFactory);
                logger.LogError(ex, "Error en {Method} para el usuario {User} con id {Id}",
                    nameof(GastosApiEndpoints.Products.Get),
                    httpContext.GetUserId(),
                    id);
                return Results.Problem();
            }
        });

        app.MapPost(GastosApiEndpoints.Products.Create, async (HttpContext httpContext,
            [FromBody] ProductDto newProduct,
            ProductServerValidator validator,
            [FromServices] IProductRepository productRepo,
            [FromServices] IOptions<GastosApiOptions> options,
            [FromServices] ILoggerFactory loggerFactory,
            CancellationToken token) =>
        {
            try
            {
                if (options.Value.LockUnauthenticated && !httpContext.User.IsAuthenticated())
                    return Results.Unauthorized();

                var validationResult = await validator.ValidateAsync(newProduct, token);
                if (!validationResult.IsValid)
                    return validationResult.ToResult();

                var result = await productRepo.CreateAsync(httpContext.GetUserId(), newProduct.ToEntity(), token);

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
                logger.LogError(ex, "Error en {Method} para el usuario {User} con ProductId: {ProductId}, Name: {Name}, SizingId: {SizingId}, UnitsPack: {UnitsPack}",
                    nameof(GastosApiEndpoints.Products.Create),
                    httpContext.GetUserId(),
                    newProduct.Id,
                    newProduct.Name,
                    newProduct.SizingId,
                    newProduct.UnitsPack);
                return Results.Problem();
            }
        });

        app.MapPut(GastosApiEndpoints.Products.Update, async (HttpContext httpContext,
            [FromBody] ProductDto updatedProduct,
            [FromServices] ProductServerValidator validator,
            [FromServices] IProductRepository productRepo,
            [FromServices] IOptions<GastosApiOptions> options,
            [FromServices] ILoggerFactory loggerFactory,
            CancellationToken token) =>
        {
            try
            {
                if (options.Value.LockUnauthenticated && !httpContext.User.IsAuthenticated())
                    return Results.Unauthorized();

                var validationResult = await validator.ValidateAsync(updatedProduct, token);
                if (!validationResult.IsValid)
                    return validationResult.ToResult();

                var result = await productRepo.UpdateAsync(httpContext.GetUserId(), updatedProduct.ToEntity(), token);

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
                logger.LogError(ex, "Error en {Method} para el usuario {User} con ProductId: {ProductId}, Name: {Name}, SizingId: {SizingId}, UnitsPack: {UnitsPack}",
                    nameof(GastosApiEndpoints.Products.Update),
                    httpContext.GetUserId(),
                    updatedProduct.Id,
                    updatedProduct.Name,
                    updatedProduct.SizingId,
                    updatedProduct.UnitsPack);
                return Results.Problem();
            }
        });

        app.MapDelete(GastosApiEndpoints.Products.Delete, async (HttpContext httpContext,
            Guid id,
            [FromServices] IProductRepository productRepo,
            [FromServices] IOptions<GastosApiOptions> options,
            [FromServices] ILoggerFactory loggerFactory,
            CancellationToken token) =>
        {
            try
            {
                if (options.Value.LockUnauthenticated && !httpContext.User.IsAuthenticated())
                    return Results.Unauthorized();

                var result = await productRepo.DeleteAsync(httpContext.GetUserId(), id, token);

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
                    nameof(GastosApiEndpoints.Products.Delete),
                    httpContext.GetUserId(),
                    id);
                return Results.Problem();
            }
        });

        return app;
    }

    private static ILogger CreateLogger(ILoggerFactory loggerFactory)
    {
        return loggerFactory.CreateLogger(nameof(ProductEndpoints));
    }
}
