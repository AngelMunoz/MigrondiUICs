namespace MigrondiUI.Services;

using Types;

public interface IRouter
{
  public Route RouteSnapshot { get; }

  public IObservable<Route> Route { get; }
  public IObservable<bool> CanGoBack { get; }
  public IObservable<bool> CanGoForward { get; }

  public void Navigate(Route route);

  public bool GoBack();

  public bool Forward();
}

record struct NavigationData(Route Route, bool CanGoBack, bool CanGoForward);

public class Router(Route initial, int historyLength = 10) : IRouter
{
  private readonly LinkedList<NavigationData> _history = new([new NavigationData(initial, false, false)]);
  private readonly LinkedList<NavigationData> _forward = new();

  private readonly BehaviorSubject<NavigationData> _routes = new(new NavigationData(initial, false, false));

  public Route RouteSnapshot => _routes.Value.Route;

  public IObservable<Route> Route => _routes.Select(x => x.Route);

  public IObservable<bool> CanGoBack => _routes.Select(x => x.CanGoBack);

  public IObservable<bool> CanGoForward => _routes.Select(x => x.CanGoForward);

  public void Navigate(Route route)
  {
    var info = new NavigationData(route, _history.Count >= 1, false);
    _history.AddLast(info);

    if (_history.Count > historyLength)
    {
      _history.RemoveFirst();
    }

    _forward.Clear();

    _routes.OnNext(info);
  }

  public bool GoBack()
  {
    if (_history.Count == 1)
    {
      return false;
    }

    NavigationData route = _history.Last!.Value with { CanGoForward = true, CanGoBack = _history.Count >= 2 };

    _history.RemoveLast();

    _forward.AddLast(route);

    _routes.OnNext(_history.Last!.Value with { CanGoForward = true, CanGoBack = _history.Count >= 2 });

    if (_forward.Count > historyLength)
    {
      _forward.RemoveFirst();
    }

    return true;
  }

  public bool Forward()
  {
    if (_forward.Count == 0)
    {
      return false;
    }

    var route = _forward.First!.Value with { CanGoForward = _forward.Count >= 1, CanGoBack = _history.Count >= 1 };

    _forward.RemoveLast();

    _history.AddLast(route);

    _routes.OnNext(route);
    return true;
  }
}
