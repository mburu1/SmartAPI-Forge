using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartAPIForge.Application.Auth.Interfaces;
using SmartAPIForge.Domain.Common;
using SmartAPIForge.Infrastructure.Identity;
using SmartAPIForge.Infrastructure.Options;
using SmartAPIForge.Infrastructure.Persistence;

namespace SmartAPIForge.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
            ?? new DatabaseOptions();

        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddDbContext<AppDbContext>(options => options.UseDatabaseProvider(configuration, databaseOptions.Provider));

        services.AddIdentityCore<ApplicationUser>(identityOptions =>
            {
                identityOptions.Password.RequiredLength = 8;
                identityOptions.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>();
        // No AddTokenProvider() call: this API doesn't expose password-reset/
        // email-confirmation/2FA endpoints, so no token provider is needed yet.

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }

    private static DbContextOptionsBuilder UseDatabaseProvider(
        this DbContextOptionsBuilder options, IConfiguration configuration, DatabaseProvider provider)
    {
        var connectionString = configuration.GetConnectionString(provider.ToString())
            ?? throw new InvalidOperationException(
                $"Missing ConnectionStrings:{provider} for the configured Database:Provider ({provider}).");

        return provider switch
        {
            DatabaseProvider.Postgres => options.UseNpgsql(connectionString),
            DatabaseProvider.SqlServer => options.UseSqlServer(connectionString),
            DatabaseProvider.MySql => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)),
            _ => throw new NotSupportedException($"Database provider '{provider}' is not supported.")
        };
    }
}
