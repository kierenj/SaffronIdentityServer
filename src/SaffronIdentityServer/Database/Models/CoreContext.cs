using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RedRiver.Saffron.EntityFramework;
using RedRiver.Saffron.EntityFramework.AuthStore.Entities;
using SaffronIdentityServer.Database.Models;
using SaffronIdentityServer.Saffron;
using SaffronIdentityServer.Saffron.EntityFramework;
using Role = SaffronIdentityServer.Database.Models.Role;

namespace SaffronIdentityServer.Database.Models
{
    public class CoreContext : SaffronIdentityDbContext
    {
        public CoreContext(DbContextOptions<CoreContext> options) : base(options)
        {
        }

        // public DbSet<User> Users { get; set; }
        // public DbSet<Role> Roles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

    }
}
