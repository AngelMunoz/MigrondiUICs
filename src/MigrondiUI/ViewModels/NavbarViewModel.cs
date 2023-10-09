namespace MigrondiUI.ViewModels;

using MigrondiUI.Services;
using MigrondiUI.Types;


public interface INavbarViewModel
{
  IObservable<string> RouteName { get; }
  IObservable<bool> CanGoBack { get; }
  IObservable<bool> CanGoForward { get; }

  void GoBack();
  void GoForward();

  void Navigate(Route route);

}


public class NavbarViewModel(IRouter router) : INavbarViewModel
{
  public IObservable<bool> CanGoBack => router.CanGoBack;

  public IObservable<bool> CanGoForward => router.CanGoForward;

  public IObservable<string> RouteName => router.Route.DistinctUntilChanged().Select(route => route.GetName());

  public void GoBack()
  {
    router.GoBack();
  }

  public void GoForward()
  {
    router.Forward();
  }

  public void Navigate(Route route)
  {
    router.Navigate(route);
  }
}
