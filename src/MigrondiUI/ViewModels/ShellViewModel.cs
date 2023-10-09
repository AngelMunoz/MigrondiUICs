namespace MigrondiUI.ViewModels;

using Autofac;
using MigrondiUI.Components;
using MigrondiUI.Services;
using MigrondiUI.Types;

public interface IShellViewModel
{
  IObservable<Control> PageContent { get; }

  IObservable<Control> NavbarContent { get; }
}

public class ShellViewModel(IContainer container, IRouter router) : IShellViewModel
{
  private readonly IObservable<Route> _pageData = router.Route.DistinctUntilChanged();

  public IObservable<Control> PageContent => _pageData.Select(route =>
      {
        return route switch
        {
          Home => HomeModule.GetView(container),
          WorkspaceDetail workspaceDetail => WorkspaceModule.GetView(container, workspaceDetail.Workspace),
          _ => ErrorPagesModule.NotFoundPage()
        };
      });

  public IObservable<Control> NavbarContent => _pageData.Select(route =>
      {
        return route switch
        {
          // add cases as required
          _ => NavbarModule.DefaultNavbar(container)
        };
      });
}

