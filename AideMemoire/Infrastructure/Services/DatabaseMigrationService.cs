using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AideMemoire.Infrastructure.Services;

public class DatabaseMigrationService {
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseMigrationService> _logger;

    public DatabaseMigrationService(IServiceProvider serviceProvider, ILogger<DatabaseMigrationService> logger) {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void InitializeDatabase() {
        try {
            using var scope = _serviceProvider.CreateScope();
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            var isBlankDatabase = !runner.HasMigrationsToApplyDown(0);
            var hasMigrations = runner.HasMigrationsToApplyUp();

            if (hasMigrations)
                _logger.LogInformation(isBlankDatabase
                ? "Initializing new database..."
                : "Database migrations found, applying...");

            runner.MigrateUp();

        }
        catch (Exception ex) {
            _logger.LogError(ex, "Failed to migrate database");
            throw;
        }
    }
}
