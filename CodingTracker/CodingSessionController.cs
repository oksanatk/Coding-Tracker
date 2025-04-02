using System.Globalization;

namespace CodingTracker;

internal class CodingSessionController
{
    internal readonly DatabaseManager _databaseManager;
    internal readonly UserInterface _userInterface;
    List<CodingSession> sessions = new();
    CodingSession? currentLiveSession;
    CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");

    internal CodingSessionController(UserInterface userInterface)
    {
        _userInterface = userInterface;
        _databaseManager = new DatabaseManager();
    }

    internal void WriteSessionToDatabase(DateTime startTime, DateTime endTime)
    {
        TimeSpan duration = endTime - startTime;
        sessions.Add(new CodingSession(startTime, endTime, duration));
        string formattedStart = startTime.ToString("M/d/yyyy h:mm:ss tt", culture);
        string formattedEnd = endTime.ToString("M/d/yyyy h:mm:ss tt", culture);

        _databaseManager.InsertRecord(formattedStart, formattedEnd, duration.ToString());
    }

    internal void StartNewLiveSession(DateTime startTime)
    {
        currentLiveSession = new CodingSession(startTime);
        currentLiveSession.TimerElapsed += elapsedTime =>
            {
                _userInterface.DisplayTimer(currentLiveSession.Duration, currentLiveSession.StartTime);
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

        _userInterface.DisplayMessage($"You started a new coding session at {currentLiveSession.StartTime}. \n", clearConsole: true);
        _userInterface.DisplayTimer(currentLiveSession.Duration, currentLiveSession.StartTime);

        currentLiveSession.StopTimer();
        _userInterface.DisplayMessage($"Session stopped.\nSession Start Time: [bold yellow]{currentLiveSession.StartTime}[/] \nSession End Time: [bold yellow]{currentLiveSession.EndTime}[/] \nTotal Session Coding Time: [bold yellow]{currentLiveSession.Duration}[/]\n\n");

        WriteSessionToDatabase(currentLiveSession.StartTime, currentLiveSession.EndTime);
    }

    internal List<CodingSession> ReadAllPastSessions()
    {
        List<CodingSession> readFromDatabase = _databaseManager.ReadAllPastSessions();
        this.sessions = readFromDatabase;
        return readFromDatabase;
    }

    internal List<CodingSession> FilterSortPastRecordsToBeViewed(out TimeSpan[] totalAverageTimes, string periodUnit = "", int numberOfPeriodUnits = 1, string sortType = "no")
    {
        List<CodingSession> filteredSessions = ReadAllPastSessions();

        if (!String.IsNullOrEmpty(periodUnit))
        {
            DateTime oldestToShow = CalculateOldestDateTime(periodUnit, numberOfPeriodUnits);
            filteredSessions = filteredSessions.Where(session => session.StartTime > oldestToShow).ToList();
        }

        totalAverageTimes = CalculateSessionTimeAverageTotal(filteredSessions);

        if (!String.IsNullOrEmpty(sortType) || sortType != "no")
        {
            switch (sortType)
            {
                case "newest":
                    filteredSessions.Sort((x, y) => DateTime.Compare(y.StartTime, x.StartTime));
                    break;

                case "oldest":
                    filteredSessions.Sort((x, y) => DateTime.Compare(x.StartTime, y.StartTime));
                    break;

                case "shortest":
                    filteredSessions.Sort((x, y) => TimeSpan.Compare(x.Duration, y.Duration));
                    break;

                case "longest":
                    filteredSessions.Sort((x, y) => TimeSpan.Compare(y.Duration, x.Duration));
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
            total += session.Duration;
        }
        if (filteredSessions.Count > 0)
        {
            average = total / filteredSessions.Count;
        }
        else
        {
            average = TimeSpan.Zero;
        }

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

    internal TimeSpan[] CalculateHoursUntilGoal(int goalInHours, int daysLeft)
    {
        sessions = this.ReadAllPastSessions();
        TimeSpan[] currentTotals = CalculateSessionTimeAverageTotal(sessions);
        TimeSpan currentTotal = currentTotals[0];

        TimeSpan goal = new TimeSpan(goalInHours, 0, 0);
        TimeSpan hoursLeft = goal - currentTotal;
        TimeSpan averagePerDay = hoursLeft / daysLeft;

        return new TimeSpan[] { hoursLeft, averagePerDay };
    }

    internal void UpdateSession(int id, DateTime startTime, DateTime endTime)
    {
        TimeSpan duration = endTime - startTime;
        string formattedStart = startTime.ToString("M/d/yyyy h:mm:ss tt", culture);
        string formattedEnd = endTime.ToString("M/d/yyyy h:mm:ss tt", culture);

        _databaseManager.UpdateRecord(id, formattedStart, formattedEnd, duration.ToString());
    }

    internal void DeleteSession(int id)
    {
        _databaseManager.DeleteRecord(id);
    }
}

