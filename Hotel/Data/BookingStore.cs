using Microsoft.Data.Sqlite;
using Hotel.Models;
using Hotel.Storage;

namespace Hotel.Storage;

public class BookingStore
{
    public List<Booking> FetchAll(string searchText = "")
    {
        var bookings = new List<Booking>();
        using var conn = DbHelper.GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT b.Id, b.GuestId, g.FullName, b.RoomId, r.Number, 
                   b.CheckInDate, b.CheckOutDate, b.Status, b.TotalAmount
            FROM Bookings b
            JOIN Guests g ON g.Id = b.GuestId
            JOIN Rooms r ON r.Id = b.RoomId
            WHERE (@search = '') 
               OR g.FullName LIKE ('%' || @search || '%')
               OR r.Number LIKE ('%' || @search || '%')
            ORDER BY b.CheckInDate DESC";
        cmd.Parameters.AddWithValue("@search", searchText);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            bookings.Add(MapBooking(reader));
        }
        return bookings;
    }

    public Booking? FindById(int id)
    {
        using var conn = DbHelper.GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT b.Id, b.GuestId, g.FullName, b.RoomId, r.Number, 
                   b.CheckInDate, b.CheckOutDate, b.Status, b.TotalAmount
            FROM Bookings b
            JOIN Guests g ON g.Id = b.GuestId
            JOIN Rooms r ON r.Id = b.RoomId
            WHERE b.Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapBooking(reader) : null;
    }

    public void Insert(Booking booking)
    {
        using var conn = DbHelper.GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Bookings (GuestId, RoomId, CheckInDate, CheckOutDate, Status, TotalAmount)
            VALUES (@guestId, @roomId, @checkIn, @checkOut, @status, @total)";
        SetParams(cmd, booking);
        cmd.ExecuteNonQuery();
    }

    public void Update(Booking booking)
    {
        using var conn = DbHelper.GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE Bookings 
            SET GuestId = @guestId, RoomId = @roomId, CheckInDate = @checkIn, 
                CheckOutDate = @checkOut, Status = @status, TotalAmount = @total
            WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", booking.Id);
        SetParams(cmd, booking);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var conn = DbHelper.GetConnection();
        conn.Open();
        var checkCmd = conn.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(*) FROM Payments WHERE BookingId = @id";
        checkCmd.Parameters.AddWithValue("@id", id);
        int count = Convert.ToInt32(checkCmd.ExecuteScalar());
        if (count > 0)
            throw new InvalidOperationException("Невозможно удалить бронирование: по нему уже проведены оплаты.");

        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Bookings WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    private void SetParams(SqliteCommand cmd, Booking b)
    {
        cmd.Parameters.AddWithValue("@guestId", b.GuestId);
        cmd.Parameters.AddWithValue("@roomId", b.RoomId);
        cmd.Parameters.AddWithValue("@checkIn", b.CheckInDate.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@checkOut", b.CheckOutDate.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@status", b.Status);
        cmd.Parameters.AddWithValue("@total", b.TotalAmount);
    }

    public List<Booking> FetchRecent(int limit = 5)
    {
        var recent = new List<Booking>();
        using var conn = DbHelper.GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT b.Id, b.GuestId, g.FullName, b.RoomId, r.Number, 
                   b.CheckInDate, b.CheckOutDate, b.Status, b.TotalAmount
            FROM Bookings b
            JOIN Guests g ON g.Id = b.GuestId
            JOIN Rooms r ON r.Id = b.RoomId
            ORDER BY b.CheckInDate DESC
            LIMIT @limit";
        cmd.Parameters.AddWithValue("@limit", limit);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            recent.Add(MapBooking(reader));
        }
        return recent;
    }

    public int CountTodayArrivals()
    {
        var today = DateTime.Now.ToString("yyyy-MM-dd");
        using var conn = DbHelper.GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Bookings WHERE CheckInDate = @today";
        cmd.Parameters.AddWithValue("@today", today);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public int CountTodayDepartures()
    {
        var today = DateTime.Now.ToString("yyyy-MM-dd");
        using var conn = DbHelper.GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Bookings WHERE CheckOutDate = @today";
        cmd.Parameters.AddWithValue("@today", today);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    private Booking MapBooking(SqliteDataReader reader)
    {
        return new Booking
        {
            Id = reader.GetInt32(0),
            GuestId = reader.GetInt32(1),
            GuestName = reader.GetString(2),
            RoomId = reader.GetInt32(3),
            RoomNumber = reader.GetString(4),
            CheckInDate = reader.GetDateTime(5),
            CheckOutDate = reader.GetDateTime(6),
            Status = reader.GetString(7),
            TotalAmount = reader.GetDecimal(8)
        };
    }
}