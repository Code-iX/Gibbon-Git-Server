using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Gibbon.Git.Server.Tests.TestHelper;

public class SqliteConnectionFactory : IDbConnectionFactory
{
    private DbConnection _connection = null!;

    public void ConfigureDbContext(DbContextOptionsBuilder optionsBuilder)
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        optionsBuilder.UseSqlite(_connection);
    }

    public void Cleanup()
    {
        _connection.Close();
        _connection.Dispose();
    }
}
