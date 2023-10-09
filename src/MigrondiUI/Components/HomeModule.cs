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

  static ItemsControl AddAndListWorkspaces(IObservable<IReadOnlyList<Workspace>> workspaces,
    Action<Workspace> onSelectWorkspace) =>
    ItemsControl()
      .ItemsSource(workspaces, mode: BindingMode.OneWay)
      .ItemTemplate(new FuncDataTemplate<Workspace>((workspace, _) =>
          DockPanel()
            .TextAlignmentLeft()
            .VerticalAlignmentCenter()
            .LastChildFill(true)
            .Children(
              Button()
                .DockRight()
                .Padding(4)
                .Content("Open")
                .OnClickHandler((_, _) => onSelectWorkspace(workspace)),
              TextBlock().DockLeft().Text(workspace.Name).FontSize(14)
            )
        )
      );

  static StackPanel View(IHomeViewModel viewModel)
  {
    viewModel.LoadWorkspaces();

    return StackPanel()
      .HorizontalAlignmentCenter()
      .VerticalAlignmentCenter()
      .TextAlignmentCenter()
      .Spacing(12)
      .Children(
        TextBlock().Text("Select or add a new workspace.").FontSize(20),
        AddButton(async () =>
        {
          var workspaces = await viewModel.AddWorkspaceSelection();
          viewModel.AddWorkspaces(workspaces);
        }),
        ScrollViewer()
          .Content(AddAndListWorkspaces(viewModel.Workspaces, viewModel.SelectWorkspace))
      );
  }

  public static StackPanel GetView(IContainer env)
  {
    IHomeViewModel vm = new HomeViewModel(env.Resolve<IWorkspaceManager>(), env.Resolve<IRouter>(), env.Resolve<IStorageProvider>());
    return View(vm);
  }
}
