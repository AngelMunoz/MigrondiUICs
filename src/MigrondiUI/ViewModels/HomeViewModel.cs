namespace MigrondiUI.Services;

using Avalonia.Platform.Storage;
using MigrondiUI.Types;


public interface IHomeViewModel
{
  IReadOnlyList<Workspace> WorkspacesSnapshot { get; }

  IObservable<IReadOnlyList<Workspace>> Workspaces { get; }

  void AddWorkspaces(IEnumerable<IStorageFolder> folders);

  void LoadWorkspaces();

  void SelectWorkspace(Workspace workspace);

  Task<IEnumerable<IStorageFolder>> AddWorkspaceSelection();
}

public class HomeViewModel(IWorkspaceManager manager, IRouter router, IStorageProvider storageProvider) : IHomeViewModel
{
  private readonly BehaviorSubject<IReadOnlyList<Workspace>> _workspaces = new([]);

  public IReadOnlyList<Workspace> WorkspacesSnapshot => _workspaces.Value;

  public IObservable<IReadOnlyList<Workspace>> Workspaces => _workspaces;

  public void AddWorkspaces(IEnumerable<IStorageFolder> newWorkspaces)
  {
    manager.AddNewWorkspaces(newWorkspaces);
    _workspaces.OnNext(manager.GetWorkspaces());
  }

  public void LoadWorkspaces()
  {
    _workspaces.OnNext(manager.GetWorkspaces());
  }

  public void SelectWorkspace(Workspace workspace)
  {
    router.Navigate(new WorkspaceDetail(workspace));
  }

  public async Task<IEnumerable<IStorageFolder>> AddWorkspaceSelection()
  {
    var wellKnown = await storageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);
    var options = new FolderPickerOpenOptions
    {
      Title = "Select a workspace folder",
      AllowMultiple = true,
      SuggestedStartLocation = wellKnown
    };
    return await storageProvider.OpenFolderPickerAsync(options);
  }
}
