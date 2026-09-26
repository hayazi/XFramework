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
using XFramework.Application.Contracts.Inventory;
using XFramework.Application.Contracts.Dimensions;
using XFramework.Application.Contracts.Numbering;
using XFramework.Application.Contracts.Tax;
using XFramework.Application.Contracts.Dtos;
using XFramework.Domain.Inventory;
using XFramework.Domain.Dimensions;
using XFramework.Domain.Numbering;
using XFramework.Domain.Tax;
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

// Radzen services
builder.Services.AddScoped<Radzen.DialogService>();
builder.Services.AddScoped<Radzen.NotificationService>();
builder.Services.AddScoped<Radzen.TooltipService>();
builder.Services.AddScoped<Radzen.ContextMenuService>();

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

        // Inventory API
        var inventoryGroup = app.MapGroup("/api/inventory").RequireAuthorization();
        // Items
        inventoryGroup.MapGet("/items", async (IItemAppService svc, int skipCount = 0, int maxResultCount = 50, CancellationToken ct = default) =>
            await svc.GetListAsync(new PagedAndSortedRequestDto { SkipCount = skipCount, MaxResultCount = maxResultCount }, ct));
        inventoryGroup.MapGet("/items/{id:guid}", async (IItemAppService svc, Guid id, CancellationToken ct) =>
            await svc.GetAsync(id, ct) is { } dto ? Results.Ok(dto) : Results.NotFound());
        inventoryGroup.MapPost("/items", async (IItemAppService svc, ItemCreateDto input, CancellationToken ct) =>
            await svc.CreateAsync(input, ct));
        inventoryGroup.MapPut("/items/{id:guid}", async (IItemAppService svc, Guid id, ItemUpdateDto input, CancellationToken ct) =>
            await svc.UpdateAsync(id, input, ct));
        inventoryGroup.MapDelete("/items/{id:guid}", async (IItemAppService svc, Guid id, CancellationToken ct) =>
        {
            await svc.DeleteAsync(id, ct);
            return Results.NoContent();
        });
        inventoryGroup.MapGet("/items/code/{code}", async (IItemAppService svc, string code, CancellationToken ct) =>
            await svc.GetByCodeAsync(code, ct) is { } dto ? Results.Ok(dto) : Results.NotFound());
        inventoryGroup.MapGet("/items/type/{type:int}", async (IItemAppService svc, ItemType type, CancellationToken ct) =>
            await svc.GetByTypeAsync(type, ct));
        inventoryGroup.MapGet("/items/low-stock", async (IItemAppService svc, CancellationToken ct) =>
            await svc.GetLowStockAsync(ct));
        inventoryGroup.MapPost("/items/{id:guid}/activate", async (IItemAppService svc, Guid id, CancellationToken ct) =>
            await svc.ActivateAsync(id, ct));
        inventoryGroup.MapPost("/items/{id:guid}/deactivate", async (IItemAppService svc, Guid id, CancellationToken ct) =>
            await svc.DeactivateAsync(id, ct));
        inventoryGroup.MapPost("/items/{id:guid}/discontinue", async (IItemAppService svc, Guid id, CancellationToken ct) =>
            await svc.DiscontinueAsync(id, ct));
        inventoryGroup.MapPost("/items/{id:guid}/block", async (IItemAppService svc, Guid id, CancellationToken ct) =>
            await svc.BlockAsync(id, ct));
        inventoryGroup.MapPost("/items/{id:guid}/unblock", async (IItemAppService svc, Guid id, CancellationToken ct) =>
            await svc.UnblockAsync(id, ct));
        // Warehouses
        inventoryGroup.MapGet("/warehouses", async (IWarehouseAppService svc, int skipCount = 0, int maxResultCount = 50, CancellationToken ct = default) =>
            await svc.GetListAsync(new PagedAndSortedRequestDto { SkipCount = skipCount, MaxResultCount = maxResultCount }, ct));
        inventoryGroup.MapGet("/warehouses/{id:guid}", async (IWarehouseAppService svc, Guid id, CancellationToken ct) =>
            await svc.GetAsync(id, ct) is { } dto ? Results.Ok(dto) : Results.NotFound());
        inventoryGroup.MapPost("/warehouses", async (IWarehouseAppService svc, WarehouseCreateDto input, CancellationToken ct) =>
            await svc.CreateAsync(input, ct));
        inventoryGroup.MapPut("/warehouses/{id:guid}", async (IWarehouseAppService svc, Guid id, WarehouseUpdateDto input, CancellationToken ct) =>
            await svc.UpdateAsync(id, input, ct));
        inventoryGroup.MapDelete("/warehouses/{id:guid}", async (IWarehouseAppService svc, Guid id, CancellationToken ct) =>
        {
            await svc.DeleteAsync(id, ct);
            return Results.NoContent();
        });
        inventoryGroup.MapGet("/warehouses/code/{code}", async (IWarehouseAppService svc, string code, CancellationToken ct) =>
            await svc.GetByCodeAsync(code, ct) is { } dto ? Results.Ok(dto) : Results.NotFound());
        inventoryGroup.MapGet("/warehouses/active", async (IWarehouseAppService svc, CancellationToken ct) =>
            await svc.GetActiveAsync(ct));
        inventoryGroup.MapPost("/warehouses/{id:guid}/activate", async (IWarehouseAppService svc, Guid id, CancellationToken ct) =>
            await svc.ActivateAsync(id, ct));
        inventoryGroup.MapPost("/warehouses/{id:guid}/deactivate", async (IWarehouseAppService svc, Guid id, CancellationToken ct) =>
            await svc.DeactivateAsync(id, ct));
        // Kardex
        inventoryGroup.MapGet("/cardex", async (ICardexAppService svc, int skipCount = 0, int maxResultCount = 50, CancellationToken ct = default) =>
            await svc.GetListAsync(new PagedAndSortedRequestDto { SkipCount = skipCount, MaxResultCount = maxResultCount }, ct));
        inventoryGroup.MapGet("/cardex/{id:guid}", async (ICardexAppService svc, Guid id, CancellationToken ct) =>
            await svc.GetAsync(id, ct) is { } dto ? Results.Ok(dto) : Results.NotFound());
        inventoryGroup.MapGet("/cardex/item/{itemId:guid}", async (ICardexAppService svc, Guid itemId, int skipCount = 0, int maxResultCount = 50, CancellationToken ct = default) =>
            await svc.GetByItemAsync(itemId, new PagedAndSortedRequestDto { SkipCount = skipCount, MaxResultCount = maxResultCount }, ct));
        inventoryGroup.MapGet("/cardex/warehouse/{warehouseId:guid}", async (ICardexAppService svc, Guid warehouseId, int skipCount = 0, int maxResultCount = 50, CancellationToken ct = default) =>
            await svc.GetByWarehouseAsync(warehouseId, new PagedAndSortedRequestDto { SkipCount = skipCount, MaxResultCount = maxResultCount }, ct));
        inventoryGroup.MapGet("/cardex/date-range", async (ICardexAppService svc, DateTime from, DateTime to, int skipCount = 0, int maxResultCount = 50, CancellationToken ct = default) =>
            await svc.GetByDateRangeAsync(from, to, new PagedAndSortedRequestDto { SkipCount = skipCount, MaxResultCount = maxResultCount }, ct));
        inventoryGroup.MapPost("/cardex/receipt", async (ICardexAppService svc, CardexEntryCreateDto input, CancellationToken ct) =>
            await svc.CreateReceiptAsync(input, ct));
        inventoryGroup.MapPost("/cardex/issue", async (ICardexAppService svc, CardexEntryCreateDto input, CancellationToken ct) =>
            await svc.CreateIssueAsync(input, ct));
        inventoryGroup.MapPost("/cardex/adjustment", async (ICardexAppService svc, CardexEntryCreateDto input, CancellationToken ct) =>
            await svc.CreateAdjustmentAsync(input, ct));
        inventoryGroup.MapGet("/cardex/stock/{itemId:guid}/{warehouseId:guid}", async (ICardexAppService svc, Guid itemId, Guid warehouseId, CancellationToken ct) =>
            await svc.GetCurrentStockAsync(itemId, warehouseId, ct));

        // Dimensions API
        var dimensionsGroup = app.MapGroup("/api/dimensions").RequireAuthorization();
        // Cost Centers
        dimensionsGroup.MapGet("/cost-centers", async (ICostCenterAppService svc, int skipCount = 0, int maxResultCount = 50, CancellationToken ct = default) =>
            await svc.GetListAsync(new PagedAndSortedRequestDto { SkipCount = skipCount, MaxResultCount = maxResultCount }, ct));
        dimensionsGroup.MapGet("/cost-centers/{id:guid}", async (ICostCenterAppService svc, Guid id, CancellationToken ct) =>
            await svc.GetAsync(id, ct) is { } dto ? Results.Ok(dto) : Results.NotFound());
        dimensionsGroup.MapPost("/cost-centers", async (ICostCenterAppService svc, CostCenterCreateDto input, CancellationToken ct) =>
            await svc.CreateAsync(input, ct));
        dimensionsGroup.MapPut("/cost-centers/{id:guid}", async (ICostCenterAppService svc, Guid id, CostCenterUpdateDto input, CancellationToken ct) =>
            await svc.UpdateAsync(id, input, ct));
        dimensionsGroup.MapDelete("/cost-centers/{id:guid}", async (ICostCenterAppService svc, Guid id, CancellationToken ct) =>
        {
            await svc.DeleteAsync(id, ct);
            return Results.NoContent();
        });
        dimensionsGroup.MapGet("/cost-centers/code/{code}", async (ICostCenterAppService svc, string code, CancellationToken ct) =>
            await svc.GetByCodeAsync(code, ct) is { } dto ? Results.Ok(dto) : Results.NotFound());
        dimensionsGroup.MapGet("/cost-centers/hierarchy", async (ICostCenterAppService svc, CancellationToken ct) =>
            await svc.GetHierarchyAsync(ct));
        dimensionsGroup.MapGet("/cost-centers/active", async (ICostCenterAppService svc, CancellationToken ct) =>
            await svc.GetActiveAsync(ct));
        dimensionsGroup.MapPost("/cost-centers/{id:guid}/activate", async (ICostCenterAppService svc, Guid id, CancellationToken ct) =>
            await svc.ActivateAsync(id, ct));
        dimensionsGroup.MapPost("/cost-centers/{id:guid}/deactivate", async (ICostCenterAppService svc, Guid id, CancellationToken ct) =>
            await svc.DeactivateAsync(id, ct));
        dimensionsGroup.MapPost("/cost-centers/{id:guid}/close", async (ICostCenterAppService svc, Guid id, CancellationToken ct) =>
            await svc.CloseAsync(id, ct));
        // Projects
        dimensionsGroup.MapGet("/projects", async (IProjectAppService svc, int skipCount = 0, int maxResultCount = 50, CancellationToken ct = default) =>
            await svc.GetListAsync(new PagedAndSortedRequestDto { SkipCount = skipCount, MaxResultCount = maxResultCount }, ct));
        dimensionsGroup.MapGet("/projects/{id:guid}", async (IProjectAppService svc, Guid id, CancellationToken ct) =>
            await svc.GetAsync(id, ct) is { } dto ? Results.Ok(dto) : Results.NotFound());
        dimensionsGroup.MapPost("/projects", async (IProjectAppService svc, ProjectCreateDto input, CancellationToken ct) =>
            await svc.CreateAsync(input, ct));
        dimensionsGroup.MapPut("/projects/{id:guid}", async (IProjectAppService svc, Guid id, ProjectUpdateDto input, CancellationToken ct) =>
            await svc.UpdateAsync(id, input, ct));
        dimensionsGroup.MapDelete("/projects/{id:guid}", async (IProjectAppService svc, Guid id, CancellationToken ct) =>
        {
            await svc.DeleteAsync(id, ct);
            return Results.NoContent();
        });
        dimensionsGroup.MapGet("/projects/code/{code}", async (IProjectAppService svc, string code, CancellationToken ct) =>
            await svc.GetByCodeAsync(code, ct) is { } dto ? Results.Ok(dto) : Results.NotFound());
        dimensionsGroup.MapGet("/projects/active", async (IProjectAppService svc, CancellationToken ct) =>
            await svc.GetActiveAsync(ct));
        dimensionsGroup.MapGet("/projects/manager/{managerId:guid}", async (IProjectAppService svc, Guid managerId, CancellationToken ct) =>
            await svc.GetByManagerAsync(managerId, ct));
        dimensionsGroup.MapGet("/projects/customer/{customerId:guid}", async (IProjectAppService svc, Guid customerId, CancellationToken ct) =>
            await svc.GetByCustomerAsync(customerId, ct));
        dimensionsGroup.MapPost("/projects/{id:guid}/activate", async (IProjectAppService svc, Guid id, CancellationToken ct) =>
            await svc.ActivateAsync(id, ct));
        dimensionsGroup.MapPost("/projects/{id:guid}/deactivate", async (IProjectAppService svc, Guid id, CancellationToken ct) =>
            await svc.DeactivateAsync(id, ct));
        dimensionsGroup.MapPost("/projects/{id:guid}/close", async (IProjectAppService svc, Guid id, DateTime actualEndDate, CancellationToken ct) =>
            await svc.CloseAsync(id, actualEndDate, ct));
        // Custom Dimensions
        dimensionsGroup.MapGet("/custom-dimensions", async (ICustomDimensionAppService svc, int skipCount = 0, int maxResultCount = 50, CancellationToken ct = default) =>
            await svc.GetListAsync(new PagedAndSortedRequestDto { SkipCount = skipCount, MaxResultCount = maxResultCount }, ct));
        dimensionsGroup.MapGet("/custom-dimensions/{id:guid}", async (ICustomDimensionAppService svc, Guid id, CancellationToken ct) =>
            await svc.GetAsync(id, ct) is { } dto ? Results.Ok(dto) : Results.NotFound());
        dimensionsGroup.MapPost("/custom-dimensions", async (ICustomDimensionAppService svc, CustomDimensionCreateDto input, CancellationToken ct) =>
            await svc.CreateAsync(input, ct));
        dimensionsGroup.MapPut("/custom-dimensions/{id:guid}", async (ICustomDimensionAppService svc, Guid id, CustomDimensionUpdateDto input, CancellationToken ct) =>
            await svc.UpdateAsync(id, input, ct));
        dimensionsGroup.MapDelete("/custom-dimensions/{id:guid}", async (ICustomDimensionAppService svc, Guid id, CancellationToken ct) =>
        {
            await svc.DeleteAsync(id, ct);
            return Results.NoContent();
        });
        dimensionsGroup.MapGet("/custom-dimensions/code/{code}", async (ICustomDimensionAppService svc, string code, CancellationToken ct) =>
            await svc.GetByCodeAsync(code, ct) is { } dto ? Results.Ok(dto) : Results.NotFound());
        dimensionsGroup.MapGet("/custom-dimensions/key/{dimensionKey}", async (ICustomDimensionAppService svc, string dimensionKey, CancellationToken ct) =>
            await svc.GetByDimensionKeyAsync(dimensionKey, ct) is { } dto ? Results.Ok(dto) : Results.NotFound());
        dimensionsGroup.MapPost("/custom-dimensions/{id:guid}/activate", async (ICustomDimensionAppService svc, Guid id, CancellationToken ct) =>
            await svc.ActivateAsync(id, ct));
        dimensionsGroup.MapPost("/custom-dimensions/{id:guid}/deactivate", async (ICustomDimensionAppService svc, Guid id, CancellationToken ct) =>
            await svc.DeactivateAsync(id, ct));

        // Numbering API
        var numberingGroup = app.MapGroup("/api/numbering").RequireAuthorization();
        numberingGroup.MapGet("/sequences", async (INumberSequenceAppService svc, int skipCount = 0, int maxResultCount = 50, CancellationToken ct = default) =>
            await svc.GetListAsync(new PagedAndSortedRequestDto { SkipCount = skipCount, MaxResultCount = maxResultCount }, ct));
        numberingGroup.MapGet("/sequences/{id:guid}", async (INumberSequenceAppService svc, Guid id, CancellationToken ct) =>
            await svc.GetAsync(id, ct) is { } dto ? Results.Ok(dto) : Results.NotFound());
        numberingGroup.MapPost("/sequences", async (INumberSequenceAppService svc, NumberSequenceCreateDto input, CancellationToken ct) =>
            await svc.CreateAsync(input, ct));
        numberingGroup.MapPut("/sequences/{id:guid}", async (INumberSequenceAppService svc, Guid id, NumberSequenceUpdateDto input, CancellationToken ct) =>
            await svc.UpdateAsync(id, input, ct));
        numberingGroup.MapDelete("/sequences/{id:guid}", async (INumberSequenceAppService svc, Guid id, CancellationToken ct) =>
        {
            await svc.DeleteAsync(id, ct);
            return Results.NoContent();
        });
        numberingGroup.MapGet("/sequences/code/{code}", async (INumberSequenceAppService svc, string code, CancellationToken ct) =>
            await svc.GetByCodeAsync(code, ct) is { } dto ? Results.Ok(dto) : Results.NotFound());
        numberingGroup.MapGet("/sequences/scope/{code}/{scopeIdentifier?}", async (INumberSequenceAppService svc, string code, string? scopeIdentifier, CancellationToken ct) =>
            await svc.GetByScopeAsync(code, scopeIdentifier, ct) is { } dto ? Results.Ok(dto) : Results.NotFound());
        numberingGroup.MapPost("/sequences/{id:guid}/next", async (INumberSequenceAppService svc, Guid id, CancellationToken ct) =>
            await svc.GetNextNumberAsync(id, ct));
        numberingGroup.MapGet("/sequences/{id:guid}/peek", async (INumberSequenceAppService svc, Guid id, CancellationToken ct) =>
            await svc.PeekNextNumberAsync(id, ct));
        numberingGroup.MapPost("/sequences/{id:guid}/reset", async (INumberSequenceAppService svc, Guid id, CancellationToken ct) =>
            await svc.ResetAsync(id, ct));
        numberingGroup.MapPost("/sequences/{id:guid}/activate", async (INumberSequenceAppService svc, Guid id, CancellationToken ct) =>
            await svc.ActivateAsync(id, ct));
        numberingGroup.MapPost("/sequences/{id:guid}/deactivate", async (INumberSequenceAppService svc, Guid id, CancellationToken ct) =>
            await svc.DeactivateAsync(id, ct));

        // Tax API
        var taxGroup = app.MapGroup("/api/tax").RequireAuthorization();
        taxGroup.MapGet("/codes", async (ITaxCodeAppService svc, int skipCount = 0, int maxResultCount = 50, CancellationToken ct = default) =>
            await svc.GetListAsync(new PagedAndSortedRequestDto { SkipCount = skipCount, MaxResultCount = maxResultCount }, ct));
        taxGroup.MapGet("/codes/{id:guid}", async (ITaxCodeAppService svc, Guid id, CancellationToken ct) =>
            await svc.GetAsync(id, ct) is { } dto ? Results.Ok(dto) : Results.NotFound());
        taxGroup.MapPost("/codes", async (ITaxCodeAppService svc, TaxCodeCreateDto input, CancellationToken ct) =>
            await svc.CreateAsync(input, ct));
        taxGroup.MapPut("/codes/{id:guid}", async (ITaxCodeAppService svc, Guid id, TaxCodeUpdateDto input, CancellationToken ct) =>
            await svc.UpdateAsync(id, input, ct));
        taxGroup.MapDelete("/codes/{id:guid}", async (ITaxCodeAppService svc, Guid id, CancellationToken ct) =>
        {
            await svc.DeleteAsync(id, ct);
            return Results.NoContent();
        });
        taxGroup.MapGet("/codes/code/{code}", async (ITaxCodeAppService svc, string code, CancellationToken ct) =>
            await svc.GetByCodeAsync(code, ct) is { } dto ? Results.Ok(dto) : Results.NotFound());
        taxGroup.MapGet("/codes/default/{taxType:int}", async (ITaxCodeAppService svc, TaxType taxType, CancellationToken ct) =>
            await svc.GetDefaultAsync(taxType, ct) is { } dto ? Results.Ok(dto) : Results.NotFound());
        taxGroup.MapGet("/codes/active", async (ITaxCodeAppService svc, CancellationToken ct) =>
            await svc.GetActiveAsync(ct));
        taxGroup.MapGet("/codes/type/{taxType:int}", async (ITaxCodeAppService svc, TaxType taxType, CancellationToken ct) =>
            await svc.GetByTypeAsync(taxType, ct));
        taxGroup.MapPost("/codes/{id:guid}/calculate", async (ITaxCodeAppService svc, Guid id, MoneyDto baseAmount, decimal? quantity, CancellationToken ct) =>
            await svc.CalculateTaxAsync(id, baseAmount, quantity, ct));
        taxGroup.MapPost("/codes/{id:guid}/activate", async (ITaxCodeAppService svc, Guid id, CancellationToken ct) =>
            await svc.ActivateAsync(id, ct));
        taxGroup.MapPost("/codes/{id:guid}/deactivate", async (ITaxCodeAppService svc, Guid id, CancellationToken ct) =>
            await svc.DeactivateAsync(id, ct));

        app.Run();
