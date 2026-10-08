using Microsoft.Data.Sqlite;
using Hotel.Services;
using Hotel.Storage; 

namespace Hotel.Storage;

public static class SchemaSeeder
{
    public static void Initialize()
    {
        using var connection = DbHelper.GetConnection();
        connection.Open();

        CreateAuthTables(connection);
        CreateCoreTables(connection);
        SeedRoles(connection);
        SeedAdminUser(connection);
        SeedSampleData(connection);
    }

    private static void CreateAuthTables(SqliteConnection conn)
    {
        var sql = @"
            CREATE TABLE IF NOT EXISTS Roles (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL UNIQUE
            );
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Login TEXT NOT NULL UNIQUE,
                PasswordHash TEXT NOT NULL,
                PasswordSalt TEXT NOT NULL,
                FullName TEXT NOT NULL,
                RoleId INTEGER NOT NULL,
                IsActive INTEGER NOT NULL DEFAULT 1,
                CreatedAt TEXT NOT NULL,
                FOREIGN KEY (RoleId) REFERENCES Roles(Id)
            );
            CREATE TABLE IF NOT EXISTS LoginAttempts (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserLogin TEXT NOT NULL,
                IsSuccess INTEGER NOT NULL,
                Message TEXT NOT NULL,
                CreatedAt TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS ActionLogs (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserLogin TEXT NOT NULL,
                ActionType TEXT NOT NULL,
                EntityName TEXT NOT NULL,
                EntityId TEXT NOT NULL,
                Details TEXT NOT NULL,
                CreatedAt TEXT NOT NULL
            );";
        using var cmd = new SqliteCommand(sql, conn);
        cmd.ExecuteNonQuery();
    }

    private static void CreateCoreTables(SqliteConnection conn)
    {
        var sql = @"
            CREATE TABLE IF NOT EXISTS Rooms (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Number TEXT NOT NULL UNIQUE,
                Type TEXT NOT NULL,
                Capacity INTEGER NOT NULL,
                PricePerNight REAL NOT NULL,
                Status TEXT NOT NULL DEFAULT 'Свободен'
            );
            CREATE TABLE IF NOT EXISTS Guests (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                FullName TEXT NOT NULL,
                Passport TEXT NOT NULL UNIQUE,
                Phone TEXT NOT NULL,
                Email TEXT NOT NULL,
                IsActive INTEGER NOT NULL DEFAULT 1
            );
            CREATE TABLE IF NOT EXISTS Bookings (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                GuestId INTEGER NOT NULL,
                RoomId INTEGER NOT NULL,
                CheckInDate TEXT NOT NULL,
                CheckOutDate TEXT NOT NULL,
                Status TEXT NOT NULL DEFAULT 'Активно',
                TotalAmount REAL NOT NULL,
                FOREIGN KEY (GuestId) REFERENCES Guests(Id),
                FOREIGN KEY (RoomId) REFERENCES Rooms(Id)
            );
            CREATE TABLE IF NOT EXISTS Services (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Price REAL NOT NULL,
                Category TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS Payments (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                BookingId INTEGER NOT NULL,
                Amount REAL NOT NULL,
                PaymentDate TEXT NOT NULL,
                Method TEXT NOT NULL,
                FOREIGN KEY (BookingId) REFERENCES Bookings(Id)
            );";
        using var cmd = new SqliteCommand(sql, conn);
        cmd.ExecuteNonQuery();
    }

    private static void SeedRoles(SqliteConnection conn)
    {
        var sql = "INSERT OR IGNORE INTO Roles (Id, Name) VALUES (1, 'admin'), (2, 'operator'), (3, 'user');";
        using var cmd = new SqliteCommand(sql, conn);
        cmd.ExecuteNonQuery();
    }

    private static void SeedAdminUser(SqliteConnection conn)
    {
        var salt = SecurityHelper.GenerateSalt();
        var hash = SecurityHelper.HashPassword("admin123", salt);
        var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var sql = $@"
            INSERT OR IGNORE INTO Users (Login, PasswordHash, PasswordSalt, FullName, RoleId, IsActive, CreatedAt)
            VALUES ('admin', '{hash}', '{salt}', 'Администратор', 1, 1, '{now}');
            INSERT OR IGNORE INTO Users (Login, PasswordHash, PasswordSalt, FullName, RoleId, IsActive, CreatedAt)
            VALUES ('operator', '{SecurityHelper.HashPassword("oper123", SecurityHelper.GenerateSalt())}', '{SecurityHelper.GenerateSalt()}', 'Оператор', 2, 1, '{now}');
            INSERT OR IGNORE INTO Users (Login, PasswordHash, PasswordSalt, FullName, RoleId, IsActive, CreatedAt)
            VALUES ('user', '{SecurityHelper.HashPassword("user123", SecurityHelper.GenerateSalt())}', '{SecurityHelper.GenerateSalt()}', 'Читатель', 3, 1, '{now}');
        ";
        using var cmd = new SqliteCommand(sql, conn);
        cmd.ExecuteNonQuery();
    }

    private static void SeedSampleData(SqliteConnection conn)
    {
        ExecuteNonQuery(conn, "INSERT OR IGNORE INTO Rooms (Id, Number, Type, Capacity, PricePerNight, Status) VALUES (1, '101', 'Стандарт', 2, 2500, 'Свободен');");
        ExecuteNonQuery(conn, "INSERT OR IGNORE INTO Rooms (Id, Number, Type, Capacity, PricePerNight, Status) VALUES (2, '102', 'Стандарт', 2, 2500, 'Занят');");
        ExecuteNonQuery(conn, "INSERT OR IGNORE INTO Rooms (Id, Number, Type, Capacity, PricePerNight, Status) VALUES (3, '201', 'Люкс', 4, 5000, 'Свободен');");
        ExecuteNonQuery(conn, "INSERT OR IGNORE INTO Guests (Id, FullName, Passport, Phone, Email, IsActive) VALUES (1, 'Иванов Иван', '1234 567890', '+71234567890', 'ivan@mail.ru', 1);");
        ExecuteNonQuery(conn, "INSERT OR IGNORE INTO Guests (Id, FullName, Passport, Phone, Email, IsActive) VALUES (2, 'Петрова Мария', '9876 543210', '+79876543210', 'maria@mail.ru', 1);");
        ExecuteNonQuery(conn, "INSERT OR IGNORE INTO Bookings (Id, GuestId, RoomId, CheckInDate, CheckOutDate, Status, TotalAmount) VALUES (1, 1, 2, '2025-06-01', '2025-06-03', 'Активно', 5000);");
        ExecuteNonQuery(conn, "INSERT OR IGNORE INTO Services (Id, Name, Price, Category) VALUES (1, 'Завтрак', 500, 'Питание');");
        ExecuteNonQuery(conn, "INSERT OR IGNORE INTO Services (Id, Name, Price, Category) VALUES (2, 'Фитнес', 300, 'Спорт');");
        ExecuteNonQuery(conn, "INSERT OR IGNORE INTO Services (Id, Name, Price, Category) VALUES (3, 'Парковка', 200, 'Услуги');");
        ExecuteNonQuery(conn, "INSERT OR IGNORE INTO Payments (Id, BookingId, Amount, PaymentDate, Method) VALUES (1, 1, 5000, '2025-06-01 10:00', 'Карта');");
    }

    private static void ExecuteNonQuery(SqliteConnection conn, string sql)
    {
        using var cmd = new SqliteCommand(sql, conn);
        cmd.ExecuteNonQuery();
    }
}