using Autofac;
using MigrondiUI.Components;
using MigrondiUI.Services;
using MigrondiUI.Types;

AppBuilder.Configure<Application>()
  .UsePlatformDetect()
  .UseFluentTheme(ThemeVariant.Default)
  .StartWithClassicDesktopLifetime(desktop =>
  {
    desktop.MainWindow = new Window { Title = "Migrondi UI" };
    TopLevel topLevel = TopLevel.GetTopLevel(desktop.MainWindow)!;

    var builder = new ContainerBuilder();

    builder.RegisterInstance(topLevel!.StorageProvider);
    builder.RegisterInstance<IRouter>(new Router(new Home()));
    builder.RegisterInstance<IWorkspaceManager>(new WorkspaceManager());
    builder.RegisterInstance<IProjectManager>(new ProjectManager());

    var container = builder.Build();

    desktop.MainWindow.Content = ShellModule.GetView(container);
  }, args);
