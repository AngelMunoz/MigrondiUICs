namespace MigrondiUI;

public static class Extensions
{
  public static Uri FilePathToUri(this string input)
  {
    var uriKind = input.StartsWith("virtual:") || input.StartsWith("file:") ? UriKind.Absolute : UriKind.Relative;
    return new Uri(input, uriKind);
  }

  public static Uri DirectoryPathToUri(this string input)
  {
    var uriKind = input.StartsWith("virtual:") || input.StartsWith("file:") ? UriKind.Absolute : UriKind.Relative;
    return input.EndsWith(System.IO.Path.DirectorySeparatorChar)
      ? new Uri(input, uriKind)
      : new Uri($"{input}{System.IO.Path.DirectorySeparatorChar}", uriKind);
  }

  public static string EnsureTrailingSlash(this string input)
  {
    return input.EndsWith(System.IO.Path.DirectorySeparatorChar)
      ? input
      : $"{input}{System.IO.Path.DirectorySeparatorChar}";
  }

}
