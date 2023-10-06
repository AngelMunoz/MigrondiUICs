namespace MigrondiUI.Types;

// App Types
public sealed record Workspace(string Name, Uri Path);

public sealed record Project(Guid ProjectId, Uri WorkspacePath, string Path);


// App Routes

public abstract record Route { }

public sealed record Home : Route;

public sealed record WorkspaceDetail(Workspace Workspace) : Route;
