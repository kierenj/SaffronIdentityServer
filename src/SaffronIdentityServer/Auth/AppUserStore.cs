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
    public class AppUserStore : IUserStore<User>
    {
        private readonly CoreContext _ctx;

        public AppUserStore(CoreContext ctx)
        {
            _ctx = ctx;
        }

        public void Dispose()
        {
        }

        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id.ToString());
        }

        public Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;

            return Task.CompletedTask;
        }

        public Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName.ToUpper();

            return Task.CompletedTask;
        }

        public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
        {
            _ctx.Users.Add(user);
            await _ctx.SaveChangesAsync();

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            var dbUser = await FindByIdAsync(user.Id.ToString(), cancellationToken);
            dbUser.AdditionalDataJson = user.AdditionalDataJson;

            await _ctx.SaveChangesAsync();

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            var dbUser = await FindByIdAsync(user.Id.ToString(), cancellationToken);
            dbUser.DeletedUtc = DateTime.UtcNow;

            await _ctx.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return await _ctx.Users.FirstOrDefaultAsync(x => !x.DeletedUtc.HasValue && x.Id == userId);
        }

        public async Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return await _ctx.Users.FirstOrDefaultAsync(x => !x.DeletedUtc.HasValue && x.NormalizedUserName == normalizedUserName);
        }
    }
}
