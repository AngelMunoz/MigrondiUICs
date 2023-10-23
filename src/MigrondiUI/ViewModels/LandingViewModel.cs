namespace MigrondiUI.ViewModels;

using System.Collections.Immutable;
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
  IProjectViewModel? GetOrAddProjectVm(Project? project);

}

public class LandingViewModel(IWorkspaceService workspaceService, IStorageProvider storageProvider) : ILandingViewModel
{
  private readonly BehaviorSubject<IReadOnlyList<Workspace>> _workspaceList = new([]);
  private readonly BehaviorSubject<Project?> _selectedProject = new(null);

  private readonly Dictionary<Project, IProjectViewModel> _projectViewModels = [];

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
            .ToImmutableList();
          var path = directory.Path.ToString().EndsWith(System.IO.Path.DirectorySeparatorChar)
            ? directory.Path
            : new Uri($"{directory.Path}{System.IO.Path.DirectorySeparatorChar}", UriKind.Absolute);
          return await workspaceService.ImportWorkspace(directory.Name, path, projects);
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

  public IProjectViewModel? GetOrAddProjectVm(Project? project)
  {
    if (project == null)
      return null;

    if (_projectViewModels.TryGetValue(project, out var vm))
      return vm;

    vm = new ProjectViewModel(project);
    _projectViewModels.Add(project, vm);
    return vm;
  }
}
