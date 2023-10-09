namespace MigrondiUI.Components;

using Autofac;
using MigrondiUI.Services;
using MigrondiUI.ViewModels;

public static class NavbarModule
{

  static DockPanel View(INavbarViewModel viewModel) =>
    DockPanel()
      .VerticalAlignmentCenter()
      .HorizontalAlignmentStretch()
      .LastChildFill(true)
      .Children(
        StackPanel()
          .DockLeft()
          .Spacing(4)
          .HorizontalAlignmentRight()
          .OrientationHorizontal()
          .Children(
            Button()
              .DockLeft()
              .IsEnabled(viewModel.CanGoBack, mode: BindingMode.OneWay)
              .Content("Back")
              .OnClickHandler((_, _) => viewModel.GoBack()),
            Button()
              .DockLeft()
              .IsEnabled(viewModel.CanGoForward, mode: BindingMode.OneWay)
              .Content("Forward")
              .OnClickHandler((_, _) => viewModel.GoForward())
        ),
        TextBlock()
          .DockRight()
          .FontSize(16)
          .Padding(8, 4)
          .TextAlignmentEnd()
          .VerticalAlignmentCenter()
          .Text(viewModel.RouteName, mode: BindingMode.OneWay)
      );


  public static DockPanel DefaultNavbar(IContainer env)
  {
    var router = env.Resolve<IRouter>();
    var viewModel = new NavbarViewModel(router);
    return View(viewModel);
  }
}
