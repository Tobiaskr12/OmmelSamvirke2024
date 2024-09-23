IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<ProjectResource> apiService = builder.AddProject<Projects.OmmelSamvirke2024_Api>("apiservice");

builder.AddProject<Projects.OmmelSamvirke2024_Web>("webfrontend")
    .WithReference(apiService);

builder.Build().Run();