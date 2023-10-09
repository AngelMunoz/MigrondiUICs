namespace MigrondiUI.Services;

using Avalonia.Platform.Storage;
using MigrondiUI.Types;

public enum ProjectChangeType
{
  Created,
  Deleted
}


public interface IWorkspaceManager
{
  Workspace? GetWorkspace(Uri workspacePath);

  IReadOnlyList<Workspace> GetWorkspaces();

  void AddNewWorkspaces(IEnumerable<IStorageFolder> folders);

  void DeleteWorkspace(Uri workspacePath);

  void UpdateWorkspaceName(Uri workspacePath, string newName);

  IObservable<(Workspace, string, ProjectChangeType)> MonitorWorkspace(Workspace workspace);

}

public class WorkspaceManager : IWorkspaceManager
{
  private readonly Dictionary<Uri, Workspace> _workspaces = [];

  private readonly Dictionary<Workspace, IList<Project>> _projects = [];

  public IReadOnlyList<Workspace> GetWorkspaces()
  {
    return _workspaces.Values.ToList();
  }

  public Workspace? GetWorkspace(Uri workspacePath)
  {
    return _workspaces.TryGetValue(workspacePath, out var workspace) ? workspace : null;
  }

  public void AddNewWorkspaces(IEnumerable<IStorageFolder> folders)
  {
    foreach (var folder in folders)
    {
      if (!_workspaces.ContainsKey(folder.Path))
      {
        var workspace = new Workspace(folder.Name, folder.Path);
        _workspaces.Add(workspace.Path, workspace);
      }
    }
  }

  public void DeleteWorkspace(Uri workspacePath)
  {
    _workspaces.Remove(workspacePath);
  }

  public void UpdateWorkspaceName(Uri workspacePath, string newName)
  {
    if (_workspaces.TryGetValue(workspacePath, out var workspace))
    {
      _workspaces.Remove(workspace.Path);
      _workspaces.Add(workspacePath, new Workspace(newName, workspacePath));
    }
  }


  public IObservable<(Workspace, string, ProjectChangeType)> MonitorWorkspace(Workspace workspace)
  {
    var sub = new Subject<(Workspace, string, ProjectChangeType)>();
    FileSystemWatcher watcher = new()
    {
      Path = workspace.Path.LocalPath,
      NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.DirectoryName,
      EnableRaisingEvents = true
    };

    void onWsChange(object _, FileSystemEventArgs e)
    {
      if (e.ChangeType == WatcherChangeTypes.Created)
      {
        sub.OnNext((workspace, e.FullPath, ProjectChangeType.Created));
      }
      if (e.ChangeType == WatcherChangeTypes.Deleted)
      {
        sub.OnNext((workspace, e.FullPath, ProjectChangeType.Deleted));
      }
    }
    watcher.Created += onWsChange;
    watcher.Deleted += onWsChange;
    sub.Finally(() =>
    {
      watcher.Created -= onWsChange;
      watcher.Deleted -= onWsChange;
      watcher.Dispose();
    });

    return sub;
  }

}
