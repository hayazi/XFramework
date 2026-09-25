using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using XFramework.Application.Contracts.Outbox;
using XFramework.Application.DependencyInjection;
using XFramework.Blazor.Components;
using XFramework.Blazor.Localization;
using XFramework.Application.Contracts.Localization;
using XFramework.EntityFrameworkCore.Identity;
using XFramework.EntityFrameworkCore.DependencyInjection;
using XFramework.Infrastructure.DependencyInjection;
using XFramework.Infrastructure.Messaging.RabbitMQ.DependencyInjection;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddXFramework();
builder.Services.AddXFrameworkEntityFrameworkCore(builder.Configuration);
builder.Services.AddXFrameworkInfrastructure(builder.Configuration);
builder.Services.AddXFrameworkRabbitMQ(builder.Configuration);
builder.Services.AddIdentityCore<XFrameworkIdentityUser>(o => { o.Password.RequiredLength = 8; o.Password.RequireDigit = true; o.Password.RequireUppercase = true; o.Password.RequireLowercase = true; o.Password.RequireNonAlphanumeric = false; o.User.RequireUniqueEmail = false; }).AddRoles<IdentityRole<Guid>>().AddEntityFrameworkStores<XFrameworkIdentityDbContext>().AddSignInManager();
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();
builder.Services.AddAuthorization();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddLocalization(o => o.ResourcesPath = "Localization/Resources");
builder.Services.AddScoped<ILocalizationService, BlazorLocalizationService>();
var cultures = new[] { "fa", "en" };
var localization = new RequestLocalizationOptions().SetDefaultCulture("fa").AddSupportedCultures(cultures).AddSupportedUICultures(cultures);
var app = builder.Build();

// Security headers middleware
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'; connect-src 'self'; frame-ancestors 'none';";
    await next();
});

app.UseRequestLocalization(localization);
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

// Outbox Admin API
var outboxGroup = app.MapGroup("/api/outbox").RequireAuthorization();
outboxGroup.MapGet("/stats", async (IOutboxAdminService admin, CancellationToken ct) =>
    await admin.GetStatsAsync(ct));
outboxGroup.MapGet("/pending", async (IOutboxAdminService admin, int page, int pageSize, CancellationToken ct) =>
    await admin.GetPendingAsync(page, pageSize, ct));
outboxGroup.MapGet("/failed", async (IOutboxAdminService admin, int page, int pageSize, CancellationToken ct) =>
    await admin.GetFailedAsync(page, pageSize, ct));
outboxGroup.MapGet("/processing", async (IOutboxAdminService admin, int page, int pageSize, CancellationToken ct) =>
    await admin.GetProcessingAsync(page, pageSize, ct));
outboxGroup.MapGet("/{id:guid}", async (IOutboxAdminService admin, Guid id, CancellationToken ct) =>
    await admin.GetByIdAsync(id, ct) is { } msg ? Results.Ok(msg) : Results.NotFound());
outboxGroup.MapPost("/{id:guid}/retry", async (IOutboxAdminService admin, Guid id, CancellationToken ct) =>
    await admin.RetryAsync(id, ct) ? Results.Ok() : Results.NotFound());
outboxGroup.MapPost("/{id:guid}/force-complete", async (IOutboxAdminService admin, Guid id, CancellationToken ct) =>
    await admin.ForceCompleteAsync(id, ct) ? Results.Ok() : Results.NotFound());

app.Run();
