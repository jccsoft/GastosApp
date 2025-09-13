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
            try
            {
                var pagedReceipts = await receiptRepo.GetAllAsync(httpContext.GetUserId(), parameters, token);

                var response = pagedReceipts.ToDto(receipt => receipt.ToDto());

                return TypedResults.Ok(response);
            }
            catch (Exception ex)
            {
                var logger = CreateLogger(loggerFactory);
                logger.LogError(ex, "Error en {Method} para el usuario {User} con parámetros ProductId: {ProductId}, ProductName: {ProductName}, FromDateUtc: {FromDateUtc}, ToDateUtc: {ToDateUtc}, Page: {Page}, PageSize: {PageSize}",
                    nameof(GastosApiEndpoints.Receipts.GetAll),
                    httpContext.GetUserId(),
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
            try
            {
                var receipt = await receiptRepo.GetByIdAsync(httpContext.GetUserId(), id, token);

                return receipt is null ? Results.NotFound() : TypedResults.Ok(receipt.ToDto());
            }
            catch (Exception ex)
            {
                var logger = CreateLogger(loggerFactory);
                logger.LogError(ex, "Error en {Method} para el usuario {User} con id {Id}",
                    nameof(GastosApiEndpoints.Receipts.Get),
                    httpContext.GetUserId(),
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
            try
            {
                var validationResult = await validator.ValidateAsync(newReceipt, token);
                if (!validationResult.IsValid)
                    return validationResult.ToResult();

                var result = await receiptRepo.CreateAsync(httpContext.GetUserId(), newReceipt.ToEntity(), token);

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
                    httpContext.GetUserId(),
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
            try
            {
                var validationResult = await validator.ValidateAsync(updatedReceipt, token);
                if (!validationResult.IsValid)
                    return validationResult.ToResult();

                var result = await receiptRepo.UpdateAsync(httpContext.GetUserId(), updatedReceipt.ToEntity(), token);

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
                    httpContext.GetUserId(),
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
            try
            {
                if (options.Value.LockUnauthenticated && !httpContext.User.IsAuthenticated())
                    return Results.Unauthorized();

                var result = await receiptRepo.DeleteAsync(httpContext.GetUserId(), id, token);

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
                    httpContext.GetUserId(),
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
            try
            {
                var exists = await receiptRepo.ExistsBySourceId(httpContext.GetUserId(), sourceId, receiptIdToExlude, token);

                return TypedResults.Ok(exists);
            }
            catch (Exception ex)
            {
                var logger = CreateLogger(loggerFactory);
                logger.LogError(ex, "Error en {Method} para el usuario {User} con sourceId: {SourceId}, receiptIdToExlude: {ReceiptIdToExlude}",
                    nameof(GastosApiEndpoints.Receipts.ExistsBySourceId),
                    httpContext.GetUserId(),
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
            try
            {
                var exists = await receiptRepo.ExistsByStoreIdAndDate(httpContext.GetUserId(), storeId, transactionDateUtc, receiptIdToExclude, token);

                return TypedResults.Ok(exists);
            }
            catch (Exception ex)
            {
                var logger = CreateLogger(loggerFactory);
                logger.LogError(ex, "Error en {Method} para el usuario {User} con storeId: {StoreId}, transactionDateUtc: {TransactionDateUtc}, receiptIdToExclude: {ReceiptIdToExclude}",
                    nameof(GastosApiEndpoints.Receipts.ExistsByStoreIdAndDate),
                    httpContext.GetUserId(),
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
            try
            {
                var exists = await receiptRepo.ExistsByStoreSourceNameAndDate(
                    httpContext.GetUserId(),
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
                    httpContext.GetUserId(),
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
            try
            {
                var product = await receiptRepo.GetProductBySourceDescription(httpContext.GetUserId(), description.UrlDecode(), token);

                return product is null ? Results.NotFound() : TypedResults.Ok(product);
            }
            catch (Exception ex)
            {
                var logger = CreateLogger(loggerFactory);
                logger.LogError(ex, "Error en {Method} para el usuario {User} con description {Description}",
                    nameof(GastosApiEndpoints.Receipts.GetProductIdBySourceDescription),
                    httpContext.GetUserId(),
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
