using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SaffronIdentityServer.Database;

namespace SaffronIdentityServer.Saffron.EntityFramework
{
    public abstract class SaffronIdentityDbContext<T> : IdentityDbContext where T : IdentityDbContext
    {
        protected SaffronIdentityDbContext(DbContextOptions<T> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddEntityConfigurationsFromAssembly(GetType().GetTypeInfo().Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
