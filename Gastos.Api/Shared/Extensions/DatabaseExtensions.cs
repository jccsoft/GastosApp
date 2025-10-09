using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql;

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
    public static WebApplicationBuilder AddMyDatabaseServices(this WebApplicationBuilder builder)
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

    /// <summary>
    /// Registers repository services in the dependency injection container.
    /// Configures repository implementations with their respective lifetimes.
    /// </summary>
    /// <param name="builder">The web application builder to configure.</param>
    /// <returns>The configured web application builder for method chaining.</returns>
    public static WebApplicationBuilder AddMyRepoServices(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddScoped<ISizingRepository, SizingRepository>()
            .AddTransient<IProductRepository, ProductRepository>()
            .AddTransient<IStoreRepository, StoreRepository>()
            .AddTransient<IReceiptRepository, ReceiptRepository>()
            .AddTransient<IStatRepository, StatRepository>();

        return builder;
    }

    /// <summary>
    /// Applies pending database migrations to the application database safely.
    /// This method handles cases where tables already exist but migration history is missing.
    /// </summary>
    /// <param name="app">The web application instance.</param>
    /// <returns>A task that represents the asynchronous migration operation.</returns>
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        await using AppDbContext applicationDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            app.Logger.LogInformation("Starting database migration process...");

            // Check if database exists
            var canConnect = await applicationDbContext.Database.CanConnectAsync();
            if (!canConnect)
            {
                app.Logger.LogError("Cannot connect to database. Please check connection string.");
                return;
            }

            // Get pending migrations
            var pendingMigrations = await applicationDbContext.Database.GetPendingMigrationsAsync();
            var appliedMigrations = await applicationDbContext.Database.GetAppliedMigrationsAsync();

            app.Logger.LogInformation("Applied migrations: {AppliedCount}", appliedMigrations.Count());
            app.Logger.LogInformation("Pending migrations: {PendingCount}", pendingMigrations.Count());

            if (!pendingMigrations.Any())
            {
                app.Logger.LogInformation("Database is up to date. No migrations to apply.");
                return;
            }

            // Check if this is the initial migration and tables already exist
            bool isInitialMigration = !appliedMigrations.Any();
            if (isInitialMigration)
            {
                app.Logger.LogInformation("Detected initial migration scenario. Checking if tables already exist...");
                
                bool tablesExist = await CheckIfTablesExistAsync(applicationDbContext);
                if (tablesExist)
                {
                    app.Logger.LogWarning("Tables already exist but no migrations are recorded. This suggests the database was created outside of EF migrations.");
                    await HandleExistingTablesScenarioAsync(applicationDbContext, app.Logger);
                    return;
                }
            }

            // Apply migrations normally
            await applicationDbContext.Database.MigrateAsync();
            app.Logger.LogInformation("Database migrations applied successfully.");
        }
        catch (PostgresException pgEx) when (pgEx.SqlState == "42P07") // Table already exists
        {
            app.Logger.LogWarning("Table already exists error detected. Attempting to sync migration history...");
            await HandleTableExistsErrorAsync(applicationDbContext, app.Logger);
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "An error occurred while applying database migrations.");
            throw;
        }
    }

    /// <summary>
    /// Checks if the main application tables exist in the database.
    /// </summary>
    private static async Task<bool> CheckIfTablesExistAsync(AppDbContext context)
    {
        try
        {
            // Check if sizings table exists (it's created first in the migration)
            var connection = context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT EXISTS (
                    SELECT FROM information_schema.tables 
                    WHERE table_schema = @schema 
                    AND table_name = @tableName
                );";

            var schemaParam = command.CreateParameter();
            schemaParam.ParameterName = "@schema";
            schemaParam.Value = Schemas.Default;
            command.Parameters.Add(schemaParam);

            var tableParam = command.CreateParameter();
            tableParam.ParameterName = "@tableName";
            tableParam.Value = "sizings";
            command.Parameters.Add(tableParam);

            var result = await command.ExecuteScalarAsync();
            return (bool)result!;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Handles the scenario where tables exist but migration history is missing.
    /// Marks the initial migration as applied without executing it.
    /// </summary>
    private static async Task HandleExistingTablesScenarioAsync(AppDbContext context, ILogger logger)
    {
        try
        {
            logger.LogInformation("Marking initial migration as applied without executing...");

            // Get the migrations
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

            // This will create the migration history table and mark migrations as applied
            // without actually executing the migration SQL
            foreach (var migration in pendingMigrations)
            {
                logger.LogInformation("Marking migration {MigrationId} as applied", migration);
                
                // We need to manually add the migration to the history table
                await AddMigrationToHistoryAsync(context, migration);
            }

            logger.LogInformation("Successfully synchronized migration history with existing database schema.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to handle existing tables scenario");
            throw;
        }
    }

    /// <summary>
    /// Adds a migration to the history table without executing it.
    /// </summary>
    private static async Task AddMigrationToHistoryAsync(AppDbContext context, string migrationId)
    {
        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();

        // Ensure the migration history table exists
        using var createHistoryCommand = connection.CreateCommand();
        createHistoryCommand.CommandText = $@"
            CREATE TABLE IF NOT EXISTS ""{Schemas.Default}"".""{HistoryRepository.DefaultTableName}"" (
                ""{HistoryRepository.MigrationIdColumnName}"" character varying(150) NOT NULL,
                ""{HistoryRepository.ProductVersionColumnName}"" character varying(32) NOT NULL,
                CONSTRAINT ""pk_{HistoryRepository.DefaultTableName}"" PRIMARY KEY (""{HistoryRepository.MigrationIdColumnName}"")
            );";
        await createHistoryCommand.ExecuteNonQueryAsync();

        // Insert the migration record
        using var insertCommand = connection.CreateCommand();
        insertCommand.CommandText = $@"
            INSERT INTO ""{Schemas.Default}"".""{HistoryRepository.DefaultTableName}"" 
            (""{HistoryRepository.MigrationIdColumnName}"", ""{HistoryRepository.ProductVersionColumnName}"")
            VALUES (@migrationId, @productVersion)
            ON CONFLICT (""{HistoryRepository.MigrationIdColumnName}"") DO NOTHING;";

        var migrationParam = insertCommand.CreateParameter();
        migrationParam.ParameterName = "@migrationId";
        migrationParam.Value = migrationId;
        insertCommand.Parameters.Add(migrationParam);

        var versionParam = insertCommand.CreateParameter();
        versionParam.ParameterName = "@productVersion";
        versionParam.Value = "9.0.0"; // EF Core version
        insertCommand.Parameters.Add(versionParam);

        await insertCommand.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Handles PostgreSQL table already exists errors during migration.
    /// </summary>
    private static async Task HandleTableExistsErrorAsync(AppDbContext context, ILogger logger)
    {
        logger.LogWarning("Attempting to recover from table exists error...");
        
        // Try to mark the current migration as applied
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        
        if (pendingMigrations.Any())
        {
            var firstPending = pendingMigrations.First();
            logger.LogInformation("Marking migration {MigrationId} as applied due to table exists error", firstPending);
            
            await AddMigrationToHistoryAsync(context, firstPending);
            
            // Try to apply remaining migrations
            var remainingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (remainingMigrations.Any())
            {
                await context.Database.MigrateAsync();
            }
            
            logger.LogInformation("Successfully recovered from table exists error.");
        }
    }
}