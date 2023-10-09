namespace MigrondiUI.Components;

using Autofac;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Services;
using Types;

public static class HomeModule
{
  static Button AddButton(Func<Task> onAddWorkspace) =>
    Button()
      .HorizontalAlignmentCenter()
      .Padding(4)
      .Content("Add a new workspace.")
      .OnClickHandler((_, _) => onAddWorkspace());

  static FuncDataTemplate<Workspace> WorkspaceList(Action<Workspace> onSelectedWorkspace) =>
    new((workspace, _) =>
      DockPanel()
        .TextAlignmentLeft()
        .VerticalAlignmentCenter()
        .LastChildFill(true)
        .Children(
          Button()
            .DockRight()
            .Padding(4)
            .Content("Open")
            .OnClickHandler((_, _) => onSelectedWorkspace(workspace)),
          TextBlock().DockLeft().Text(workspace.Name).FontSize(14)
        )
    );

  static ContentControl View(IHomeViewModel viewModel)
  {
    viewModel.LoadWorkspaces();

    return ContentControl()
      .HorizontalAlignmentCenter()
      .VerticalAlignmentCenter()
      .Content(
        StackPanel()
        .TextAlignmentCenter()
        .Spacing(12)
        .Children(
          TextBlock().Text("Select or add a new workspace.").FontSize(20),
          AddButton(async () =>
          {
            var workspaces = await viewModel.AddWorkspaceSelection();
            viewModel.AddWorkspaces(workspaces);
          }),
          ItemsControl()
            .ItemsSource(viewModel.Workspaces, mode: BindingMode.OneWay)
            .ItemTemplate(WorkspaceList(viewModel.SelectWorkspace))
        )
    );
  }

  public static Control GetView(IContainer env)
  {
    IHomeViewModel vm = new HomeViewModel(
      env.Resolve<IWorkspaceManager>(),
      env.Resolve<IRouter>(),
      env.Resolve<IStorageProvider>()
    );
    return View(vm);
  }
}
