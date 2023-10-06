namespace MigrondiUI;

public static class ObservableExtensions
{
  public static IObservable<T> Tap<T>(this IObservable<T> obs, Action<T> onTap)
  {
    return obs.Select(value =>
    {
      onTap(value);
      return value;
    });
  }
}