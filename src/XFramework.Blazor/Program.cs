using XFramework.Blazor.Components;
using XFramework.Application;
using XFramework.Application.DependencyInjection;
using XFramework.EntityFrameworkCore.DependencyInjection;
using XFramework.Infrastructure.DependencyInjection;
var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var configuration = builder.Configuration;

// Application
services.AddXFramework();

// Persistence
services.AddXFrameworkEntityFrameworkCore(configuration);

// Infrastructure
services.AddXFrameworkInfrastructure(configuration);
services.AddXFrameworkRabbitMQ(configuration);

services
    .AddIdentityCore<XFrameworkIdentityUser>(options =>
    {
        options.Password.RequiredLength = 8;

        options.Password.RequireDigit = true;

        options.Password.RequireUppercase = true;

        options.Password.RequireLowercase = true;

        options.Password.RequireNonAlphanumeric = false;

        options.User.RequireUniqueEmail = false;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<XFrameworkIdentityDbContext>()
    .AddSignInManager();



services
    .AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();
services.AddAuthorization();

services
    .AddRazorComponents()
    .AddInteractiveServerComponents();


services.AddXFrameworkApplication(
    typeof(XFrameworkApplicationAssembly).Assembly);

services.AddXFrameworkApplication()
    .AddXFrameworkEntityFrameworkCore(options =>
    {
        options.UseSqlServer(configuration.GetConnectionString("Default"));
    });
services.AddLocalization(options =>
    {
        options.ResourcesPath = "Localization/Resources";
    });
var supportedCultures = new[]
{
    "fa",
    "en"
};

var localizationOptions =
    new RequestLocalizationOptions()
        .SetDefaultCulture("fa")
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
services.AddScoped<ILocalizationService, BlazorLocalizationService>();
var app = builder.Build();

app.UseRequestLocalization(localizationOptions);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
