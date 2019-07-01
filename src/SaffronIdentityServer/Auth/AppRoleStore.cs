using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SaffronIdentityServer.Database;
using SaffronIdentityServer.Database.Models;

namespace SaffronIdentityServer.Auth
{
    /*
    public class AppRoleStore : IRoleStore<Role>
    {
        private readonly CoreContext _ctx;

        public AppRoleStore(CoreContext ctx)
        {
            _ctx = ctx;
        }

        public void Dispose()
        {
        }

        public async Task<IdentityResult> CreateAsync(Role role, CancellationToken cancellationToken)
        {
            _ctx.Add(role);
            await _ctx.SaveChangesAsync();

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(Role role, CancellationToken cancellationToken)
        {
            var dbRole = await FindByIdAsync(role.Id, cancellationToken);
            _ctx.RolePermissions.RemoveRange(role.Permissions);

            dbRole.Name = role.Name;
            dbRole.Description = role.Description;
            dbRole.Type = role.Type;
            dbRole.Permissions = role.Permissions;

            await _ctx.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(Role role, CancellationToken cancellationToken)
        {
            var dbRole = await FindByIdAsync(role.Id, cancellationToken);
            _ctx.Remove(dbRole);

            await _ctx.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<string> GetRoleIdAsync(Role role, CancellationToken cancellationToken)
        {
            var dbRole = await FindByIdAsync(role.Id, cancellationToken);
            return dbRole.Id.ToString();
        }

        public async Task<string> GetRoleNameAsync(Role role, CancellationToken cancellationToken)
        {
            var dbRole = await FindByIdAsync(role.Id, cancellationToken);
            return dbRole.Name;
        }

        public async Task SetRoleNameAsync(Role role, string roleName, CancellationToken cancellationToken)
        {
            var dbRole = await FindByIdAsync(role.Id, cancellationToken);
            dbRole.Name = roleName;
        }

        public async Task<string> GetNormalizedRoleNameAsync(Role role, CancellationToken cancellationToken)
        {
            var dbRole = await FindByIdAsync(role.Id, cancellationToken);
            return dbRole.Name.ToUpper();
        }

        public Task SetNormalizedRoleNameAsync(Role role, string normalizedName, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task<Role> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            return await FindByIdAsync(roleId, cancellationToken);
        }

        public async Task<Role> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            return await _ctx.Roles.SingleOrDefaultAsync(x => x.Name.ToUpper() == normalizedRoleName);
        }
    }
    */
}
