services.AddScoped<AuditSaveChangesInterceptor>();

services.AddDbContext<XFrameworkDbContext>(
    (serviceProvider, options) =>
    {
        options.UseSqlServer(connectionString);

        options.AddInterceptors(
            serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>());
    });