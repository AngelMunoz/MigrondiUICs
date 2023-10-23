namespace MigrondiUI.Services;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.FSharp.Core;
using Migrondi.Core;
using Migrondi.Core.Database;
using Migrondi.Core.FileSystem;
using MigrondiUI.Types;
using SqlKata.Execution;


static class Dynamic
{

  public static Migration ToMigration(dynamic row)
  {
    return new Migration(
      row.name,
      row.timestamp,
      row.upContent,
      row.downContent
    );
  }
  public static MigrondiConfig ToMigrondiConfig(dynamic row)
  {
    return new MigrondiConfig(
      row.connection,
      row.migrations,
      row.tableName,
      MigrondiDriver.FromString((string)row.migrondiDriver)
    );
  }
}


public class MiProjectFileSystem(QueryFactory db, Project project) : IMiFileSystem
{
  public IReadOnlyList<Migration> ListMigrations(string migrationsLocation)
  {
    return db
      .Query(migrationsLocation)
      .Select("*")
      .Where("projectId", project.Id)
      .Get()
      .Select(Dynamic.ToMigration)
      .ToList();
  }

  public async Task<IReadOnlyList<Migration>> ListMigrationsAsync(string migrationsLocation, [OptionalArgument] FSharpOption<CancellationToken> cancellationToken)
  {
    return await db
      .Query(migrationsLocation)
      .Select("*")
      .Where("projectId", project.Id)
      .GetAsync()
      .ToAsyncEnumerable()
      .Select(Dynamic.ToMigration)
      .ToListAsync();
  }

  public MigrondiConfig ReadConfiguration(string readFrom)
  {
    return db
      .Query(readFrom)
      .Select("*")
      .Where("projectId", project.Id)
      .Get()
      .Select(Dynamic.ToMigrondiConfig)
      .First();
  }

  public async Task<MigrondiConfig> ReadConfigurationAsync(string readFrom, [OptionalArgument] FSharpOption<CancellationToken> cancellationToken)
  {
    return await db
      .Query(readFrom)
      .Select("*")
      .Where("projectId", project.Id)
      .GetAsync()
      .ToAsyncEnumerable()
      .Select(Dynamic.ToMigrondiConfig)
      .FirstAsync();
  }

  public Migration ReadMigration(string migrationName)
  {
    return db
      .Query("migrondi_migrations")
      .Select("*")
      .Where("name", migrationName)
      .Get()
      .Select(Dynamic.ToMigration)
      .First();
  }

  public async Task<Migration> ReadMigrationAsync(string migrationName, [OptionalArgument] FSharpOption<CancellationToken> cancellationToken)
  {
    return await db
      .Query("migrondi_migrations")
      .Select("*")
      .Where("name", migrationName)
      .GetAsync()
      .ToAsyncEnumerable()
      .Select(Dynamic.ToMigration)
      .FirstAsync();
  }

  public void WriteConfiguration(MigrondiConfig config, string writeTo)
  {
    throw new NotImplementedException();
  }

  public Task WriteConfigurationAsync(MigrondiConfig config, string writeTo, [OptionalArgument] FSharpOption<CancellationToken> cancellationToken)
  {
    throw new NotImplementedException();
  }

  public void WriteMigration(Migration migration, string migrationName)
  {
    throw new NotImplementedException();
  }

  public Task WriteMigrationAsync(Migration migration, string migrationName, [OptionalArgument] FSharpOption<CancellationToken> cancellationToken)
  {
    throw new NotImplementedException();
  }
}
