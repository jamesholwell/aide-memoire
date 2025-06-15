using AideMemoire;
using AideMemoire.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Runtime.CompilerServices;
using AideMemoire.Infrastructure.Data;
using AideMemoire.Infrastructure.Repositories;
using FluentMigrator.Runner;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
[assembly: InternalsVisibleTo("AideMemoire.Tests")]

// create host and configure services
var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder();
ConfigureDatabaseServices(builder);
ConfigureOtherServices(builder);
builder.Services.AddSingleton<Application>();
host = builder.Build();

// initlialize database
host.Services.GetRequiredService<DatabaseMigrationService>().InitializeDatabase();

// run the application
await host.Services.GetRequiredService<Application>().RunAsync(args);

public partial class Program {
    private static IHost? host;

    internal static IHost Host { get => host!; }

    internal static void ConfigureDatabaseServices(HostApplicationBuilder builder) {
        // ef core
        var connectionString = "Data Source=aide-memoire.db";
        builder.Services.AddDbContext<AideMemoireDbContext>(options =>
            options.UseSqlite(connectionString));

        // repositories
        builder.Services.AddScoped<IRealmRepository, RealmRepository>();
        builder.Services.AddScoped<IMemoryRepository, MemoryRepository>();

        // fluentmigrator
        builder.Services
            .AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(Program).Assembly).For.Migrations())
            .AddLogging(lb => lb
                .SetMinimumLevel(LogLevel.Warning)
                .AddFilter("FluentMigrator", LogLevel.Warning)
                .AddFilter("DatabaseMigrationService", LogLevel.Information));

        // migration
        builder.Services.AddSingleton<DatabaseMigrationService>();
    }

    internal static void ConfigureOtherServices(HostApplicationBuilder builder) {
        builder.Services
            .AddHttpClient()
            .ConfigureHttpClientDefaults(builder => {
                builder.ConfigureHttpClient(client => {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("aide-memoire/0.1");
                });
            });
    }
}