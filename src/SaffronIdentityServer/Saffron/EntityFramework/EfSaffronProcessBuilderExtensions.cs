using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RedRiver.Saffron.EntityFramework.Tooling;
using RedRiver.SaffronCore;

namespace SaffronIdentityServer.Saffron.EntityFramework
{
    public static class EfSaffronProcessBuilderExtensions
    {
        public static SaffronProcessBuilder SupportEfIsCommandLineTool(this SaffronProcessBuilder builder, Action<HostRoleConfigurationBuilder> configureRole = null)
        {
            var hrcb = new HostRoleConfigurationBuilder(typeof(EntityFrameworkToolHostRoleIdentityServer));
            configureRole?.Invoke(hrcb);
            var roleConfig = hrcb.Build();
            builder.ConfigureRole(roleConfig);
            return builder;
        }
    }
}
