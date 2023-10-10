using Autofac;
using MigrondiUI.Components;
using MigrondiUI.Services;
using MigrondiUI.Types;

using Avalonia.Diagnostics;


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

    var builder = new ContainerBuilder();

    builder.RegisterInstance(topLevel!.StorageProvider);
    builder.RegisterInstance<IRouter>(new Router(new Home()));
    builder.RegisterInstance<IWorkspaceService>(new WorkspaceService());

    var container = builder.Build();

    desktop.MainWindow.Content = ShellModule.GetView(container);
  }, args);
