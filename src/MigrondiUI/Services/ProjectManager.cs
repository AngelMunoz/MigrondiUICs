namespace MigrondiUI.Services;

using Avalonia.Platform.Storage;
using Types;

public interface IProjectManager
{
  IReadOnlyList<Project> LoadProjects(Workspace workspace);

  Project AddProject(Workspace workspace, IStorageFolder storageFolder);

  bool RemoveProject(Project project);

  bool RenameProject(Project project, string newName);
}

public class ProjectManager : IProjectManager
{
  private readonly HashSet<Project> _projects = new();

  public IReadOnlyList<Project> LoadProjects(Workspace workspace)
  {
    var wsPath = workspace.Path.LocalPath;

    var dirInfo = new DirectoryInfo(wsPath);

    var found =
      dirInfo
        .EnumerateDirectories()
        .Select(dir => new Project(dir.Name, workspace.Path, new Uri(dir.FullName, UriKind.Absolute)));
    foreach (var project in found)
    {
      _projects.Add(project);
    }

    return _projects.ToList();
  }

  public Project AddProject(Workspace workspace, IStorageFolder storageFolder)
  {
    var project = new Project(storageFolder.Name, workspace.Path, storageFolder.Path);
    _projects.Add(project);
    return project;
  }

  public bool RemoveProject(Project project)
  {
    return _projects.Remove(project);
  }

  public bool RenameProject(Project project, string newName)
  {
    _projects.ExceptWith([project]);
    return _projects.Add(project with { Name = newName });
  }
}

