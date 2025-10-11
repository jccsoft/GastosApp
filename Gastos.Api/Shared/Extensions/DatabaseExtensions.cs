using Microsoft.EntityFrameworkCore.Migrations;

namespace Gastos.Api.Shared.Extensions;

/// <summary>
/// Provides extension methods for configuring database services and applying migrations.
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Adds database services to the application's service collection.
    /// Configures Entity Framework with PostgreSQL and applies snake_case naming convention.
    /// </summary>
    /// <param name="builder">The web application builder to configure.</param>
    /// <returns>The configured web application builder for method chaining.</returns>
    public static WebApplicationBuilder AddDatabaseServices(this WebApplicationBuilder builder)
    {
        string connectionString = builder.Environment.IsProduction() ?
            Environment.GetEnvironmentVariable("POSTGRESQLCONNSTR_Supabase")! :
            builder.Configuration.GetConnectionString("Supabase")!;


        builder.Services.AddDbContext<AppDbContext>(options =>
            options
                .UseNpgsql(
                    connectionString,
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Default))
                .UseSnakeCaseNamingConvention());

        return builder;
    }

    /// <summary>
    /// Registers repository services in the dependency injection container.
    /// Configures repository implementations with their respective lifetimes.
    /// </summary>
    /// <param name="builder">The web application builder to configure.</param>
    /// <returns>The configured web application builder for method chaining.</returns>
    public static WebApplicationBuilder AddRepoServices(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddScoped<ISizingRepository, SizingRepository>()
            .AddScoped<IProductRepository, ProductRepository>()
            .AddScoped<IStoreRepository, StoreRepository>()
            .AddScoped<IReceiptRepository, ReceiptRepository>()
            .AddScoped<IStatRepository, StatRepository>();

        return builder;
    }

    /// <summary>
    /// Applies pending database migrations to the application database.
    /// This method should be called during application startup to ensure the database schema is up to date.
    /// </summary>
    /// <param name="app">The web application instance.</param>
    /// <returns>A task that represents the asynchronous migration operation.</returns>
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        await using AppDbContext applicationDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            await applicationDbContext.Database.MigrateAsync();
            app.Logger.LogInformation("Application database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "An error occurred while applying database migrations.");
        }
    }
}
