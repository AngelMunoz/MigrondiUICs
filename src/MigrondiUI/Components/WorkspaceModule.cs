namespace MigrondiUI.Components;

using Autofac;
using Services;
using Types;
using ViewModels;

public static class WorkspaceModule
{

  static FuncDataTemplate<Project> ProjectTemplate(Action<Project> onSelectProject) =>
    new((project, _) =>
      StackPanel()
      .OrientationHorizontal()
      .Spacing(8)
      .Children(
        TextBlock().Text(project.Name),
        Button()
          .Content("View Details").OnClickHandler((_, _) => onSelectProject(project))
      )
    );

  static ScrollViewer View(IWorkspaceViewModel viewModel)
  {

    viewModel.LoadProjects();

    return ScrollViewer()
      .VerticalAlignmentStretch()
      .HorizontalAlignmentStretch()
      .Content(
        StackPanel()
          .Spacing(8)
          .VerticalAlignmentCenter()
          .HorizontalAlignmentCenter()
          .Children(
            TextBlock().Text("Projects").FontSize(20),
            ItemsControl()
              .ItemsSource(viewModel.ProjectList, mode: BindingMode.OneWay)
              .ItemTemplate(ProjectTemplate(viewModel.VisitProject))
          )
      );
  }

  public static Control GetView(IContainer env, Workspace workspace)
  {
    var router = env.Resolve<IRouter>();
    var projectManager = env.Resolve<IProjectManager>();
    var viewModel = new WorkspaceViewModel(workspace, router, projectManager);
    return View(viewModel);
  }
}
