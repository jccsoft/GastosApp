using Microsoft.EntityFrameworkCore.Migrations;

namespace Gastos.Api.Shared.Extensions;

public static class DatabaseExtensions
{
    public static WebApplicationBuilder AddDatabaseServices(this WebApplicationBuilder builder)
    {
        string connectionString = builder.Environment.IsProduction() ?
            Environment.GetEnvironmentVariable("POSTGRESQLCONNSTR_Supabase")! :
            builder.Configuration.GetConnectionString("Default")!;


        builder.Services.AddDbContext<AppDbContext>(options =>
            options
                .UseNpgsql(
                    connectionString,
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Default))
                .UseSnakeCaseNamingConvention());

        return builder;
    }

    public static WebApplicationBuilder AddRepoServices(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddScoped<ISizingRepository, SizingRepository>()
            .AddTransient<IProductRepository, ProductRepository>()
            .AddTransient<IStoreRepository, StoreRepository>()
            .AddTransient<IReceiptRepository, ReceiptRepository>()
            .AddTransient<IStatRepository, StatRepository>();

        return builder;
    }



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
