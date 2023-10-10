using Autofac;
using Avalonia.Platform.Storage;
using MigrondiUI.Services;
using MigrondiUI.Types;
using MigrondiUI.ViewModels;

namespace MigrondiUI.Components;


static class ProjectModule
{

  static DockPanel View(IProjectViewModel project) =>
    DockPanel()
      .LastChildFill(true)
      .Children(
        TextBlock().Text(project.Project.Name)
      );

  public static Control GetView(IObservable<Project?> project, IDictionary<Project, IProjectViewModel> vmCache)
  {
    var content = project.Select<Project?, Control>(project =>
    {
      if (project is null)
      {
        return StackPanel()
          .HorizontalAlignmentCenter()
          .VerticalAlignmentCenter()
          .TextAlignmentCenter()
          .Children(TextBlock().Text("No project selected"));
      }
      if (vmCache.TryGetValue(project, out var vm))
      {
        return View(vm);
      }
      else
      {
        vm = new ProjectViewModel(project);
        vmCache.Add(project, vm);
        return View(vm);
      }
    });
    return ContentControl()
      .Margin(12, 0)
      .Content(content, mode: BindingMode.OneWay);
  }
}


public static class LandingModule
{

  static FuncDataTemplate<Project> ProjectNavigator =>
    new((project, _) =>
      StackPanel()
        .Spacing(4)
        .Children(
          TextBlock().Text(project.Name)
        )
    );

  static FuncTreeDataTemplate<Workspace> WorkspaceNavigator =>
    new((workspace, _) =>
      StackPanel()
        .Spacing(4)
        .Children(
          TextBlock().Text(workspace.Name)
        ),
      workspace => workspace.Projects
    );

  static TreeView WorkspaceView(IReadOnlyList<Workspace> workspaces, Action<Project> onSelectProject) =>
    TreeView()
      .DockLeft()
      .SelectionModeAlwaysSelected()
      .ItemsSource(workspaces)
      .DataTemplates(ProjectNavigator, WorkspaceNavigator)
      .OnSelectedItem((_, obs) => obs.Subscribe(obj =>
      {
        if (obj is Project selected)
        {
          onSelectProject(selected);
        }
      }));

  static ContentControl WorkspaceSidebar(IObservable<IReadOnlyList<Workspace>> workspaces, Action onSelectWorkspaces, Action<Project> onSelectProject)
  {

    var content = workspaces.Select(workspaces =>
    {
      Control selectWorkspaces =
        Button()
          .Content(TextBlock().Text("Add Workspaces"))
          .OnClickHandler((_, _) => onSelectWorkspaces());
      if (workspaces.Count == 0)
      {
        return selectWorkspaces;
      }
      return StackPanel()
        .Spacing(8)
        .Children(
          selectWorkspaces,
          WorkspaceView(workspaces, onSelectProject)
        );
    });

    return ContentControl()
      .DockLeft()
      .Content(content, mode: BindingMode.OneWay);
  }


  static ScrollViewer View(ILandingViewModel vm)
  {
    vm.LoadWorkspaces();
    var vmCache = new Dictionary<Project, IProjectViewModel>();
    return ScrollViewer()
      .Content(
        DockPanel()
        .LastChildFill(true)
        .Children(
          WorkspaceSidebar(vm.WorkspaceList, () => vm.SelectWorkspaces(), vm.SelectProject),
          ProjectModule.GetView(vm.SelectedProject, vmCache)
        )
      );
  }

  public static Control GetView(IContainer env)
  {
    var workspaceService = env.Resolve<IWorkspaceService>();
    var storageProvider = env.Resolve<IStorageProvider>();

    return View(new LandingViewModel(workspaceService, storageProvider));
  }

}
