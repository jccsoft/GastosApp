var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
    .AddPostgres("Default")
    .WithDataVolume(isReadOnly: false)
    .WithPgAdmin();

var api = builder.AddProject<Projects.Gastos_Api>("gastos-api")
    .WithReference(postgres)
    .WaitFor(postgres, WaitBehavior.WaitOnResourceUnavailable);

// Añadir el proyecto Blazor WebAssembly Standalone
builder.AddProject<Projects.Gastos_Pwa>("gastos-pwa")
    .WithReference(api)
    .WaitFor(api);

await builder.Build().RunAsync();
