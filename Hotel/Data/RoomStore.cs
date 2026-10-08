using Microsoft.Data.Sqlite;
using Hotel.Models;
using Hotel.Storage;

namespace Hotel.Storage;

public class RoomStore
{
    public List<Room> GetAll(string keyword = "")
    {
        var rooms = new List<Room>();
        using var conn = DbHelper.GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT Id, Number, Type, Capacity, PricePerNight, Status
            FROM Rooms
            WHERE (@keyword = '') 
               OR Number LIKE ('%' || @keyword || '%')
               OR Type LIKE ('%' || @keyword || '%')
            ORDER BY CAST(Number AS INTEGER)";
        cmd.Parameters.AddWithValue("@keyword", keyword);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            rooms.Add(new Room
            {
                Id = reader.GetInt32(0),
                Number = reader.GetString(1),
                Type = reader.GetString(2),
                Capacity = reader.GetInt32(3),
                PricePerNight = reader.GetDecimal(4),
                Status = reader.GetString(5)
            });
        }
        return rooms;
    }

    public Room? GetById(int id)
    {
        using var conn = DbHelper.GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Number, Type, Capacity, PricePerNight, Status FROM Rooms WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapRoom(reader) : null;
    }

    public void Add(Room room)
    {
        using var conn = DbHelper.GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Rooms (Number, Type, Capacity, PricePerNight, Status)
            VALUES (@number, @type, @capacity, @price, @status)";
        FillParams(cmd, room);
        cmd.ExecuteNonQuery();
    }

    public void Update(Room room)
    {
        using var conn = DbHelper.GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE Rooms 
            SET Number = @number, Type = @type, Capacity = @capacity, PricePerNight = @price, Status = @status
            WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", room.Id);
        FillParams(cmd, room);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var conn = DbHelper.GetConnection();
        conn.Open();
        var checkCmd = conn.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(*) FROM Bookings WHERE RoomId = @id";
        checkCmd.Parameters.AddWithValue("@id", id);
        int count = Convert.ToInt32(checkCmd.ExecuteScalar());
        if (count > 0)
            throw new InvalidOperationException("Невозможно удалить комнату: есть активные или завершённые бронирования.");

        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Rooms WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    private void FillParams(SqliteCommand cmd, Room r)
    {
        cmd.Parameters.AddWithValue("@number", r.Number);
        cmd.Parameters.AddWithValue("@type", r.Type);
        cmd.Parameters.AddWithValue("@capacity", r.Capacity);
        cmd.Parameters.AddWithValue("@price", r.PricePerNight);
        cmd.Parameters.AddWithValue("@status", r.Status);
    }

    public int CountFree()
    {
        using var conn = DbHelper.GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Rooms WHERE Status = 'Свободен'";
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    private Room MapRoom(SqliteDataReader reader)
    {
        return new Room
        {
            Id = reader.GetInt32(0),
            Number = reader.GetString(1),
            Type = reader.GetString(2),
            Capacity = reader.GetInt32(3),
            PricePerNight = reader.GetDecimal(4),
            Status = reader.GetString(5)
        };
    }
}