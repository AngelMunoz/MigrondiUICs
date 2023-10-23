using Microsoft.Extensions.Logging;
using Migrondi.Core;
using Migrondi.Core.Serialization;

namespace MigrondiUI.Services;

using System.Collections.Immutable;
using System.Data;
using RepoDb;
using SqlKata.Execution;
using Types;

public interface IProjectService
{
  Project CreateProject(string name, long workspaceId);

  IImmutableList<Project> GetProjectsForWorkspace(long workspaceId);

  Task<IImmutableList<Project>> ImportProjects(IImmutableList<string> projects, Workspace workspace);

  static Project MapDynamicToProject(dynamic project)
  {
    return new Project(project.Id, project.Name, ((string)project.Path).DirectoryPathToUri(), project.WorkspaceId);
  }
}

public interface IWorkspaceService
{
  IImmutableList<Workspace> GetWorkspaces();

  Workspace CreateWorkspace(string name);

  Task<Workspace> ImportWorkspace(string name, Uri path, IImmutableList<string> projects);

  static Workspace MapDynamicToProject(dynamic workspace, IImmutableList<Project> createdProjects)
  {
    return new Workspace(workspace.Id, workspace.Name, ((string)workspace.Path).DirectoryPathToUri(), createdProjects);
  }
}

public class ProjectService(
  ILogger logger,
  IMiConfigurationSerializer miConfiguration,
  IMiMigrationSerializer miMigration,
  QueryFactory db) : IProjectService
{
  public Project CreateProject(string name, long workspaceId)
  {
    var path = new Uri($"./{name}/", UriKind.Relative);
    var id = db.Query("projects")
      .InsertGetId<long>(new { Name = name, Path = path.ToString(), WorkspaceId = workspaceId });
    return new Project(id, name, path, workspaceId);
  }

  public IImmutableList<Project> GetProjectsForWorkspace(long workspaceId)
  {
    var result = db
      .Query("projects")
      .Where("WorkspaceId", workspaceId)
      .Get()
      .Select(project => new Project(project.Id, project.Name, ((string)project.Path).FilePathToUri(), project.WorkspaceId));
    return result.ToImmutableList();
  }

  async Task ImportMigrondiProject(Workspace parent, Project project)
  {
    static async Task<string> ReadTextContents(FileInfo file)
    {
      using var reader = file.OpenText();
      return await reader.ReadToEndAsync();
    }

    var projectRoot = parent.GetProjectAbsUri(project);
    var projectDir = new DirectoryInfo(projectRoot.LocalPath);
    var config = await projectDir
      .EnumerateFiles("*.json", SearchOption.AllDirectories)
      .Where(file => file.Name.Contains("migrondi.json", StringComparison.InvariantCultureIgnoreCase))
      .ToAsyncEnumerable()
      .SelectAwait(async file => miConfiguration.Decode(await ReadTextContents(file)))
      .SingleAsync();

    var migrationsDir = new DirectoryInfo(System.IO.Path.Combine(projectRoot.LocalPath, config.migrations));
    var migrations = await migrationsDir
      .EnumerateFiles("*.sql", SearchOption.AllDirectories)
      .ToAsyncEnumerable()
      .SelectAwait(async file =>
      {
        var extractedName = Migration.ExtractFromFilename(file.Name);
        if (extractedName.IsOk)
        {
          var (name, timestamp) = extractedName.ResultValue;
          var content = await ReadTextContents(file);
          return miMigration.DecodeText(content, name);
        }

        throw new ArgumentException($"File '{file.Name}' is not a valid migration name.");
      })
      .ToListAsync();
    var configId = await db.Query("migrondi_configs").InsertGetIdAsync<long>(new
    {
      config.connection,
      config.migrations,
      driver = config.driver.AsString,
      projectId = project.Id
    });
    var headers = new[] { "name", "timestamp", "upContent", "downContent", "projectId", "configId" };
    var toInsert = migrations
      .Select(migration =>
        new object[]
        {
          migration.name,
          migration.timestamp,
          migration.upContent,
          migration.downContent,
          project.Id,
          configId
        }).ToArray();

    await db.Query("migrondi_migrations").InsertAsync(headers, toInsert);
  }

  public async Task<IImmutableList<Project>> ImportProjects(IImmutableList<string> projects,
    Workspace workspace)
  {
    var headers = new[] { "Name", "Path", "WorkspaceId" };
    try
    {
      var toInsert = projects
        .Select(project =>
          new object[] { project, $"./{project}/", workspace.Id }
        )
        .ToArray();
      await db
        .Query("projects")
        .InsertAsync(headers, toInsert);
    }
    catch (Exception e)
    {
      logger.LogError("Failed to insert projects due to {Error}", e.Message);
    }

    var wsProjects = GetProjectsForWorkspace(workspace.Id).ToImmutableList();

    await wsProjects.ToAsyncEnumerable()
      .ForEachAwaitAsync(async project => await ImportMigrondiProject(workspace, project));

    return wsProjects;
  }
}

public class WorkspaceService(QueryFactory db, IProjectService projectManager) : IWorkspaceService
{
  public async Task<Workspace> ImportWorkspace(string name, Uri path, IImmutableList<string> projects)
  {
    var ws = await db.Query("workspaces").InsertGetIdAsync<long>(new { Name = name, Path = path.ToString() });
    var dynWs = await db.Query("workspaces").FirstAsync();
    var workspace = IWorkspaceService.MapDynamicToProject(dynWs, ImmutableList.Create<Project>());
    var createdProjects = await projectManager.ImportProjects(projects, workspace);

    var queryResult = await db.Query("workspaces").Select("*").Where("Id", ws).GetAsync();

    return queryResult
      .Select(result => new Workspace(result.Id, result.Name, ((string)result.Path).FilePathToUri(), createdProjects)).Single();
  }

  public IImmutableList<Workspace> GetWorkspaces()
  {
    var result = db.Query("workspaces").Select("*").Get();
    return result
      .Select(workspace =>
      {
        var projects = projectManager.GetProjectsForWorkspace(workspace.Id);
        return new Workspace(workspace.Id, workspace.Name, ((string)workspace.Path).FilePathToUri(), projects);
      })
      .ToImmutableList();
  }

  public Workspace CreateWorkspace(string name)
  {
    var path = new Uri($"virtual://{name}/", UriKind.Absolute);
    var ws = db.Query("workspaces").InsertGetId<int>(new { Name = name, Path = path.ToString() });
    return new Workspace(ws, name, path, []);
  }
}
