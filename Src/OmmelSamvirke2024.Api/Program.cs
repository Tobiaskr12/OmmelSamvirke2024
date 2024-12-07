using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Localization;
using OmmelSamvirke.DataAccess;
using OmmelSamvirke.DomainModules;
using OmmelSamvirke.Infrastructure;
using OmmelSamvirke.ServiceModules;
using OmmelSamvirke.SupportModules.Logging;
using OmmelSamvirke.SupportModules.SecretsManager;
using OmmelSamvirke2024.Api.Middleware;
using OmmelSamvirke2024.ServiceDefaults;
using Swashbuckle.AspNetCore.Filters;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddControllers(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

builder.Services.AddSwaggerGen(options =>
{
    options.ExampleFilters();
});
builder.Services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly());

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
builder.Services.AddSingleton(appLogger)
       .InitializeDataAccessModule(builder.Configuration).Result
       .InitializeInfrastructureModule()
       .InitializeDomainModule()
       .InitializeServicesModule();

WebApplication app = builder.Build();

app.UseRequestLocalization();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    // Redirect from "/" to swagger dashboard
    app.Use(async (context, next) =>
    {
        if (context.Request.Path == "/")
        {
            context.Response.Redirect("/swagger/index.html");
            return;
        }
        await next();
    });
}

app.UseMiddleware<ValidationExceptionHandlingMiddleware>();
app.UseMiddleware<ResultExceptionHandlingMiddleware>();
app.UseRouting();

// app.UseAuthentication();
// app.UseAuthorization();
app.MapControllers();

app.Run();
