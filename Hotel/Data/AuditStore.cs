using Microsoft.Data.Sqlite;
using Hotel.Models;
using Hotel.Storage;

namespace Hotel.Storage;

public class AuditStore
{
    public void LogAction(string userLogin, string action, string entity, string entityId, string details)
    {
        using var conn = DbHelper.GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO ActionLogs (UserLogin, ActionType, EntityName, EntityId, Details, CreatedAt)
            VALUES (@login, @action, @entity, @entityId, @details, @createdAt)";
        cmd.Parameters.AddWithValue("@login", userLogin);
        cmd.Parameters.AddWithValue("@action", action);
        cmd.Parameters.AddWithValue("@entity", entity);
        cmd.Parameters.AddWithValue("@entityId", entityId);
        cmd.Parameters.AddWithValue("@details", details);
        cmd.Parameters.AddWithValue("@createdAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        cmd.ExecuteNonQuery();
    }

    public List<ActionLog> FetchAllLogs()
    {
        var logs = new List<ActionLog>();
        using var conn = DbHelper.GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, UserLogin, ActionType, EntityName, EntityId, Details, CreatedAt FROM ActionLogs ORDER BY CreatedAt DESC";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            logs.Add(new ActionLog
            {
                Id = reader.GetInt32(0),
                UserLogin = reader.GetString(1),
                ActionType = reader.GetString(2),
                EntityName = reader.GetString(3),
                EntityId = reader.GetString(4),
                Details = reader.GetString(5),
                CreatedAt = reader.GetString(6)
            });
        }
        return logs;
    }
}