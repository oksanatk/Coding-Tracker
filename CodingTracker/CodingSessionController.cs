

namespace TSCA.CodingTracker;
internal class CodingSessionController
{
    internal readonly DatabaseManager _databaseManager;
    internal readonly UserInterface _userInterface;
    List<CodingSession> sessions = new();
    CodingSession currentLiveSession;
    internal CodingSessionController(UserInterface userInterface)
    {
        _userInterface = userInterface;
        _databaseManager = new DatabaseManager();
    }

    internal void WriteSessionToDatabase(DateTime startTime, DateTime endTime)
    {
        TimeSpan duration = endTime - startTime;
        sessions.Add(new CodingSession(startTime, endTime, duration));

        _databaseManager.InsertRecord(startTime.ToString(), endTime.ToString(), duration.ToString());
    }

    internal void StartNewLiveSession(DateTime startTime)
    {
        currentLiveSession = new CodingSession(startTime);
        currentLiveSession.TimerElapsed += elapsedTime => 
            {
            _userInterface.DisplayTimer(currentLiveSession.duration, currentLiveSession.startTime);
            };
        currentLiveSession.StartTimer();

        _userInterface.DisplayMessage($"You started a new coding session at {startTime}. \n", clearConsole: true);
    }

    internal void StopCurrentLiveSession()
    {
        if (currentLiveSession == null)
        {
            _userInterface.DisplayMessage("[bold maroon]No active session to stop.[/]");
            return;
        }

        currentLiveSession.StopTimer();
        _userInterface.DisplayMessage($"Session stopped.\nSession Start Time: [bold yellow]{currentLiveSession.startTime}[/] \nSession End Time: [bold yellow]{currentLiveSession.endTime}[/] \nTotal Session Coding Time: [bold yellow]{currentLiveSession.duration}[/]\n\n");

        WriteSessionToDatabase(currentLiveSession.startTime, currentLiveSession.endTime);
    }

    internal List<CodingSession> ReadAllPastSessions() 
    {
        List<CodingSession> readFromDatabase = _databaseManager.ReadAllPastSessions();
        this.sessions = readFromDatabase;
        return readFromDatabase;
    }

    internal List<CodingSession> FilterSortPastRecordsToBeViewed(out TimeSpan[] totalAverageTimes, string periodUnit="", int numberOfPeriodUnits=1, string sortType="no")
    {
        List<CodingSession> filteredSessions = ReadAllPastSessions();

        if (!String.IsNullOrEmpty(periodUnit))
        {
            DateTime oldestToShow = CalculateOldestDateTime(periodUnit, numberOfPeriodUnits);
            filteredSessions = filteredSessions.Where(session => session.startTime > oldestToShow).ToList();
        } 

        totalAverageTimes = CalculateSessionTimeAverageTotal(filteredSessions);
        
        if (!String.IsNullOrEmpty(sortType) || sortType != "no")
        {
            switch (sortType)
            {
                case "newest":
                    filteredSessions.Sort((x, y) => DateTime.Compare(y.startTime, x.startTime));
                    break;

                case "oldest": 
                    filteredSessions.Sort((x, y) => DateTime.Compare(x.startTime, y.startTime));
                    break;

                case "shortest":
                    filteredSessions.Sort((x, y) => TimeSpan.Compare(x.duration, y.duration));
                    break;

                case "longest":
                    filteredSessions.Sort((x, y) => TimeSpan.Compare(y.duration, x.duration));
                    break;
            }
        }

        return filteredSessions; 
    }

    private TimeSpan[] CalculateSessionTimeAverageTotal(List<CodingSession> filteredSessions)
    {
        TimeSpan total = new();
        TimeSpan average;

        foreach (CodingSession session in filteredSessions)
        {
            total += session.duration;
        }
        average = total / filteredSessions.Count;

        return new TimeSpan[] { total, average };
    }

    private DateTime CalculateOldestDateTime(string periodUnit, int numberOfPeriods)
    {
        DateTime oldestToShow = DateTime.Now;

        switch (periodUnit)
        {
            case "days":
                oldestToShow = oldestToShow.AddDays(numberOfPeriods * -1);
                break;

            case "weeks":
                oldestToShow = oldestToShow.AddDays(numberOfPeriods * -7);
                break;

            case "months":
                oldestToShow = oldestToShow.AddMonths(numberOfPeriods * -1);
                break;

            case "years":
                oldestToShow = oldestToShow.AddYears(numberOfPeriods * -1);
                break;
        }
        return oldestToShow;
    }

    internal void UpdateSession() { }

    internal void DeleteSession() { }
}

