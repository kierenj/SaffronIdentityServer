using Microsoft.EntityFrameworkCore;
using RedRiver.Saffron.EntityFramework;
using RedRiver.Saffron.EntityFramework.AuthStore.Entities;
using SaffronIdentityServer.Database.Models;
using Role = SaffronIdentityServer.Database.Models.Role;

namespace SaffronIdentityServer.Database
{
    public class CoreContext : SaffronDbContext
    {
        public CoreContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

    }
}
