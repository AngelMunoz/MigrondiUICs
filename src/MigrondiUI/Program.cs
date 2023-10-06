using Autofac;
using Avalonia.Platform.Storage;
using MigrondiUI.Components;
using MigrondiUI.Services;
using MigrondiUI.Types;

var builder = new ContainerBuilder();

AppBuilder.Configure<Application>()
  .UsePlatformDetect()
  .UseFluentTheme(ThemeVariant.Default)
  .StartWithClassicDesktopLifetime(desktop =>
  {
    var window = new Window { Title = "MogrondiUI" };

    builder.RegisterInstance<IRouter>(new Router(new Home()));
    builder.RegisterInstance<IWorkspaceManager>(new WorkspaceManager());
    builder.Register<IHomeViewModel>(services =>
      new HomeViewModel(services.Resolve<IWorkspaceManager>(), services.Resolve<IRouter>()));
    builder
      .RegisterInstance<Func<IStorageProvider>>(() =>
      {
        var topmost = TopLevel.GetTopLevel(window);
        return topmost!.StorageProvider;
      });

    var container = builder.Build();


    desktop.MainWindow = window;

    window.Content = HomeModule.View(container);
  }, args);
