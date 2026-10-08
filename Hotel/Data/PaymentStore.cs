using Microsoft.Data.Sqlite;
using Hotel.Models;
using Hotel.Storage;

namespace Hotel.Storage;

public class PaymentStore
{
    public List<Payment> FetchAll(string query = "")
    {
        var items = new List<Payment>();
        using var conn = DbHelper.GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT p.Id, p.BookingId, (b.Id || ' - ' || g.FullName) AS BookingInfo, 
                   p.Amount, p.PaymentDate, p.Method
            FROM Payments p
            JOIN Bookings b ON b.Id = p.BookingId
            JOIN Guests g ON g.Id = b.GuestId
            WHERE (@query = '') 
               OR g.FullName LIKE ('%' || @query || '%')
               OR p.Method LIKE ('%' || @query || '%')
            ORDER BY p.PaymentDate DESC";
        cmd.Parameters.AddWithValue("@query", query);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            items.Add(new Payment
            {
                Id = reader.GetInt32(0),
                BookingId = reader.GetInt32(1),
                BookingInfo = reader.GetString(2),
                Amount = reader.GetDecimal(3),
                PaymentDate = reader.GetDateTime(4),
                Method = reader.GetString(5)
            });
        }
        return items;
    }

    public void Add(Payment payment)
    {
        using var conn = DbHelper.GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Payments (BookingId, Amount, PaymentDate, Method)
            VALUES (@bookingId, @amount, @date, @method)";
        cmd.Parameters.AddWithValue("@bookingId", payment.BookingId);
        cmd.Parameters.AddWithValue("@amount", payment.Amount);
        cmd.Parameters.AddWithValue("@date", payment.PaymentDate.ToString("yyyy-MM-dd HH:mm:ss"));
        cmd.Parameters.AddWithValue("@method", payment.Method);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var conn = DbHelper.GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Payments WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public List<Booking> FetchActiveBookings()
    {
        var active = new List<Booking>();
        using var conn = DbHelper.GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT b.Id, g.FullName
            FROM Bookings b
            JOIN Guests g ON g.Id = b.GuestId
            WHERE b.Status = 'Активно'
            ORDER BY b.CheckInDate";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            active.Add(new Booking { Id = reader.GetInt32(0), GuestName = reader.GetString(1) });
        }
        return active;
    }
}