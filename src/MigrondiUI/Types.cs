namespace MigrondiUI.Types;

// App Types

/// <summary>
/// Represents a particular working set of migrations with it's own Migrondi Configuration.
/// </summary>
/// <param name="Id"></param>
/// <param name="Name">Human readable string for the user to identify the project</param>
/// <param name="Path">Relative location to the parent's workspace</param>
/// <param name="WorkspaceId"></param>
public record Project(long Id, string Name, Uri Path, long WorkspaceId);


/// <summary>
/// It is the main organization unit for working with migrations.
/// It contains a collection of projects.
/// </summary>
/// <param name="Id"></param>
/// <param name="Name">Human readable string for the user to identify the project</param>
/// <param name="Path">Absolute location for this project</param>
/// <param name="Projects"></param>
public record Workspace(long Id, string Name, Uri Path, ICollection<Project> Projects);

public static class TypeExtensions
{
  public static bool IsVirtual(this Project project)
  {
    return project.Path.Scheme == "virtual";
  }

  public static bool IsVirtual(this Workspace workspace)
  {
    return workspace.Path.Scheme == "virtual";
  }

  public static Uri GetProjectAbsUri(this Workspace workspace, Project project)
  {
    return new Uri(workspace.Path, project.Path);
  }
}

// App Routes

public abstract record Route
{
}

public sealed record Home : Route;

public sealed record WorkspaceDetail(Workspace Workspace) : Route;

public sealed record ProjectDetail(Project Project) : Route;

public static class RouteExtensions
{
  public static string GetName(this Route route) => route switch
  {
    Home => "Migrondi UI",
    WorkspaceDetail ws => $"Workspace: {ws.Workspace.Name}.",
    ProjectDetail p => $"Project: {p.Project.Name}.",
    _ => "Unknown."
  };
}
