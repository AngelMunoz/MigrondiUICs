using Autofac;

using Serilog;
using Serilog.Extensions.Logging;

using System.Data;
using Microsoft.Data.Sqlite;

using Migrondi.Core;
using Migrondi.Core.Database;
using Migrondi.Core.Serialization;

using MigrondiUI.Components;
using MigrondiUI.Services;
using MigrondiUI.Types;
using Migrondi.Core.FileSystem;
using SqlKata.Execution;
using SqlKata.Compilers;





#if DEBUG
using Avalonia.Diagnostics;
#endif

Log.Logger = new LoggerConfiguration()
  .MinimumLevel
#if DEBUG
  .Debug()
#else
  .Information()
#endif
  .Enrich.FromLogContext()
  .WriteTo
  .Console()
  .CreateLogger();

var msIlogger =
  new SerilogLoggerFactory(Log.Logger)
    .CreateLogger("MigrondiUI");

var (config, cwd, migrations) = LocalMigrationsHandler.GetMigrondiParams();

var builder = new ContainerBuilder();

builder.RegisterInstance(msIlogger);

builder.RegisterInstance<IRouter>(new Router(new Home()));

builder.RegisterInstance(new MigrondiSerializer())
  .As<IMiConfigurationSerializer>()
  .As<IMiMigrationSerializer>();

builder.Register(services =>
{
  var logger = services.Resolve<Microsoft.Extensions.Logging.ILogger>();
  var configSerializer = services.Resolve<IMiConfigurationSerializer>();
  var migrationSerializer = services.Resolve<IMiMigrationSerializer>();

  var db = new MiDatabaseHandler(logger, config);


  (db as IMiDatabaseHandler).SetupDatabase(); ;

  var fs = new MiFileSystem(logger, configSerializer, migrationSerializer, cwd, migrations);

  return new Migrondi.Core.Migrondi(config, db, fs, logger);
})
.Named<IMigrondi>("local:Migrondi");

builder.Register(services =>
  Migrondi.Core.Migrondi.MigrondiFactory(services.Resolve<Microsoft.Extensions.Logging.ILogger>()));

builder.RegisterInstance<Func<IDbConnection>>(() => new SqliteConnection(config.connection));

builder.Register(services =>
{
  var connection = services.Resolve<Func<IDbConnection>>()();
  return new QueryFactory(connection, new SqliteCompiler());
});

AppBuilder.Configure<Application>()
  .UsePlatformDetect()
  .UseFluentTheme(ThemeVariant.Default)
  .StartWithClassicDesktopLifetime(desktop =>
  {
    desktop.MainWindow = new Window { Title = "Migrondi UI" };
#if DEBUG
    desktop.MainWindow.AttachDevTools();
#endif
    TopLevel topLevel = TopLevel.GetTopLevel(desktop.MainWindow)!;

    builder.RegisterInstance(topLevel!.StorageProvider);

    builder.Register<IProjectService>(services =>
      new ProjectService(
        services.Resolve<Microsoft.Extensions.Logging.ILogger>(),
        services.Resolve<IMiConfigurationSerializer>(),
        services.Resolve<IMiMigrationSerializer>(),
        services.Resolve<QueryFactory>()
      )
    );

    builder.Register<IWorkspaceService>(services =>
      new WorkspaceService(
        services.Resolve<QueryFactory>(),
        services.Resolve<IProjectService>()
      )
    );

    var container = builder.Build();

    var localMigrondi = container.ResolveNamed<IMigrondi>("local:Migrondi");

    LocalMigrationsHandler.Initialize(localMigrondi);


    desktop.MainWindow.Content = ShellModule.GetView(container);
  }, args);
