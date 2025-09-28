var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
    .AddPostgres("Default")
    .WithDataVolume(isReadOnly: false)
    .WithPgAdmin();

builder.AddProject<Projects.Gastos_Api>("gastos-api")
    .WithReference(postgres)
    .WaitFor(postgres, WaitBehavior.WaitOnResourceUnavailable);

await builder.Build().RunAsync();
