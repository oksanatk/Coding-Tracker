using Dapper;
using Microsoft.Data.Sqlite;
using System.Configuration;

namespace CodingTracker;
internal class DatabaseManager
{
    internal static string? connectionString = ConfigurationManager.AppSettings.Get("ConnectionString");

    internal void CreateTable()
    {
        using (SqliteConnection connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            string sqliteCommand =
                @"
                    CREATE TABLE IF NOT EXISTS coding_tracker (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        start_datetime TEXT,
                        end_datetime TEXT,
                        duration TEXT
                    );";

            connection.Execute(sqliteCommand);
        }
    }

    internal List<CodingSession> ReadAllPastSessions()
    {
        List<CodingSession> pastSessions = new();

        using (SqliteConnection connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            string sqliteCommand = "SELECT * FROM coding_tracker;";

            pastSessions = connection.Query<CodingSession>(sqliteCommand).ToList();
        }
        return pastSessions;
    }

    internal void InsertRecord(string startTime, string endTime, string duration)
    {
        using (SqliteConnection connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            string sqliteCommand =
                @"
                    INSERT INTO coding_tracker 
                    (start_datetime, end_datetime, duration)
                    VALUES 
                    (@start_datetime, @end_datetime, @duration);
                ";
            connection.Execute(sqliteCommand, new { start_datetime = startTime, end_datetime = endTime, duration = duration });
        }
    }

    internal void UpdateRecord(int idOfRecord, string startTime, string endTime, string duration)
    {
        using (SqliteConnection connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            string sqliteCommand =
                @"
                    INSERT INTO coding_tracker 
                     (id, start_datetime, end_datetime, duration)
                    VALUES  
                     (@idOfRecord, @startTime, @endTime, @duration)
                    ON CONFLICT(id) DO UPDATE SET 
                        start_datetime = excluded.start_datetime, 
                        end_datetime = excluded.end_datetime,
                        duration = excluded.duration;
                ";
            connection.Execute(sqliteCommand, new { idOfRecord = idOfRecord, startTime = startTime, endTime = endTime, duration = duration });
        }
    }

    internal void DeleteRecord(int idOfRecord)
    {
        using (SqliteConnection connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            string sqliteCommand =
                @"
                    DELETE FROM coding_tracker WHERE id=@idOfRecord;
                ";
            connection.Execute(sqliteCommand, new { idOfRecord = idOfRecord });
        }
    }
}
