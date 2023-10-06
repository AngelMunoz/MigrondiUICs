using MigrondiUI.Types;

namespace MigrondiUI.Components;



public static class WorkspaceModule
{


  public static Control View(Workspace workspace) =>
    StackPanel().Children(
      TextBlock().Text("Hello World!")
    );
}
