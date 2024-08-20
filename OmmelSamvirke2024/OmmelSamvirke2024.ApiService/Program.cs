using Logging;
using OmmelSamvirke2024.Persistence;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddControllers();

builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddEnvironmentVariables()
    .Build();

// Register custom logger
ILogger appLogger = AppLoggerFactory.CreateLogger(builder.Configuration);
builder.Logging.ClearProviders();
builder.Logging.AddProvider(new AppLoggerProvider(appLogger));

// Register services
await builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddSingleton(appLogger);

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

// app.UseAuthentication();
// app.UseAuthorization();
app.MapControllers();

app.Run();
