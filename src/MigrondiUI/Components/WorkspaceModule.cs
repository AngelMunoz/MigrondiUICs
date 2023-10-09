namespace MigrondiUI.Components;

using Autofac;
using Services;
using Types;
using ViewModels;

public static class WorkspaceModule
{
  static StackPanel View(IWorkspaceViewModel viewModel) =>
    StackPanel()
      .Spacing(8)
      .VerticalAlignmentCenter()
      .HorizontalAlignmentCenter()
      .Children(
        TextBlock().Text("Hello World!")
      );

  public static StackPanel GetView(IContainer env, Workspace workspace)
  {
    var router = env.Resolve<IRouter>();
    var projectManager = env.Resolve<IProjectManager>();
    var viewModel = new WorkspaceViewModel(workspace, router, projectManager);
    return View(viewModel);
  }
}
