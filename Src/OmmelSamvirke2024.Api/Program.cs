using EmailWrapper;
using Logging;
using ErrorHandling;
using OmmelSamvirke2024.Api.Middleware;
using OmmelSamvirke2024.Persistence;
using SecretsManager;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddControllers(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddKeyVaultSecrets(builder.Environment.IsDevelopment() ? ExecutionEnvironment.Development : ExecutionEnvironment.Production)
    .AddEnvironmentVariables()
    .Build();

// Register custom logger
ILogger appLogger = AppLoggerFactory.CreateLogger(builder.Configuration);
builder.Logging.ClearProviders();
builder.Logging.AddProvider(new AppLoggerProvider(appLogger));

// Register services
await builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddSingleton(appLogger);
builder.Services.InitializeErrorHandlingModule();
builder.Services.InitializeEmailWrapperModule();

WebApplication app = builder.Build();

// Configure services after all services have been registered. For example: Register error messages
app.ConfigureEmailWrapperModule();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ResultExceptionHandlingMiddleware>();
app.UseRouting();

// app.UseAuthentication();
// app.UseAuthorization();
app.MapControllers();

app.Run();
