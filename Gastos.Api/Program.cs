using Gastos.Api;
using Gastos.Api.Middleware;
using GastosApp.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddMyApiServices();

var app = builder.Build();

app.UseMyRequestLocalization();

app.MapDefaultEndpoints();

// Agregar middleware de logging detallado para debug
if (app.Environment.IsProduction() || app.Environment.IsStaging())
{
    app.UseRequestLogging();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApi();

await app.ApplyMigrationsAsync();

app.UseHttpsRedirection();

app.MapDocIntelApiForwarder();

app.MapMyApiEndpoints();

await app.RunAsync();


