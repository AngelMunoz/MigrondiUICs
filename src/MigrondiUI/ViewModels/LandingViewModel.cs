namespace MigrondiUI.ViewModels;

using Avalonia.Platform.Storage;
using MigrondiUI.Services;
using MigrondiUI.Types;


public interface ILandingViewModel
{
  IObservable<IReadOnlyList<Workspace>> WorkspaceList { get; }

  IObservable<Project?> SelectedProject { get; }

  Task ImportWorkspaces();

  void LoadWorkspaces();

  void SelectProject(Project project);

}

public class LandingViewModel(IWorkspaceService workspaceService, IStorageProvider storageProvider) : ILandingViewModel
{
  private readonly BehaviorSubject<IReadOnlyList<Workspace>> _workspaceList = new([]);
  private readonly BehaviorSubject<Project?> _selectedProject = new(null);

  public IObservable<IReadOnlyList<Workspace>> WorkspaceList => _workspaceList;

  public IObservable<Project?> SelectedProject => _selectedProject.DistinctUntilChanged();

  public async Task ImportWorkspaces()
  {
    var selectedDirectories = await
      storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
      {
        AllowMultiple = true,
        Title = "Select The Directory To Import."
      });

    var workspaces =
      await selectedDirectories
        .ToAsyncEnumerable()
        .SelectAwait(async directory =>
        {
          var projects =
            new DirectoryInfo(directory.Path.LocalPath)
            .EnumerateFiles("migrondi.json", SearchOption.AllDirectories)
            .Select(file => file.Directory!.Name)
            .ToList();
          return await workspaceService.ImportWorkspace(directory.Name, directory.Path, projects);
        })
        .ToListAsync();

    _workspaceList.OnNext(workspaces);
  }

  public void SelectProject(Project project)
  {
    _selectedProject.OnNext(project);
  }

  public void LoadWorkspaces()
  {
    _workspaceList.OnNext(workspaceService.GetWorkspaces());
  }
}
