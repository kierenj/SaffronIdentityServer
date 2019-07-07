using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Design;

namespace SaffronIdentityServer.Saffron.EntityFramework
{
    /// <summary>
    /// Context factory class for commandline tooling scenarios only.
    /// </summary>
    public abstract class SaffronDbContextSupportFactory<TContext> : IDesignTimeDbContextFactory<TContext>
        where TContext : SaffronIdentityDbContext
    {
        public TContext CreateDbContext(string[] args)
        {
            throw new NotSupportedException(
                "To perform EF command-line work, invoke the app and use the EntityFrameworkToolHostRole host role.");
        }
    }
}
