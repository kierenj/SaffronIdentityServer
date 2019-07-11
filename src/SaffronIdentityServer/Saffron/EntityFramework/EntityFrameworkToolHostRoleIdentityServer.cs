using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RedRiver.SaffronCore;
using SaffronIdentityServer.Saffron.EntityFramework;
using EfConfigKeys = SaffronIdentityServer.Saffron.EntityFramework.EfConfigKeys;

namespace RedRiver.Saffron.EntityFramework.Tooling
{
    [CommandLineShortName("efis")]
    [Description("Runs EntityFramework Core CLI utilities for database and context management")]
    public class EntityFrameworkToolHostRoleIdentityServer : SimpleRoleBase
    {
        private static readonly string MigrationSubNamespace = "Migrations";

        public override async Task RunAsync(CancellationToken stop)
        {
            // attempt to determine which context type (and so assembly) to use for the tool invocation
            var contextTypes = App.GetEfContextData().KnownContextTypes;
            Type contextType = null;

            if (contextTypes.Count == 0)
            {
                throw new SaffronConfigurationException("No known EF context types");
            }

            var specificType =
                Configuration.GetSection(EfConfigKeys.ConfigSectionName)[EfConfigKeys.ToolContextTypeKeyName];
            if (specificType == null)
            {
                if (contextTypes.Count > 1)
                {
                    var keyName = $"{EfConfigKeys.ConfigSectionName}:{EfConfigKeys.ToolContextTypeKeyName}";
                    throw new SaffronConfigurationException(
                        $"Multiple known EF context types; specify with configuration key '{keyName}'");
                }
                contextType = contextTypes.Single();
            }
            else
            {
                contextType = contextTypes.SingleOrDefault(t => t.Name == specificType);
                if (contextType == null)
                {
                    throw new SaffronConfigurationException($"Invalid context type name '{specificType}'");
                }
            }

            switch (Configuration["action"])
            {
                case "add-migration":
                    await AddMigrationAsync(contextType, stop);
                    break;

                case "update-database":
                    await UpdateDatabaseAsync(contextType, stop);
                    break;

                case "script":
                    await ScriptAsync(contextType, stop);
                    break;

                case "remove-migration":
                    await RemoveMigrationAsync(contextType, stop);
                    break;

                default:
                    throw new SaffronException(
                        "Please specify an EF action via command line/configuration key 'action'");
            }
        }

        private Task RemoveMigrationAsync(Type contextType, CancellationToken stop)
        {
            var projectDir = Configuration["projectDir"];
            if (string.IsNullOrWhiteSpace(projectDir))
            {
                throw new SaffronException(
                    "Please specify a migration project folder via command line/configuration key 'projectDir");
            }

            var forceConfig = Configuration["force"];
            if (forceConfig == null)
            {
                Console.WriteLine("Note: no 'force' command line/configuration key was specified.");
            }
            var force = bool.Parse(forceConfig ?? "false");

            if (projectDir.EndsWith("/") || projectDir.EndsWith("\\"))
            {
                projectDir = projectDir.Substring(0, projectDir.Length - 1);
            }

            // source: https://github.com/aspnet/EntityFrameworkCore/issues/9339
            using (var db = (SaffronIdentityDbContext)App.CompositionRoot.Resolve(contextType))
            {
                Console.WriteLine();
                Console.WriteLine("About to remove the last migration.  This will DELETE migration files.");
                Console.WriteLine();
                Console.WriteLine("The currently-configured database will be checked; if the migration has");
                Console.WriteLine("been applied there, an error will be thrown, unless you specify a");
                Console.WriteLine("'force' value of 'true', which case the migration would be reverted on");
                Console.WriteLine("the database first.");
                Console.WriteLine();
                Console.WriteLine("If the migration has been applied to multiple databases, you'll want to");
                Console.WriteLine("manually revert that on all databases before running this command, which");
                Console.WriteLine("will forever remove your ability to do so.");
                Console.WriteLine();
                Console.WriteLine("Context to be checked/updated:");
                Console.WriteLine(db.GetType().Name);
                Console.WriteLine(db.Database.GetDbConnection().ConnectionString);
                Console.WriteLine();
                Console.WriteLine("Please check this is the correct database / server / credential.");
                Console.WriteLine("Enter 'certainly' if you would like to proceed:");
                var ok = Console.ReadLine();
                if (ok != "certainly")
                {
                    throw new SaffronException("Update aborted");
                }

                var reporter = new OperationReporter(
                    new OperationReportHandler(
                        m => Console.WriteLine("  error: " + m),
                        m => Console.WriteLine("   warn: " + m),
                        m => Console.WriteLine("   info: " + m),
                        m => Console.WriteLine("verbose: " + m)));

                var designTimeServices = new ServiceCollection()
                    .AddSingleton(db.GetService<IHistoryRepository>())
                    .AddSingleton(db.GetService<IMigrationsIdGenerator>())
                    .AddSingleton(db.GetService<IMigrationsModelDiffer>())
                    .AddSingleton(db.GetService<IMigrationsAssembly>())
                    .AddSingleton(db.GetService<ICurrentDbContext>())
                    .AddSingleton(db.GetService<IDatabaseProvider>())
                    .AddSingleton(db.GetService<IDatabaseCreator>())
                    .AddSingleton(db.GetService<IRelationalTypeMappingSource>())
#pragma warning disable CS0618
                    // this is obsolete, yet the code won't run without it - disable CS0618
                    .AddSingleton(db.GetService<IRelationalTypeMapper>())
#pragma warning restore CS0618
                    .AddSingleton(db.GetService<ISingletonUpdateSqlGenerator>())
                    .AddSingleton(db.GetService<ISqlGenerationHelper>())
                    .AddSingleton(db.GetService<IRelationalConnection>())
                    .AddSingleton(db.Model)
                    .AddSingleton<ISnapshotModelProcessor, SnapshotModelProcessor>()
                    .AddSingleton<ICSharpHelper, CSharpHelper>()
                    .AddSingleton<ICSharpMigrationOperationGenerator, CSharpMigrationOperationGenerator>()
                    .AddSingleton<ICSharpSnapshotGenerator, CSharpSnapshotGenerator>()
                    .AddSingleton<IMigrator, Migrator>()
                    .AddSingleton<IMigrationsCodeGenerator, CSharpMigrationsGenerator>()
                    .AddSingleton<IRelationalCommandBuilderFactory, RelationalCommandBuilderFactory>()
                    .AddSingleton<ILoggerFactory, LoggerFactory>()
                    .AddSingleton<ILoggingOptions, LoggingOptions>()
                    .AddSingleton<IRawSqlCommandBuilder, RawSqlCommandBuilder>()
                    .AddSingleton<IParameterNameGeneratorFactory, ParameterNameGeneratorFactory>()
                    .AddSingleton<IMigrationCommandExecutor, MigrationCommandExecutor>()
                    .AddSingleton<IMigrationsSqlGenerator, MigrationsSqlGenerator>()
                    .AddSingleton<IMigrationsCodeGeneratorSelector, MigrationsCodeGeneratorSelector>()
                    .AddSingleton<IOperationReporter>(reporter)
                    .AddSingleton<MigrationsCodeGeneratorDependencies>()
                    .AddSingleton<CSharpMigrationOperationGeneratorDependencies>()
                    .AddSingleton<CSharpSnapshotGeneratorDependencies>()
                    .AddSingleton<CSharpMigrationsGeneratorDependencies>()
                    .AddSingleton<MigrationsSqlGeneratorDependencies>()
                    .AddSingleton<ParameterNameGeneratorDependencies>()
                    .AddSingleton<MigrationsScaffolderDependencies>()
                    .AddSingleton<MigrationsScaffolder>()
                    .AddSingleton<DiagnosticSource>(new DiagnosticListener(DbLoggerCategory.Name))
                    .AddSingleton(typeof(IDiagnosticsLogger<>), typeof(DiagnosticsLogger<>))
                    .BuildServiceProvider();

                var scaffolder = designTimeServices.GetRequiredService<MigrationsScaffolder>();

                var migFiles = scaffolder.RemoveMigration(projectDir, contextType.Namespace, force);

                Console.WriteLine("Removed metadata file: " + migFiles.MetadataFile);
                Console.WriteLine("Removed migration file: " + migFiles.MigrationFile);
                Console.WriteLine("Regenerated snapshot file: " + migFiles.SnapshotFile);
            }

            return Task.CompletedTask;
        }

        private async Task AddMigrationAsync(Type contextType, CancellationToken stop)
        {
            var migName = Configuration["name"];
            if (string.IsNullOrWhiteSpace(migName))
            {
                throw new SaffronException("Please specify a migration name via command line/configuration key 'name'");
            }

            var folder = Configuration["folder"];
            if (string.IsNullOrWhiteSpace(folder))
            {
                throw new SaffronException(
                    "Please specify a migration source code folder via command line/configuration key 'folder'");
            }

            if (!folder.EndsWith("/") && !folder.EndsWith("\\"))
            {
                folder += Path.DirectorySeparatorChar;
            }

            // source: https://github.com/aspnet/EntityFrameworkCore/issues/9339
            using (var db = (SaffronDbContext)App.CompositionRoot.Resolve(contextType))
            {
                var reporter = new OperationReporter(
                    new OperationReportHandler(
                        m => Console.WriteLine("  error: " + m),
                        m => Console.WriteLine("   warn: " + m),
                        m => Console.WriteLine("   info: " + m),
                        m => Console.WriteLine("verbose: " + m)));

                var designTimeServices = new ServiceCollection()
                    .AddSingleton(db.GetService<IHistoryRepository>())
                    .AddSingleton(db.GetService<IMigrationsIdGenerator>())
                    .AddSingleton(db.GetService<IMigrationsModelDiffer>())
                    .AddSingleton(db.GetService<IMigrationsAssembly>())
                    .AddSingleton(db.GetService<ICurrentDbContext>())
                    .AddSingleton(db.GetService<IDatabaseProvider>())
                    .AddSingleton(db.GetService<IDatabaseCreator>())
                    .AddSingleton(db.GetService<IRelationalTypeMappingSource>())
#pragma warning disable CS0618
                    // this is obsolete, yet the code won't run without it - disable CS0618
                    .AddSingleton(db.GetService<IRelationalTypeMapper>())
#pragma warning restore CS0618
                    .AddSingleton(db.GetService<ISingletonUpdateSqlGenerator>())
                    .AddSingleton(db.GetService<ISqlGenerationHelper>())
                    .AddSingleton(db.GetService<IRelationalConnection>())
                    .AddSingleton(db.Model)
                    .AddSingleton<ISnapshotModelProcessor, SnapshotModelProcessor>()
                    .AddSingleton<ICSharpHelper, CSharpHelper>()
                    .AddSingleton<ICSharpMigrationOperationGenerator, CSharpMigrationOperationGenerator>()
                    .AddSingleton<ICSharpSnapshotGenerator, CSharpSnapshotGenerator>()
                    .AddSingleton<IMigrator, Migrator>()
                    .AddSingleton<IMigrationsCodeGenerator, CSharpMigrationsGenerator>()
                    .AddSingleton<IRelationalCommandBuilderFactory, RelationalCommandBuilderFactory>()
                    .AddSingleton<ILoggerFactory, LoggerFactory>()
                    .AddSingleton<ILoggingOptions, LoggingOptions>()
                    .AddSingleton<IRawSqlCommandBuilder, RawSqlCommandBuilder>()
                    .AddSingleton<IParameterNameGeneratorFactory, ParameterNameGeneratorFactory>()
                    .AddSingleton<IMigrationCommandExecutor, MigrationCommandExecutor>()
                    .AddSingleton<IMigrationsSqlGenerator, MigrationsSqlGenerator>()
                    .AddSingleton<IMigrationsCodeGeneratorSelector, MigrationsCodeGeneratorSelector>()
                    .AddSingleton<IOperationReporter>(reporter)
                    .AddSingleton<MigrationsCodeGeneratorDependencies>()
                    .AddSingleton<CSharpMigrationOperationGeneratorDependencies>()
                    .AddSingleton<CSharpSnapshotGeneratorDependencies>()
                    .AddSingleton<CSharpMigrationsGeneratorDependencies>()
                    .AddSingleton<MigrationsSqlGeneratorDependencies>()
                    .AddSingleton<ParameterNameGeneratorDependencies>()
                    .AddSingleton<MigrationsScaffolderDependencies>()
                    .AddSingleton<MigrationsScaffolder>()
                    .AddSingleton<DiagnosticSource>(new DiagnosticListener(DbLoggerCategory.Name))
                    .AddSingleton(typeof(IDiagnosticsLogger<>), typeof(DiagnosticsLogger<>))
                    .BuildServiceProvider();

                var scaffolder = designTimeServices.GetRequiredService<MigrationsScaffolder>();

                var migration = scaffolder.ScaffoldMigration(
                    migName,
                    contextType.Namespace,
                    MigrationSubNamespace);

                await File.WriteAllTextAsync(
                    folder + migration.MigrationId + migration.FileExtension,
                    migration.MigrationCode, stop);
                await File.WriteAllTextAsync(
                    folder + migration.MigrationId + ".Designer" + migration.FileExtension,
                    migration.MetadataCode, stop);
                await File.WriteAllTextAsync(
                    folder + migration.SnapshotName + migration.FileExtension,
                    migration.SnapshotCode, stop);

                Console.WriteLine();
                Console.WriteLine("Migration generated.");
            }
        }

        private async Task ScriptAsync(Type contextType, CancellationToken stop)
        {
            var from = Configuration["from"];
            var to = Configuration["to"];
            var idempotent = bool.Parse(Configuration["idempotent"] ?? "false");
            var path = Configuration["path"];
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new SaffronException("Please specify output file name via command line/configuration key 'path'");
            }

            using (var db = (SaffronDbContext)App.CompositionRoot.Resolve(contextType))
            {
                var migrator = db.GetService<IMigrator>();
                var script = migrator.GenerateScript(from, to, idempotent);
                await File.WriteAllTextAsync(path, script, stop);
            }
        }

        private async Task UpdateDatabaseAsync(Type contextType, CancellationToken stop)
        {
            var target = Configuration["target"];

            using (var db = (SaffronIdentityDbContext)App.CompositionRoot.Resolve(contextType))
            {
                var migrator = db.GetService<IMigrator>();

                Console.WriteLine("About to perform migration.");

                if (target == null)
                {
                    var pending = await db.Database.GetPendingMigrationsAsync(stop);
                    Console.WriteLine("To latest migration - via:");
                    foreach (var item in pending)
                    {
                        Console.WriteLine("  " + item);
                    }
                }
                else
                {
                    Console.WriteLine($"To migration '{target}'");
                }

                Console.WriteLine("Context to be updated:");
                Console.WriteLine(db.GetType().Name);
                Console.WriteLine(db.Database.GetDbConnection().ConnectionString);
                Console.WriteLine();
                Console.WriteLine("Please check this is the correct database / server / credential.");
                Console.WriteLine("Enter 'certainly' if you would like to proceed:");
                var ok = Console.ReadLine();
                if (ok != "certainly")
                {
                    throw new SaffronException("Update aborted");
                }
                await migrator.MigrateAsync(target, stop);
            }
        }
    }
}