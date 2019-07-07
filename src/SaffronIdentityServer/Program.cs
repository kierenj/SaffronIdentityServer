using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RedRiver.Saffron.AspNetCore;
using RedRiver.SaffronCore;
using SaffronIdentityServer.App;
using SaffronIdentityServer.Database;
using SaffronIdentityServer.Database.Models;
using SaffronIdentityServer.Saffron.EntityFramework;

namespace SaffronIdentityServer
{
    public class Program : IAppDefinition
    {
        public static async Task Main(string[] args)
        {
            // Discovery endpoint: http://localhost:5000/.well-known/openid-configuration

            var process = new SaffronProcessBuilder()
                .ConfigureHost(h => h
                    .UseDefinition<Program>()
                    .UseDefaultLogging()
                    .UseSaffronConfig()
                )
                .SupportAspNetCore()
                .SupportEfIsCommandLineTool()
                .UseRoles<FromCommandLine>()
                .Build();

            await process.RunAsync();
        }

        public void DefineApp(IAppBuilder app)
        {
            app.UseName("IdentityServer", "IDSR")
                .UseAutofac()
                .UseSwagger()
                .UseEntityFrameworkIdentityServer<CoreContext>((options, connStr) => options.UseSqlServer(connStr))
                .UseWebApiModule<IdentityServerModule>()
                .UseIdentityStores();
        }
    }
}
