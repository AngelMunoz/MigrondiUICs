namespace MigrondiUI.Services;

using Avalonia.Platform.Storage;
using MigrondiUI.Types;


public interface IWorkspaceService
{
  Task<IReadOnlyList<Workspace>> SelectWorkspaces(IStorageProvider storageProvider);

  ICollection<Project> GetProjectsForWorkspace(Workspace workspace);

  Workspace AddProjectsToWorkspace(Workspace workspace, IEnumerable<Project> projects);

  Workspace RemoveProjectFromWorkspace(Workspace workspace, Project project);

}


public class WorkspaceService : IWorkspaceService
{

  public Workspace AddProjectsToWorkspace(Workspace workspace, IEnumerable<Project> projects)
  {
    var newProjects = projects.Aggregate(workspace.Projects.ToHashSet(), (current, next) =>
    {
      current.Add(next);
      return current;
    });

    return workspace with { Projects = newProjects };
  }

  public ICollection<Project> GetProjectsForWorkspace(Workspace workspace)
  {
    var workspaces =
      new DirectoryInfo(workspace.Path.LocalPath)
      .EnumerateFiles("migrondi.json", SearchOption.AllDirectories)
      .Select(file =>
      {
        var filename =
          System.IO.Path.EndsInDirectorySeparator(file.FullName)
          ? file.FullName
          : file.FullName + System.IO.Path.DirectorySeparatorChar;

        return new Project(Guid.NewGuid(), file.Directory!.Name, new Uri(filename, UriKind.Absolute), workspace.Id);
      });
    return workspaces.ToList();
  }

  public Workspace RemoveProjectFromWorkspace(Workspace workspace, Project project)
  {
    return workspace with { Projects = workspace.Projects.Where(p => p.Id != project.Id).ToHashSet() };
  }

  public async Task<IReadOnlyList<Workspace>> SelectWorkspaces(IStorageProvider storageProvider)
  {
    var startLocation = await storageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);

    var selected = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
    {
      AllowMultiple = true,
      Title = "Select Folders To Work With",
      SuggestedStartLocation = startLocation
    });

    return selected.Select(folder =>
    {
      var wsId = Guid.NewGuid();
      var ws = new Workspace(wsId, folder.Name, folder.Path, new List<Project>());

      var projects = GetProjectsForWorkspace(ws);
      ws = AddProjectsToWorkspace(ws, projects);
      return ws;
    })
    .ToList();
  }
}
