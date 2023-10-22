namespace MigrondiUI.Services;

using Migrondi.Core;
using MigrondiUI.Types;
using System.Data;

public class LocalMigrationsHandler()
{


  public static void Initialize(IMigrondi migrondi)
  {
    var migrationsList = migrondi.MigrationsList();
    // run migrations if they are missing
    if (migrationsList.Any(status => status.IsPending))
    {
      migrondi.RunUp();
    }
  }


  public static (MigrondiConfig, Uri, Uri) GetMigrondiParams()
  {
    var config = new MigrondiConfig("Data Source=./local.db", "./sql/", "__migrondi_migrations", MigrondiDriver.Sqlite);
    var cwd = System.IO.Path.EndsInDirectorySeparator(AppContext.BaseDirectory)
      ? AppContext.BaseDirectory
      : AppContext.BaseDirectory + System.IO.Path.DirectorySeparatorChar;

    return (config, new Uri(cwd, UriKind.Absolute), new Uri("./sql/", UriKind.Relative));
  }
}
