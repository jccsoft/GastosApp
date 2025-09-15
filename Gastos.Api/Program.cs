using Gastos.Api;
using GastosApp.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddMyApiServices();

var app = builder.Build();

app.UseMyRequestLocalization();

app.MapDefaultEndpoints();

app.UseAuthentication();
//app.UseAuthorization();

app.MapOpenApi();

await app.ApplyMigrationsAsync();

app.UseHttpsRedirection();

app.MapAuth0Endpoints();

app.MapDocIntelApiForwarder();

app.MapMyApiEndpoints();
//app.MapApiEndpointsForwarder();

await app.RunAsync();


