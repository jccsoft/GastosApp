namespace Gastos.Api.Features.Receipts;

public static class ReceiptEndpoints
{
    public static IEndpointRouteBuilder MapReceiptEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet(GastosApiEndpoints.Receipts.GetAll, async (HttpContext httpContext,
            [AsParameters] ReceiptParameters parameters,
            [FromServices] IReceiptRepository receiptRepo,
            [FromServices] ILoggerFactory loggerFactory,
            CancellationToken token) =>
        {
            var userId = httpContext.GetUserId();

            try
            {
                var pagedReceipts = await receiptRepo.GetAllAsync(userId, parameters, token);

                var response = pagedReceipts.ToDto(receipt => receipt.ToDto());

                return TypedResults.Ok(response);
            }
            catch (Exception ex)
            {
                var logger = CreateLogger(loggerFactory);
                logger.LogError(ex, "Error en {Method} para el usuario {User} con parámetros ProductId: {ProductId}, ProductName: {ProductName}, FromDateUtc: {FromDateUtc}, ToDateUtc: {ToDateUtc}, Page: {Page}, PageSize: {PageSize}",
                    nameof(GastosApiEndpoints.Receipts.GetAll),
                    userId,
                    parameters.ProductId,
                    parameters.ProductName,
                    parameters.FromDateUtc,
                    parameters.ToDateUtc,
                    parameters.Page,
                    parameters.PageSize);
                return Results.Problem();
            }
        });

        app.MapGet(GastosApiEndpoints.Receipts.Get, async (HttpContext httpContext,
            Guid id,
            [FromServices] IReceiptRepository receiptRepo,
            [FromServices] ILoggerFactory loggerFactory,
            CancellationToken token) =>
        {
            var userId = httpContext.GetUserId();

            try
            {
                var receipt = await receiptRepo.GetByIdAsync(userId, id, token);

                return receipt is null ? Results.NotFound() : TypedResults.Ok(receipt.ToDto());
            }
            catch (Exception ex)
            {
                var logger = CreateLogger(loggerFactory);
                logger.LogError(ex, "Error en {Method} para el usuario {User} con id {Id}",
                    nameof(GastosApiEndpoints.Receipts.Get),
                    userId,
                    id);
                return Results.Problem();
            }
        });

        app.MapPost(GastosApiEndpoints.Receipts.Create, async (HttpContext httpContext,
            [FromBody] ReceiptDto newReceipt,
            ReceiptServerValidator validator,
            [FromServices] IReceiptRepository receiptRepo,
            [FromServices] IOptions<GastosApiOptions> options,
            [FromServices] ILoggerFactory loggerFactory,
            CancellationToken token) =>
        {
            var userId = httpContext.GetUserId();

            try
            {
                var validationResult = await validator.ValidateAsync(newReceipt, token);
                if (!validationResult.IsValid)
                    return validationResult.ToResult();

                var result = await receiptRepo.CreateAsync(userId, newReceipt.ToEntity(), token);

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
                logger.LogError(ex, "Error en {Method} para el usuario {User} con ReceiptId: {ReceiptId}, SourceId: {SourceId}, StoreId: {StoreId}, TransactionDateUtc: {TransactionDateUtc}",
                    nameof(GastosApiEndpoints.Receipts.Create),
                    userId,
                    newReceipt.Id,
                    newReceipt.SourceId,
                    newReceipt.StoreId,
                    newReceipt.TransactionDateUtc);
                return Results.Problem();
            }
        });

        app.MapPut(GastosApiEndpoints.Receipts.Update, async (HttpContext httpContext,
            [FromBody] ReceiptDto updatedReceipt,
            ReceiptServerValidator validator,
            [FromServices] IReceiptRepository receiptRepo,
            [FromServices] IOptions<GastosApiOptions> options,
            [FromServices] ILoggerFactory loggerFactory,
            CancellationToken token) =>
        {
            var userId = httpContext.GetUserId();

            try
            {
                var validationResult = await validator.ValidateAsync(updatedReceipt, token);
                if (!validationResult.IsValid)
                    return validationResult.ToResult();

                var result = await receiptRepo.UpdateAsync(userId, updatedReceipt.ToEntity(), token);

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
                logger.LogError(ex, "Error en {Method} para el usuario {User} con ReceiptId: {ReceiptId}, SourceId: {SourceId}, StoreId: {StoreId}, TransactionDateUtc: {TransactionDateUtc}",
                    nameof(GastosApiEndpoints.Receipts.Update),
                    userId,
                    updatedReceipt.Id,
                    updatedReceipt.SourceId,
                    updatedReceipt.StoreId,
                    updatedReceipt.TransactionDateUtc);
                return Results.Problem();
            }
        });

        app.MapDelete(GastosApiEndpoints.Receipts.Delete, async (HttpContext httpContext,
            Guid id,
            [FromServices] IReceiptRepository receiptRepo,
            [FromServices] IOptions<GastosApiOptions> options,
            [FromServices] ILoggerFactory loggerFactory,
            CancellationToken token) =>
        {
            var userId = httpContext.GetUserId();

            try
            {
                if (options.Value.LockUnauthenticated && !httpContext.User.IsAuthenticated())
                    return Results.Unauthorized();

                var result = await receiptRepo.DeleteAsync(userId, id, token);

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
                    nameof(GastosApiEndpoints.Receipts.Delete),
                    userId,
                    id);
                return Results.Problem();
            }
        });

        app.MapGet(GastosApiEndpoints.Receipts.ExistsBySourceId, async (HttpContext httpContext,
            Guid sourceId,
            Guid receiptIdToExlude,
            [FromServices] IReceiptRepository receiptRepo,
            [FromServices] ILoggerFactory loggerFactory,
            CancellationToken token) =>
        {
            var userId = httpContext.GetUserId();

            try
            {
                var exists = await receiptRepo.ExistsBySourceId(userId, sourceId, receiptIdToExlude, token);

                return TypedResults.Ok(exists);
            }
            catch (Exception ex)
            {
                var logger = CreateLogger(loggerFactory);
                logger.LogError(ex, "Error en {Method} para el usuario {User} con sourceId: {SourceId}, receiptIdToExlude: {ReceiptIdToExlude}",
                    nameof(GastosApiEndpoints.Receipts.ExistsBySourceId),
                    userId,
                    sourceId,
                    receiptIdToExlude);
                return Results.Problem();
            }
        });

        app.MapGet(GastosApiEndpoints.Receipts.ExistsByStoreIdAndDate, async (HttpContext httpContext,
            Guid storeId,
            DateTime transactionDateUtc,
            Guid? receiptIdToExclude,
            [FromServices] IReceiptRepository receiptRepo,
            [FromServices] ILoggerFactory loggerFactory,
            CancellationToken token) =>
        {
            var userId = httpContext.GetUserId();

            try
            {
                var exists = await receiptRepo.ExistsByStoreIdAndDate(userId, storeId, transactionDateUtc, receiptIdToExclude, token);

                return TypedResults.Ok(exists);
            }
            catch (Exception ex)
            {
                var logger = CreateLogger(loggerFactory);
                logger.LogError(ex, "Error en {Method} para el usuario {User} con storeId: {StoreId}, transactionDateUtc: {TransactionDateUtc}, receiptIdToExclude: {ReceiptIdToExclude}",
                    nameof(GastosApiEndpoints.Receipts.ExistsByStoreIdAndDate),
                    userId,
                    storeId,
                    transactionDateUtc,
                    receiptIdToExclude);
                return Results.Problem();
            }
        });

        app.MapGet(GastosApiEndpoints.Receipts.ExistsByStoreSourceNameAndDate, async (HttpContext httpContext,
            string storeSourceName,
            DateTime transactionDateUtc,
            [FromServices] IReceiptRepository receiptRepo,
            [FromServices] ILoggerFactory loggerFactory,
            CancellationToken token) =>
        {
            var userId = httpContext.GetUserId();

            try
            {
                var exists = await receiptRepo.ExistsByStoreSourceNameAndDate(
                    userId,
                    storeSourceName.UrlDecode(),
                    transactionDateUtc,
                    token);

                return TypedResults.Ok(exists);
            }
            catch (Exception ex)
            {
                var logger = CreateLogger(loggerFactory);
                logger.LogError(ex, "Error en {Method} para el usuario {User} con storeSourceName: {StoreSourceName}, transactionDateUtc: {TransactionDateUtc}",
                    nameof(GastosApiEndpoints.Receipts.ExistsByStoreSourceNameAndDate),
                    userId,
                    storeSourceName,
                    transactionDateUtc);
                return Results.Problem();
            }
        });

        app.MapGet(GastosApiEndpoints.Receipts.GetProductIdBySourceDescription, async (HttpContext httpContext,
            string description,
            [FromServices] IReceiptRepository receiptRepo,
            [FromServices] ILoggerFactory loggerFactory,
            CancellationToken token) =>
        {
            var userId = httpContext.GetUserId();

            try
            {
                var product = await receiptRepo.GetProductBySourceDescription(userId, description.UrlDecode(), token);

                return product is null ? Results.NotFound() : TypedResults.Ok(product);
            }
            catch (Exception ex)
            {
                var logger = CreateLogger(loggerFactory);
                logger.LogError(ex, "Error en {Method} para el usuario {User} con description {Description}",
                    nameof(GastosApiEndpoints.Receipts.GetProductIdBySourceDescription),
                    userId,
                    description);
                return Results.Problem();
            }
        });

        return app;
    }

    private static ILogger CreateLogger(ILoggerFactory loggerFactory)
    {
        return loggerFactory.CreateLogger(nameof(ReceiptEndpoints));
    }
}
