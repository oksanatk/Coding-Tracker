﻿

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
        return _databaseManager.ReadAllPastSessions();
    }

    internal List<String> StatsAboutSessions()
    {
        // TODO : What to include: 
            // filter: user input: can view: past #of days / weeks / month / year
            // sort: user input : ascending or descending
            // stats: total and average hours per period selected

            


        return new List<String>();
    }

    internal void UpdateSession() { }

    internal void DeleteSession() { }
}

