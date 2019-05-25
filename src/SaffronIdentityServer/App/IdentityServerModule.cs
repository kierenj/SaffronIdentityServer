using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using RedRiver.Saffron.AspNetCore;
using SaffronIdentityServer.Auth;
using SaffronIdentityServer.Database.Models;
using RedRiver.Saffron.Autofac;

namespace SaffronIdentityServer.App
{
    public class IdentityServerModule : ISaffronWebApiModule
    {
        public void ConfigureBuilder(IApplicationBuilder builder)
        {
            builder.UseIdentityServer();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // TODO: Figure out how to move into settings and grab
            var connectionString = @"Data Source=localhost;Initial Catalog=SaffronIdentityServerDemo;Integrated Security=True";
            var migrationAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;

            // TODO: AutoFac registration error => 
            services.AddIdentity<User, Role>()
                .AddUserStore<AppUserStore>()
                .AddRoleStore<AppRoleStore>()
                .AddUserManager<UserManager<User>>()
                .AddRoleManager<RoleManager<Role>>();

            services.AddIdentityServer()
                .AddOperationalStore(opts =>
                    opts.ConfigureDbContext = builder =>
                        builder.UseSqlServer(connectionString, sqlOpts =>
                            sqlOpts.MigrationsAssembly(migrationAssembly)))
                .AddConfigurationStore(opts =>
                    opts.ConfigureDbContext = builder =>
                        builder.UseSqlServer(connectionString, sqlOpts =>
                            sqlOpts.MigrationsAssembly(migrationAssembly)))
                .AddAspNetIdentity<User>()
                .AddDeveloperSigningCredential();
        }

        public void ConfigureMvcOptions(MvcOptions options)
        {
           
        }

        public BuilderConfigurationPriority ConfigureBuilderPriority => BuilderConfigurationPriority.AfterMvc;
        public ServiceConfigurationPriority ConfigureServicesPriority => ServiceConfigurationPriority.AfterMvc;

        public MvcOptionsConfigurationPriority ConfigureMvcOptionsPriority =>
            MvcOptionsConfigurationPriority.BeforeValidationFilters;
    }
}
