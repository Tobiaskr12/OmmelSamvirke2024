var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.OmmelSamvirke2024_Api>("apiservice");

builder.AddProject<Projects.OmmelSamvirke2024_Web>("webfrontend")
    .WithReference(apiService);

builder.Build().Run();