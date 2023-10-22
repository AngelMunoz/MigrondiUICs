namespace MigrondiUI;

public static class Extensions
{
  public static Uri ToUri(this string input)
  {
    var uriKind = input.StartsWith("virtual:") || input.StartsWith("file:") ? UriKind.Absolute : UriKind.Relative;
    return new Uri(input, uriKind);
  }
}
