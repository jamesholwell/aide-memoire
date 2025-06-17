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
using Microsoft.SemanticKernel;

[assembly: InternalsVisibleTo("AideMemoire.Tests")]

// create bootstrapper
var bootstrapBuilder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder();
ConfigureHttpClient(bootstrapBuilder);
ConfigureLogging(bootstrapBuilder, args.Contains("--verbose") || args.Contains("-v"));
bootstrapBuilder.Services.AddSingleton<OnnxModelDownloadService>();
var bootstrapper = bootstrapBuilder.Build();
await bootstrapper.Services.GetRequiredService<OnnxModelDownloadService>().DownloadModelsAsync();

// create host and configure services
var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder();
ConfigureHttpClient(builder);
ConfigureLogging(builder, args.Contains("--verbose") || args.Contains("-v"));
ConfigureAIServices(builder);
ConfigureDatabaseServices(builder);
ConfigureMediator(builder);
builder.Services.AddSingleton<Application>();
host = builder.Build();

// initlialize database
host.Services.GetRequiredService<DatabaseMigrationService>().InitializeDatabase();

// run the application
await host.Services.GetRequiredService<Application>().RunAsync(args);

public partial class Program {
    private static IHost? host;

    internal static IHost Host { get => host!; }

    internal static void ConfigureAIServices(HostApplicationBuilder builder) {
        #pragma warning disable SKEXP0070
        builder.Services.AddBertOnnxEmbeddingGenerator(
            onnxModelPath: OnnxModelDownloadService.GetPath("minilm-l12-v2.onnx"),
            vocabPath: OnnxModelDownloadService.GetPath("minilm-l12-v2_vocab.txt")
        );
    }

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
                .ScanIn(typeof(Program).Assembly).For.Migrations());

        // migration
        builder.Services.AddSingleton<DatabaseMigrationService>();

        // vector stores
        builder.Services.AddSqliteVectorStore(_ => "Data Source=aide-memoire.db");
    }

    internal static void ConfigureLogging(HostApplicationBuilder builder, bool isVerbose) {
        string[] internalLoggers = [
            nameof(AideMemoire.Handlers.MemoryUpdatedEmbeddingHandler),
            typeof(OnnxModelDownloadService).FullName!,
            typeof(DatabaseMigrationService).FullName!,
        ];

        builder.Services.AddLogging(options => options
            .ClearProviders()
            .AddSimpleConsole(consoleOptions => {
                consoleOptions.IncludeScopes = false;
                consoleOptions.SingleLine = true;
            })
            .AddFilter((category, level) =>
                level >= (internalLoggers.Contains(category) || isVerbose ? LogLevel.Information : LogLevel.Warning)
            ));
    }

    internal static void ConfigureMediator(HostApplicationBuilder builder) {
        builder.Services
            .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
    }

    internal static void ConfigureHttpClient(HostApplicationBuilder builder) {
        builder.Services
            .AddHttpClient()
            .ConfigureHttpClientDefaults(builder => {
                builder.ConfigureHttpClient(client => {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("aide-memoire/0.1");
                });
            });
    }
}