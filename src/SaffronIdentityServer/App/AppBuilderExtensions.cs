using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using RedRiver.SaffronCore;
using SaffronIdentityServer.Database.Models;

namespace SaffronIdentityServer.App
{
    public static class AppBuilderExtensions
    {
        public static IAppBuilder UseIdentityStores(this IAppBuilder builder)
        {
            builder.CompositionRoot.RegisterScoped<IUserStore<User>, UserStore<User>>();
            builder.CompositionRoot.RegisterScoped<IRoleStore<Role>, RoleStore<Role>>();

            return builder;
        }
    }
}
