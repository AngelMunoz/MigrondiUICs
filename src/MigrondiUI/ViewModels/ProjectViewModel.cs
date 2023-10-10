namespace MigrondiUI.ViewModels;

using Migrondi.Core;
using MigrondiUI.Types;


public interface IProjectViewModel
{
  Project Project { get; }

  MigrondiConfig Configuration { get; }

  IReadOnlyCollection<Migration> Migrations { get; }


}


public class ProjectViewModel(Project project) : IProjectViewModel
{
  public Project Project => project;

  public MigrondiConfig Configuration => throw new NotImplementedException();

  public IReadOnlyCollection<Migration> Migrations => [];
}
