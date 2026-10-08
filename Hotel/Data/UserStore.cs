using Microsoft.Data.Sqlite;
using Hotel.Models;
using Hotel.Services;
using Hotel.Storage;

namespace Hotel.Storage;

public class UserStore
{
    public User? FindByLogin(string login)
    {
        using var conn = DbHelper.GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT u.Id, u.Login, u.PasswordHash, u.PasswordSalt, u.FullName, u.IsActive, r.Name AS RoleName
            FROM Users u
            JOIN Roles r ON r.Id = u.RoleId
            WHERE u.Login = @login";
        cmd.Parameters.AddWithValue("@login", login);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapUser(reader) : null;
    }

    public bool IsLoginTaken(string login)
    {
        return FindByLogin(login) != null;
    }

    public void RegisterNewUser(string login, string password, string fullName, int roleId)
    {
        var salt = SecurityHelper.GenerateSalt();
        var hash = SecurityHelper.HashPassword(password, salt);
        using var conn = DbHelper.GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Users (Login, PasswordHash, PasswordSalt, FullName, RoleId, IsActive, CreatedAt)
            VALUES (@login, @hash, @salt, @fullName, @roleId, 1, @createdAt)";
        cmd.Parameters.AddWithValue("@login", login);
        cmd.Parameters.AddWithValue("@hash", hash);
        cmd.Parameters.AddWithValue("@salt", salt);
        cmd.Parameters.AddWithValue("@fullName", fullName);
        cmd.Parameters.AddWithValue("@roleId", roleId);
        cmd.Parameters.AddWithValue("@createdAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        cmd.ExecuteNonQuery();
    }

    public void LogLoginAttempt(string login, bool success, string message)
    {
        using var conn = DbHelper.GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO LoginAttempts (UserLogin, IsSuccess, Message, CreatedAt)
            VALUES (@login, @success, @message, @createdAt)";
        cmd.Parameters.AddWithValue("@login", login);
        cmd.Parameters.AddWithValue("@success", success ? 1 : 0);
        cmd.Parameters.AddWithValue("@message", message);
        cmd.Parameters.AddWithValue("@createdAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        cmd.ExecuteNonQuery();
    }

    public List<LoginAttempt> GetAllAttempts()
    {
        var attempts = new List<LoginAttempt>();
        using var conn = DbHelper.GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, UserLogin, IsSuccess, Message, CreatedAt FROM LoginAttempts ORDER BY CreatedAt DESC";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            attempts.Add(new LoginAttempt
            {
                Id = reader.GetInt32(0),
                UserLogin = reader.GetString(1),
                IsSuccess = reader.GetInt32(2) == 1,
                Message = reader.GetString(3),
                CreatedAt = reader.GetString(4)
            });
        }
        return attempts;
    }

    private User MapUser(SqliteDataReader reader)
    {
        return new User
        {
            Id = reader.GetInt32(0),
            Login = reader.GetString(1),
            PasswordHash = reader.GetString(2),
            PasswordSalt = reader.GetString(3),
            FullName = reader.GetString(4),
            IsActive = reader.GetInt32(5) == 1,
            RoleName = reader.GetString(6)
        };
    }
}