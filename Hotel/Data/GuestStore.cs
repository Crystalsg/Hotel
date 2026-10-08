using Microsoft.Data.Sqlite;
using Hotel.Models;
using Hotel.Storage;

namespace Hotel.Storage;

public class GuestStore
{
    public List<Guest> FetchAll(string search = "")
    {
        var result = new List<Guest>();
        using var conn = DbHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT Id, FullName, Passport, Phone, Email, IsActive
            FROM Guests
            WHERE (@search = '') OR FullName LIKE ('%' || @search || '%') 
               OR Passport LIKE ('%' || @search || '%') OR Phone LIKE ('%' || @search || '%')
            ORDER BY FullName";
        cmd.Parameters.AddWithValue("@search", search);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Guest
            {
                Id = reader.GetInt32(0),
                FullName = reader.GetString(1),
                Passport = reader.GetString(2),
                Phone = reader.GetString(3),
                Email = reader.GetString(4),
                IsActive = reader.GetInt32(5) == 1
            });
        }
        return result;
    }

    public Guest? FindById(int id)
    {
        using var conn = DbHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, FullName, Passport, Phone, Email, IsActive FROM Guests WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapGuest(reader) : null;
    }

    public void Insert(Guest guest)
    {
        using var conn = DbHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Guests (FullName, Passport, Phone, Email, IsActive)
            VALUES (@fullName, @passport, @phone, @email, @isActive)";
        SetParams(cmd, guest);
        cmd.ExecuteNonQuery();
    }

    public void Update(Guest guest)
    {
        using var conn = DbHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE Guests 
            SET FullName = @fullName, Passport = @passport, Phone = @phone, Email = @email, IsActive = @isActive
            WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", guest.Id);
        SetParams(cmd, guest);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var conn = DbHelper.GetConnection();
        conn.Open();
        var checkCmd = conn.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(*) FROM Bookings WHERE GuestId = @id";
        checkCmd.Parameters.AddWithValue("@id", id);
        int count = Convert.ToInt32(checkCmd.ExecuteScalar());
        if (count > 0)
            throw new InvalidOperationException("Невозможно удалить гостя: у него есть бронирования.");

        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Guests WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    private void SetParams(SqliteCommand cmd, Guest g)
    {
        cmd.Parameters.AddWithValue("@fullName", g.FullName);
        cmd.Parameters.AddWithValue("@passport", g.Passport);
        cmd.Parameters.AddWithValue("@phone", g.Phone);
        cmd.Parameters.AddWithValue("@email", g.Email);
        cmd.Parameters.AddWithValue("@isActive", g.IsActive ? 1 : 0);
    }

    public bool IsPassportDuplicate(string passport, int excludeId = 0)
    {
        using var conn = DbHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Guests WHERE Passport = @passport AND Id != @excludeId";
        cmd.Parameters.AddWithValue("@passport", passport);
        cmd.Parameters.AddWithValue("@excludeId", excludeId);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    private Guest MapGuest(SqliteDataReader reader)
    {
        return new Guest
        {
            Id = reader.GetInt32(0),
            FullName = reader.GetString(1),
            Passport = reader.GetString(2),
            Phone = reader.GetString(3),
            Email = reader.GetString(4),
            IsActive = reader.GetInt32(5) == 1
        };
    }
}