using System.Collections.Immutable;

namespace MigrondiUI.Components;

using Autofac;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Services;
using Types;


public static class HomeModule
{
  private static Button AddButton(Func<Task> onAddWorkspace) =>
    Button()
      .HorizontalAlignmentCenter()
      .Padding(4)
      .Content("Add a new workspace.")
      .OnClickHandler((_, _) => onAddWorkspace());

  private static ItemsControl AddAndListWorkspaces(IObservable<IReadOnlyList<Workspace>> workspaces, Action<Workspace> onSelectWorkspace) =>
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

  public static Control View(IContainer env)
  {
    var viewModel = env.Resolve<IHomeViewModel>();
    var getStorageProvider = env.Resolve<Func<IStorageProvider>>();

    viewModel.LoadWorkspaces();

    async Task OnAddWorkspace()
    {
      var storage = getStorageProvider();
      var wellKnown = await storage.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);
      var options = new FolderPickerOpenOptions
      {
        Title = "Select a workspace folder",
        AllowMultiple = true,
        SuggestedStartLocation = wellKnown
      };
      var folders = await storage.OpenFolderPickerAsync(options);
      viewModel.AddWorkspaces(folders);
    };

    return StackPanel()
      .HorizontalAlignmentCenter()
      .VerticalAlignmentCenter()
      .TextAlignmentCenter()
      .Spacing(12)
      .Children(
        TextBlock().Text("Select or add a new workspace.").FontSize(20),
        AddButton(() => OnAddWorkspace()),
        ScrollViewer()
        .Content(
          AddAndListWorkspaces(viewModel.Workspaces, viewModel.SelectWorkspace)
        )
      );

  }
}
