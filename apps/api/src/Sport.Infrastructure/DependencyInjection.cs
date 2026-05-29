using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sport.Application.Abstractions;
using Sport.Infrastructure.Interceptors;
using Sport.Infrastructure.Persistence;

namespace Sport.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSportInfrastructure(this IServiceCollection services)
    {
        services.AddOptions<SlowQueryOptions>().BindConfiguration("SlowQuery");
        services.AddSingleton<SlowQueryInterceptor>();

        services.AddDbContext<SportDbContext>((sp, options) =>
        {
            var cfg = sp.GetRequiredService<IConfiguration>();
            var env = sp.GetRequiredService<IHostEnvironment>();

            options
                .UseNpgsql(
                    cfg.GetConnectionString("Postgres"),
                    npgsql => npgsql.MigrationsAssembly(typeof(SportDbContext).Assembly.FullName))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<SlowQueryInterceptor>())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution)
                .ConfigureWarnings(w => w
                    .Throw(RelationalEventId.MultipleCollectionIncludeWarning)
                    .Throw(CoreEventId.RowLimitingOperationWithoutOrderByWarning)
                    .Throw(RelationalEventId.PendingModelChangesWarning));

            if (env.IsDevelopment())
            {
                options
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors()
                    .LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name },
                           LogLevel.Information);
            }
        });

        services.AddHealthChecks()
            .AddDbContextCheck<SportDbContext>(name: "postgres", tags: new[] { "ready" });

        services.AddScoped<SportMigrationRunner>();
        services.AddScoped<ICompetitionRepository, CompetitionRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}
