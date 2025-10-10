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
            var logger = CreateLogger(loggerFactory);
            var userId = httpContext.GetUserId();
            
            using var scope = logger.BeginScope(new Dictionary<string, object>
            {
                ["UserId"] = userId,
                ["Method"] = nameof(GastosApiEndpoints.Products.GetAll),
                ["SearchString"] = parameters.SearchString ?? "",
                ["Page"] = parameters.Page,
                ["PageSize"] = parameters.PageSize
            });

            try
            {
                logger.LogDebug("Iniciando obtención de productos para usuario {UserId} con parámetros: SearchString={SearchString}, Page={Page}, PageSize={PageSize}",
                    userId, parameters.SearchString, parameters.Page, parameters.PageSize);

                var pagedProducts = await productRepo.GetAllAsync(userId, parameters, token);

                logger.LogDebug("Productos obtenidos exitosamente. Total items: {TotalItems}, Página actual: {CurrentPage}",
                    pagedProducts.TotalItems, pagedProducts.Page);

                var response = pagedProducts.ToDto(product => product.ToDto());

                return TypedResults.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en {Method} para el usuario {UserId} con parámetros SearchString: {SearchString}, Page: {Page}, PageSize: {PageSize}",
                    nameof(GastosApiEndpoints.Products.GetAll),
                    userId,
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
            var logger = CreateLogger(loggerFactory);
            var userId = httpContext.GetUserId();
            
            using var scope = logger.BeginScope(new Dictionary<string, object>
            {
                ["UserId"] = userId,
                ["Method"] = nameof(GastosApiEndpoints.Products.Get),
                ["ProductId"] = id
            });

            try
            {
                logger.LogDebug("Obteniendo producto {ProductId} para usuario {UserId}", id, userId);

                var product = await productRepo.GetByIdAsync(userId, id, token);

                if (product is null)
                {
                    logger.LogDebug("Producto {ProductId} no encontrado para usuario {UserId}", id, userId);
                    return Results.NotFound();
                }

                logger.LogDebug("Producto {ProductId} encontrado exitosamente: {ProductName}", id, product.Name);
                return TypedResults.Ok(product.ToDto());
            }
            catch (Exception ex)
            {
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
            var logger = CreateLogger(loggerFactory);
            var userId = httpContext.GetUserId();
            
            using var scope = logger.BeginScope(new Dictionary<string, object>
            {
                ["UserId"] = userId,
                ["Method"] = nameof(GastosApiEndpoints.Products.Create),
                ["ProductId"] = newProduct.Id,
                ["ProductName"] = newProduct.Name ?? "Unknown",
                ["SizingId"] = newProduct.SizingId ?? 0,
                ["UnitsPack"] = newProduct.UnitsPack
            });

            try
            {
                logger.LogDebug("Creando producto para usuario {UserId}: {ProductName} (UnitsPack: {UnitsPack})",
                    userId, newProduct.Name, newProduct.UnitsPack);

                if (options.Value.LockUnauthenticated && !httpContext.User.IsAuthenticated())
                {
                    logger.LogWarning("Intento de crear producto sin autenticación para usuario {UserId}", userId);
                    return Results.Unauthorized();
                }

                logger.LogDebug("Validando producto antes de crear...");
                var validationResult = await validator.ValidateAsync(newProduct, token);
                if (!validationResult.IsValid)
                {
                    logger.LogWarning("Validación fallida para producto: {ValidationErrors}",
                        string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                    return validationResult.ToResult();
                }

                logger.LogDebug("Guardando producto en repositorio...");
                var result = await productRepo.CreateAsync(userId, newProduct.ToEntity(), token);

                logger.LogInformation("Resultado de creación de producto: {Result} para producto {ProductName}",
                    result, newProduct.Name);

                return result switch
                {
                    RepoResult.Success => TypedResults.Ok(true),
                    RepoResult.Restricted => Results.Conflict(),
                    _ => Results.Problem()
                };
            }
            catch (Exception ex)
            {
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
            var logger = CreateLogger(loggerFactory);
            var userId = httpContext.GetUserId();
            
            using var scope = logger.BeginScope(new Dictionary<string, object>
            {
                ["UserId"] = userId,
                ["Method"] = nameof(GastosApiEndpoints.Products.Update),
                ["ProductId"] = updatedProduct.Id,
                ["ProductName"] = updatedProduct.Name ?? "Unknown",
                ["SizingId"] = updatedProduct.SizingId ?? 0,
                ["UnitsPack"] = updatedProduct.UnitsPack
            });

            try
            {
                logger.LogDebug("Actualizando producto {ProductId} para usuario {UserId}: {ProductName}",
                    updatedProduct.Id, userId, updatedProduct.Name);

                if (options.Value.LockUnauthenticated && !httpContext.User.IsAuthenticated())
                {
                    logger.LogWarning("Intento de actualizar producto sin autenticación para usuario {UserId}", userId);
                    return Results.Unauthorized();
                }

                logger.LogDebug("Validando producto actualizado...");
                var validationResult = await validator.ValidateAsync(updatedProduct, token);
                if (!validationResult.IsValid)
                {
                    logger.LogWarning("Validación fallida para actualización de producto: {ValidationErrors}",
                        string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                    return validationResult.ToResult();
                }

                logger.LogDebug("Actualizando producto en repositorio...");
                var result = await productRepo.UpdateAsync(userId, updatedProduct.ToEntity(), token);

                logger.LogInformation("Resultado de actualización de producto: {Result} para producto {ProductId}",
                    result, updatedProduct.Id);

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
            var logger = CreateLogger(loggerFactory);
            var userId = httpContext.GetUserId();
            
            using var scope = logger.BeginScope(new Dictionary<string, object>
            {
                ["UserId"] = userId,
                ["Method"] = nameof(GastosApiEndpoints.Products.Delete),
                ["ProductId"] = id
            });

            try
            {
                logger.LogDebug("Eliminando producto {ProductId} para usuario {UserId}", id, userId);

                if (options.Value.LockUnauthenticated && !httpContext.User.IsAuthenticated())
                {
                    logger.LogWarning("Intento de eliminar producto sin autenticación para usuario {UserId}", userId);
                    return Results.Unauthorized();
                }

                logger.LogDebug("Eliminando producto del repositorio...");
                var result = await productRepo.DeleteAsync(userId, id, token);

                logger.LogInformation("Resultado de eliminación de producto: {Result} para producto {ProductId}",
                    result, id);

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
