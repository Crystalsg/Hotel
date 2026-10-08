using Microsoft.Data.Sqlite;
namespace Hotel.Storage; 

public static class DbHelper
{
    private const string DatabaseFile = "hotel.db";
    private static string ConnectionString => $"Data Source={DatabaseFile}";

    public static SqliteConnection GetConnection()
    {
        return new SqliteConnection(ConnectionString);
    }
}