namespace MigrondiUI.Services;

using Types;

public interface IRouter
{
  public Route RouteSnapshot { get; }

  public IObservable<Route> Route { get; }

  public void Navigate(Route route);

  public bool Previous();

  public bool Forward();
}

public class Router(Route initial, int historyLength = 10) : IRouter
{
  private readonly LinkedList<Route> _history = new([initial]);
  private readonly LinkedList<Route> _forward = new();

  private readonly BehaviorSubject<Route> _routes = new(initial);

  public Route RouteSnapshot => _routes.Value;

  public IObservable<Route> Route => _routes;

  public void Navigate(Route route)
  {
    _history.AddLast(route);

    if (_history.Count > historyLength)
    {
      _history.RemoveFirst();
    }

    _forward.Clear();

    _routes.OnNext(route);
  }

  public bool Previous()
  {
    if (_history.Count == 1)
    {
      return false;
    }

    var route = _history.Last?.Value!;
    _history.RemoveLast();

    _forward.AddLast(route);

    _routes.OnNext(_history.Last?.Value!);

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

    var route = _forward.First?.Value!;

    _forward.RemoveLast();

    _history.AddLast(route);

    _routes.OnNext(route);
    return true;
  }
}
