using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RedRiver.Saffron.EntityFramework;
using RedRiver.SaffronCore;

namespace SaffronIdentityServer.Saffron.EntityFramework
{
    public static class AppBuilderExtensions
    {
        public static IAppBuilder UseEntityFramework<TContext>(
            this IAppBuilder builder,
            Action<DbContextOptionsBuilder, string> providerAction
        ) where TContext : SaffronIdentityDbContext<TContext>
        {
            builder.UseDefaultConfiguration(new Dictionary<string, string>
            {
                { "Logging:logLevel:Microsoft.EntityFrameworkCore.Database.Connection", "Warning" },
                { "Logging:logLevel:Microsoft.EntityFrameworkCore.Database.Command", "Information" },
                { "Logging:logLevel:Microsoft.EntityFrameworkCore.Database.Transaction", "Warning" },
                { "Logging:logLevel:Microsoft.EntityFrameworkCore.ChangeTracking", "Warning" },
                { "Logging:logLevel:Microsoft.EntityFrameworkCore.Infrastructure", "Warning" }
            });

            var contextName = typeof(TContext).Name;

            // record the context as a known type: needed for EF tooling integration
            builder.GetEfContextData().KnownContextTypes.Add(typeof(TContext));

            builder.AfterConfiguration((configuredBuilder, config) =>
            {
                // grab the configuration once it's available..
                var connectionString = GetConnectionStringOrThrow<TContext>(config);

                configuredBuilder.UseMicrosoftServices(services => services
                    .AddDbContext<TContext>(options =>
                    {
                        // use caller's config method to set up EF provider, any other custom settings
                        providerAction(options, connectionString);
                    }, ServiceLifetime.Transient)
                    .AddHealthChecks()
                    .AddDbContextCheck<TContext>(contextName + " ready", null, new[] { "ready", "status" }, ReadyCheck)
                    .AddDbContextCheck<TContext>(contextName + " connects", null, new[] { "live", "status" })
                );
            });

            return builder;
        }

        private static async Task<bool> ReadyCheck<TContext>(TContext ctx, CancellationToken cancel) where TContext : SaffronIdentityDbContext<TContext>
        {
            if (!await NoMigsCheck(ctx, cancel)) return false;
            return await WarmedUpCheck(ctx, cancel);
        }

        private static async Task<bool> NoMigsCheck<TContext>(TContext ctx, CancellationToken cancel) where TContext : SaffronIdentityDbContext<TContext>
        {
            var pendingMigs = await ctx.Database.GetPendingMigrationsAsync(cancel);
            var noMigs = !pendingMigs.Any();
            return noMigs;
        }

        private static Task<bool> WarmedUpCheck<TContext>(TContext ctx, CancellationToken cancel) where TContext : SaffronIdentityDbContext<TContext>
        {
            // ref: https://github.com/aspnet/EntityFrameworkCore/issues/15568
            _ = ctx.Model;
            return Task.FromResult(true);
        }

        private static string GetConnectionStringOrThrow<TContext>(IConfiguration config) where TContext : DbContext
        {
            var contextName = typeof(TContext).Name;
            var configSect = config.GetSection(EfConfigKeys.ConfigSectionName).GetSection(contextName);
            var connectionString = configSect[EfConfigKeys.ConnectionStringKeyName];
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                var msg = $"Missing connection string: looked for configuration key " +
                          $"{EfConfigKeys.ConfigSectionName}:{contextName}:{EfConfigKeys.ConnectionStringKeyName}";
                throw new SaffronConfigurationException(msg);
            }
            return connectionString;
        }
    }
}
