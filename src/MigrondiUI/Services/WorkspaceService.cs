namespace MigrondiUI.Services;

using System.Data;
using RepoDb;
using SqlKata.Execution;
using Types;

public interface IProjectService
{
  Project CreateProject(string name, long workspaceId);

  ICollection<Project> GetProjectsForWorkspace(long workspaceId);

  Task<ICollection<Project>> ImportProjects(IReadOnlyList<string> projects, long workspaceId);
}


public interface IWorkspaceService
{
  IReadOnlyList<Workspace> GetWorkspaces();

  Workspace CreateWorkspace(string name);

  Task<Workspace> ImportWorkspace(string name, Uri path, IReadOnlyList<string> projects);
}


public class ProjectService(QueryFactory db) : IProjectService
{
  public Project CreateProject(string name, long workspaceId)
  {

    var path = new Uri($"./{name}/", UriKind.Relative);
    var id = db.Query("projects").InsertGetId<long>(new { Name = name, Path = path.ToString(), WorkspaceId = workspaceId });
    return new Project(id, name, path, workspaceId);
  }

  public ICollection<Project> GetProjectsForWorkspace(long workspaceId)
  {

    var result = db
      .Query("projects")
      .Where("WorkspaceId", workspaceId)
      .Get()
      .Select(project => new Project(project.Id, project.Name, ((string)project.Path).ToUri(), project.WorkspaceId));
    return result.ToList();
  }

  public async Task<ICollection<Project>> ImportProjects(IReadOnlyList<string> projects, long workspaceId)
  {
    try
    {
      var toInsert = projects
        .Select(project =>
          new object[] { project, $"./{project}/", workspaceId }
        )
        .ToArray();
      await db
        .Query("projects")
        .InsertAsync(new[] { "Name", "Path", "WorkspaceId" }, toInsert);
    }
    catch (System.Exception)
    {

      throw;
    }


    return GetProjectsForWorkspace(workspaceId);
  }

}

public class WorkspaceService(QueryFactory db, IProjectService projectManager) : IWorkspaceService
{
  public async Task<Workspace> ImportWorkspace(string name, Uri path, IReadOnlyList<string> projects)
  {

    var ws = await db.Query("workspaces").InsertGetIdAsync<long>(new { Name = name, Path = path.ToString() });

    var createdProjects = await projectManager.ImportProjects(projects, ws);

    var result = await db.Query("workspaces").Select("*").Where("Id", ws).GetAsync();

    return result.Select(result => new Workspace(result.Id, result.Name, ((string)result.Path).ToUri(), createdProjects)).Single();
  }

  public IReadOnlyList<Workspace> GetWorkspaces()
  {

    var result = db.Query("workspaces").Select("*").Get();
    return result
      .Select(workspace =>
      {
        var projects = projectManager.GetProjectsForWorkspace(workspace.Id);
        return new Workspace(workspace.Id, workspace.Name, ((string)workspace.Path).ToUri(), projects);
      })
      .ToList();
  }

  public Workspace CreateWorkspace(string name)
  {
    var path = new Uri("virtual://{name}/", UriKind.Absolute);
    var ws = db.Query("workspaces").InsertGetId<int>(new { Name = name, Path = path.ToString() });
    return new Workspace(ws, name, path, []);
  }
}
