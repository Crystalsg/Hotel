using Microsoft.Data.Sqlite;
using Hotel.Models;
using Hotel.Storage;

namespace Hotel.Storage;

public class ServiceStore
{
    public List<Service> FetchAll(string filter = "")
    {
        var services = new List<Service>();
        using var conn = DbHelper.GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT Id, Name, Price, Category
            FROM Services
            WHERE (@filter = '') 
               OR Name LIKE ('%' || @filter || '%')
               OR Category LIKE ('%' || @filter || '%')
            ORDER BY Name";
        cmd.Parameters.AddWithValue("@filter", filter);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            services.Add(new Service
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Price = reader.GetDecimal(2),
                Category = reader.GetString(3)
            });
        }
        return services;
    }

    public Service? FindById(int id)
    {
        using var conn = DbHelper.GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, Price, Category FROM Services WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapService(reader) : null;
    }

    public void Insert(Service service)
    {
        using var conn = DbHelper.GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Services (Name, Price, Category) VALUES (@name, @price, @category)";
        SetParams(cmd, service);
        cmd.ExecuteNonQuery();
    }

    public void Update(Service service)
    {
        using var conn = DbHelper.GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Services SET Name = @name, Price = @price, Category = @category WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", service.Id);
        SetParams(cmd, service);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        
        using var conn = DbHelper.GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Services WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    private void SetParams(SqliteCommand cmd, Service s)
    {
        cmd.Parameters.AddWithValue("@name", s.Name);
        cmd.Parameters.AddWithValue("@price", s.Price);
        cmd.Parameters.AddWithValue("@category", s.Category);
    }

    private Service MapService(SqliteDataReader reader)
    {
        return new Service
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Price = reader.GetDecimal(2),
            Category = reader.GetString(3)
        };
    }
}