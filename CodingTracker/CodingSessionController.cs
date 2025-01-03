

namespace TSCA.CodingTracker;
internal class CodingSessionController
{
    internal readonly DatabaseManager _databaseManager;
    List<CodingSession> sessions = new();
    internal CodingSessionController()
    {
        _databaseManager = new DatabaseManager();
    }

    internal void CreateSession(DateTime startTime, DateTime endTime)
    {
        TimeSpan duration = endTime - startTime;
        sessions.Add(new CodingSession(startTime, endTime, duration));

        _databaseManager.InsertRecord(startTime.ToString(), endTime.ToString(), duration.ToString());
    }

    internal void ReadSession() { } // also include a view that 

    internal void UpdateSession() { }

    internal void DeleteSession() { }
}

