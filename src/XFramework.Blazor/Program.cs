using XFramework.Blazor.Components;
using XFramework.Application;
using XFramework.Application.DependencyInjection;
using XFramework.EntityFrameworkCore.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

// builder.Services.AddXFrameworkApplication();
builder.Services.AddXFrameworkApplication(
    typeof(XFrameworkApplicationAssembly).Assembly);

builder.Services..AddXFrameworkApplication()
    .AddXFrameworkEntityFrameworkCore(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
    });

var app = builder.Build();

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
