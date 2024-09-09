using System.Globalization;
using EmailWrapper;
using Logging;
using ErrorHandling;
using Microsoft.AspNetCore.Localization;
using OmmelSamvirke2024.Api.Middleware;
using DataAccess.Common;
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

// Setup Configuration
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddKeyVaultSecrets(builder.Environment.IsDevelopment() ? ExecutionEnvironment.Development : ExecutionEnvironment.Production)
    .AddEnvironmentVariables()
    .Build();

// Register supported languages
const string defaultCulture = "en";

var supportedCultures = new[]
{
    new CultureInfo(defaultCulture),
    new CultureInfo("da")
};

builder.Services.Configure<RequestLocalizationOptions>(options => {
    options.DefaultRequestCulture = new RequestCulture(defaultCulture);
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    
    // Use Accept-Language header
    options.RequestCultureProviders.Insert(0, new AcceptLanguageHeaderRequestCultureProvider());
});

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

app.UseRequestLocalization();

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
