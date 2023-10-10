namespace MigrondiUI.Types;

// App Types

public sealed record Project(Guid Id, string Name, Uri Path, Guid WorkspaceId);
public sealed record Workspace(Guid Id, string Name, Uri Path, ICollection<Project> Projects);

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
