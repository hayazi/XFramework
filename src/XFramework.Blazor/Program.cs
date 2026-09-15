using XFramework.Blazor.Components;
using XFramework.Application;
using XFramework.Application.DependencyInjection;
using XFramework.EntityFrameworkCore.DependencyInjection;
using XFramework.Infrastructure.DependencyInjection;
var builder = WebApplication.CreateBuilder(args);
builder.Services
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

builder.Services.AddXFrameworkInfrastructure(builder.Configuration);

builder.Services
    .AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();
builder.Services.AddAuthorization();

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();


builder.Services.AddXFrameworkApplication(
    typeof(XFrameworkApplicationAssembly).Assembly);

builder.Services..AddXFrameworkApplication()
    .AddXFrameworkEntityFrameworkCore(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
    });
builder.Services.AddLocalization(options =>
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
builder.Services.AddScoped<ILocalizationService, BlazorLocalizationService>();
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
