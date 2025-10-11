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
            var userId = httpContext.GetUserId();

            try
            {
                var pagedProducts = await productRepo.GetAllAsync(userId, parameters, token);

                var response = pagedProducts.ToDto(product => product.ToDto());

                return TypedResults.Ok(response);
            }
            catch (Exception ex)
            {
                var logger = CreateLogger(loggerFactory);
                logger.LogError(ex, "Error en {Method} para el usuario {UserId} con parámetros SearchString: {SearchString}, EmptyImageUrl: {EmptyImageUrl}, Page: {Page}, PageSize: {PageSize}",
                    nameof(GastosApiEndpoints.Products.GetAll),
                    userId,
                    parameters.SearchString,
                    parameters.EmptyImageUrl,
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
            var userId = httpContext.GetUserId();

            try
            {
                var product = await productRepo.GetByIdAsync(userId, id, token);

                return product is null ? Results.NotFound() : TypedResults.Ok(product.ToDto());
            }
            catch (Exception ex)
            {
                var logger = CreateLogger(loggerFactory);
                logger.LogError(ex, "Error en {Method} para el usuario {UserId} con id {ProductId}",
                    nameof(GastosApiEndpoints.Products.Get),
                    userId,
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
            var userId = httpContext.GetUserId();

            try
            {
                if (options.Value.LockUnauthenticated && !httpContext.User.IsAuthenticated())
                    return Results.Unauthorized();


                var validationResult = await validator.ValidateAsync(newProduct, token);
                if (!validationResult.IsValid)
                    return validationResult.ToResult();


                var result = await productRepo.CreateAsync(userId, newProduct.ToEntity(), token);

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
                logger.LogError(ex, "Error en {Method} para el usuario {UserId} con ProductId: {ProductId}, Name: {ProductName}, SizingId: {SizingId}, UnitsPack: {UnitsPack}",
                    nameof(GastosApiEndpoints.Products.Create),
                    userId,
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
            var userId = httpContext.GetUserId();

            try
            {
                if (options.Value.LockUnauthenticated && !httpContext.User.IsAuthenticated())
                    return Results.Unauthorized();

                var validationResult = await validator.ValidateAsync(updatedProduct, token);
                if (!validationResult.IsValid)
                    return validationResult.ToResult();


                var result = await productRepo.UpdateAsync(userId, updatedProduct.ToEntity(), token);

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
                logger.LogError(ex, "Error en {Method} para el usuario {UserId} con ProductId: {ProductId}, Name: {ProductName}, SizingId: {SizingId}, UnitsPack: {UnitsPack}",
                    nameof(GastosApiEndpoints.Products.Update),
                    userId,
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
            var userId = httpContext.GetUserId();

            try
            {
                if (options.Value.LockUnauthenticated && !httpContext.User.IsAuthenticated())
                    return Results.Unauthorized();

                var result = await productRepo.DeleteAsync(userId, id, token);

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
                logger.LogError(ex, "Error en {Method} para el usuario {UserId} con id {ProductId}",
                    nameof(GastosApiEndpoints.Products.Delete),
                    userId,
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
