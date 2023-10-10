namespace MigrondiUI.ViewModels;

using Avalonia.Platform.Storage;
using MigrondiUI.Services;
using MigrondiUI.Types;


public interface ILandingViewModel
{
  IObservable<IReadOnlyList<Workspace>> WorkspaceList { get; }

  IObservable<Project?> SelectedProject { get; }

  Task SelectWorkspaces();

  void LoadWorkspaces();

  void SelectProject(Project project);

}

public class LandingViewModel(IWorkspaceService workspaceService, IStorageProvider storageProvider) : ILandingViewModel
{
  private readonly BehaviorSubject<IReadOnlyList<Workspace>> _workspaceList = new([]);
  private readonly BehaviorSubject<Project?> _selectedProject = new(null);

  public IObservable<IReadOnlyList<Workspace>> WorkspaceList => _workspaceList;

  public IObservable<Project?> SelectedProject => _selectedProject.DistinctUntilChanged();

  public async Task SelectWorkspaces()
  {
    var workspaces = await workspaceService.SelectWorkspaces(storageProvider);

    _workspaceList.OnNext(workspaces);
  }

  public void SelectProject(Project project)
  {
    _selectedProject.OnNext(project);
  }

  public void LoadWorkspaces()
  {
    // TODO: Load from local database
    Debug.WriteLine("TODO: Load from local database");
  }
}
