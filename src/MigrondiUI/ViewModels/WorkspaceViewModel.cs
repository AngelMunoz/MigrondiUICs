using MigrondiUI.Services;
using MigrondiUI.Types;

namespace MigrondiUI.ViewModels;

public interface IWorkspaceViewModel
{
  Workspace Workspace { get; }
  IObservable<IReadOnlyList<Project>> ProjectList { get; }
  void LoadProjects();
  void VisitProject(Project project);
}

public class WorkspaceViewModel(
  Workspace workspace,
  IRouter router,
  IProjectManager projectManager
) : IWorkspaceViewModel
{
  readonly Subject<IReadOnlyList<Project>> _projects = new();

  public Workspace Workspace => workspace;

  public IObservable<IReadOnlyList<Project>> ProjectList => _projects;

  public void LoadProjects()
  {
    var projects = projectManager.LoadProjects(workspace);
    _projects.OnNext(projects);
  }

  public void VisitProject(Project project)
  {
    router.Navigate(new ProjectDetail(project));
  }
}
