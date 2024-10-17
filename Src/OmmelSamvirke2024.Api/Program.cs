using System.Globalization;
using System.Reflection;
using Emails.Services;
using Logging;
using Microsoft.AspNetCore.Localization;
using OmmelSamvirke2024.Api.Middleware;
using DataAccess.Common;
using Emails.Domain;
using OmmelSamvirke2024.ServiceDefaults;
using SecretsManager;
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
await builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddSingleton(appLogger)
   .InitializeEmailServicesModule()
   .InitializeEmailDomainModule();

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
