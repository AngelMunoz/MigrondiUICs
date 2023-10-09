namespace MigrondiUI.Components;

using Autofac;
using MigrondiUI.Services;
using MigrondiUI.ViewModels;

static class ErrorPagesModule
{
  public static StackPanel NotFoundPage() =>
    StackPanel()
      .TextAlignmentCenter()
      .HorizontalAlignmentCenter()
      .VerticalAlignmentCenter()
      .Children(
        TextBlock().Text("404").FontSize(100).Margin(0, 0, 0, 20),
        TextBlock().Text("Page not found").FontSize(20)
      );
}

public static class ShellModule
{
  static DockPanel View(IShellViewModel viewModel) =>
    DockPanel()
      .HorizontalAlignmentStretch()
      .VerticalAlignmentStretch()
      .LastChildFill(true)
      .Children(
        ContentControl()
          .DockTop()
          .Content(viewModel.NavbarContent, mode: BindingMode.OneWay),
        ContentControl()
          .DockBottom()
          .Content(viewModel.PageContent, mode: BindingMode.OneWay)
      );

  public static DockPanel GetView(IContainer env)
  {
    var router = env.Resolve<IRouter>();
    var viewModel = new ShellViewModel(env, router);
    return View(viewModel);
  }
}
