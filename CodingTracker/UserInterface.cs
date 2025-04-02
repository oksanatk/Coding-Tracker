using Microsoft.CognitiveServices.Speech;
using Spectre.Console;
using System.Text.RegularExpressions;

namespace CodingTracker;

internal class UserInterface
{
    private readonly CodingSessionController _codingSessionController;
    private static string? speechKey = Environment.GetEnvironmentVariable("Azure_SpeechSDK_Key");
    private static string? speechRegion = Environment.GetEnvironmentVariable("Azure_SpeechSDK_Region"); 

    internal UserInterface()
    {
        _codingSessionController = new CodingSessionController(this);
    }

    internal void ShowMainMenu(bool voiceMode)
    {
        bool endApp = false;
        string userMainMenuOption = "";

        while (!endApp)
        {
            Panel mainMenuPanel = MainMenuPanel();

            AnsiConsole.Clear();
            AnsiConsole.Write(mainMenuPanel);

            userMainMenuOption = UserInput.GetUserInput(voiceMode);
            switch (userMainMenuOption)
            {
                case "1":
                case "one":
                    CreateNewSessionMenu(voiceMode);
                    break;
                case "2":
                case "two":
                    ViewEditPastSessionsMenu(voiceMode);
                    break;
                case "exit":
                    endApp = true;
                    break;
                default:
                    AnsiConsole.MarkupLine("I'm sorry, but I didn't understand that input. Please try again.");
                    break;
            }
        }
    }

    internal Panel MainMenuPanel()
    {
        Grid grid = new();
        grid.AddColumn();
        grid.AddEmptyRow();
        grid.AddRow("[aqua]1[/] - Create new coding session");
        grid.AddRow("[aqua]2[/] - View past coding sessions");
        grid.AddEmptyRow();
        grid.AddRow("OR enter [aqua]exit[/] to exit the app");

        return new Panel(grid)
        {
            Border = BoxBorder.Square,
            Header = new PanelHeader("[bold yellow]Welcome![/] Main Menu")
        };
    }

    internal void CreateNewSessionMenu(bool voiceMode)
    {
        string userMenuChoice = "";
        bool exitToMainMenu = false;

        AnsiConsole.Clear();
        while (!exitToMainMenu)
        {
            AnsiConsole.Write(CreateNewSessionMenuPanel());
            userMenuChoice = UserInput.GetUserInput(voiceMode);

            switch (userMenuChoice)
            {
                case "1":
                case "one":
                    StartNewSessionNow(voiceMode);
                    break;

                case "2":
                case "two":
                    UserInput.ManuallyInputSessionDetailsPrompt(voiceMode);
                    break;

                case "exit":
                    exitToMainMenu = true;
                    break;

                default:
                    AnsiConsole.MarkupLine("I'm sorry, but I didn't understand that input.");
                    break;
            }
        }
    }

    internal void StartNewSessionNow(bool voiceMode)
    {
        DateTime startTime = DateTime.Now;
        bool timerIsRunning = true;
        string stopString = "";

        _codingSessionController.StartNewLiveSession(startTime);

        new Thread(() =>
        {
            Thread.CurrentThread.IsBackground = true;
            stopString = UserInput.GetUserInput(voiceMode);
        }).Start();

        while (timerIsRunning)
        {
            if (stopString.StartsWith("s"))
            {
                _codingSessionController.StopCurrentLiveSession();
                timerIsRunning = false;
            } 
        }
    }

    internal void DisplayMessage(string message, bool clearConsole = false)
    {
        if (clearConsole)
        {
            Console.Clear();
        }
        AnsiConsole.MarkupLine(message);
    }

    internal void DisplayTimer(TimeSpan elapsedTime, DateTime startTime)
    {
        Console.SetCursorPosition(0, 2);
        Console.Write(new string(' ', Console.WindowWidth * (Console.WindowHeight-3)));
        Console.SetCursorPosition(0, 2);
        AnsiConsole.MarkupLine($"Time elapsed since you began this coding session: {elapsedTime:hh\\:mm\\:ss}");
        AnsiConsole.MarkupLine("Enter [bold aqua]stop[/] to stop the session.");
    }

    internal void CreateNewCustomSession(bool voiceMode)
    {
        DateTime[] startEndTime = UserInput.ManuallyInputSessionDetailsPrompt(voiceMode);

        AnsiConsole.MarkupLine($"You're creating a new record of a coding session that: \n\tStarted on:[bold yellow]{startEndTime[0].ToString()}[/]\n\tEnded on:[bold yellow]{startEndTime[1].ToString()}[/]");
        _codingSessionController.WriteSessionToDatabase(startEndTime[0], startEndTime[1]);
    }

    internal void ViewEditPastSessionsMenu(bool voiceMode)
    {
        string userMenuChoice = "";
        bool exitToMainMenu = false;

        while (!exitToMainMenu)
        {
            Console.Clear();
            AnsiConsole.Write(ShowExistingRecordsMenuPanel());
            userMenuChoice = UserInput.GetUserInput(voiceMode);

            switch (userMenuChoice)
            {
                case "1":
                case "one":
                    ShowFilteredPastRecords(voiceMode);
                    break;

                case "2":
                case "two":
                    UpdatePastSessionRecord(voiceMode);
                    break;

                case "3":
                case "three":
                    DeletePastSessionRecord(voiceMode);
                    break;

                case "4":
                case "four":
                    DisplayTimeUntilGoal(voiceMode, UserInput.GoalCalculationPrompt(voiceMode));
                    break;

                case "exit":
                    exitToMainMenu = true;
                    break;

                default:
                    AnsiConsole.MarkupLine("I'm sorry, but I didn't understand that menu option. Please try again.");
                    break;
            }
        }
    }

    internal Panel CreateNewSessionMenuPanel()
    {
        Grid grid = new();
        grid.AddColumn();
        grid.AddEmptyRow();
        grid.AddRow("[aqua]1[/] - Start a new session [yellow]now[/].");
        grid.AddRow("[aqua]2[/] - Manually input the start and end times of a new record.");
        grid.AddEmptyRow();
        grid.AddRow("OR enter [aqua]exit[/] to exit back to the main menu.");

        return new Panel(grid)
        {
            Border = BoxBorder.Square,
            Header = new PanelHeader("Create a New Coding Session")
        };
    }

    internal Panel ShowExistingRecordsMenuPanel()
    {
        Grid grid = new();
        grid.AddColumn();
        grid.AddEmptyRow();
        grid.AddRow("[aqua]1[/] - View all past coding sessions and associated stats");
        grid.AddRow("[aqua]2[/] - Update a past coding session");
        grid.AddRow("[aqua]3[/] - Delete a past coding session");
        grid.AddRow("[aqua]4[/] - Calculate hours needed to meet your coding goal");
        grid.AddEmptyRow();
        grid.AddRow("OR enter [aqua]exit[/] to exit back to the main menu.");

        return new Panel(grid)
        {
            Border = BoxBorder.Square,
            Header = new PanelHeader("Past Coding Sessions")
        };
    }

    internal Panel ShowPastRecordsPanel(List<CodingSession> sessions, TimeSpan[]? totalAverageTimes = null)
    {
        Grid grid = new();
        grid.AddColumn();
        grid.AddColumn();
        grid.AddColumn();
        grid.AddColumn();
        grid.AddRow("[bold yellow]ID[/]", "[bold yellow]Start Time[/]", "[bold yellow]End Time[/]", "[bold yellow]Duration[/]");
        grid.AddEmptyRow();

        foreach (CodingSession session in sessions)
        {
            grid.AddRow(new string[] { session.Id.ToString(), session.StartTime.ToString(), session.EndTime.ToString(), session.Duration.ToString("hh\\:mm\\:ss") });
        }
        grid.AddEmptyRow();

        if (totalAverageTimes != null)
        {
            grid.AddRow("", "", "[yellow]Total Time Coding:[/]", totalAverageTimes[0].ToString("hh\\:mm\\:ss"));
            grid.AddRow("", "", "[yellow]Average Time Coding: [/]", totalAverageTimes[1].ToString("hh\\:mm\\:ss"));
            grid.AddEmptyRow();
        }

        return new Panel(grid)
        {
            Header = new PanelHeader("Past Sessions Recorded"),
            Border = BoxBorder.Square,
        };
    }

    internal void ShowFilteredPastRecords(bool voiceMode)
    {
        string periodLengthUnit;
        int periodLengthCount;
        string sortType;
        TimeSpan[] totalAverageTimes;
        List<CodingSession> sessions = new();

        Console.Clear();
        UserInput.FilterSortPastSessionsPrompt(voiceMode, out periodLengthCount, out periodLengthUnit, out sortType);

        AnsiConsole.WriteLine();
        sessions = _codingSessionController.FilterSortPastRecordsToBeViewed(out totalAverageTimes, periodLengthUnit, periodLengthCount, sortType);

        AnsiConsole.Write(ShowPastRecordsPanel(sessions, totalAverageTimes));

        AnsiConsole.MarkupLine(voiceMode ? "\nSay [bold yellow]Continue[/] to continue back to the menu." : "\nPress [bold yellow]Enter[/] to continue back to the menu.");
        UserInput.GetUserInput(voiceMode);
    }

    internal void UpdatePastSessionRecord(bool voiceMode)
    {
        string userMenuSelection = "";
        int userIdSelection = -1;
        string errorMessage = "";
        bool validIdSelected = false;
        DateTime[] startEndTimes;

        do
        {
            Console.Clear();
            AnsiConsole.MarkupLine("You are choosing to update a record.Below are all of the current records.\n");
            AnsiConsole.Write(ShowPastRecordsPanel(_codingSessionController.ReadAllPastSessions()));
            AnsiConsole.MarkupLine("\nPlease enter the [bold yellow]id[/] (##) of the record you would like to update. \nOR enter [bold yellow]Exit[/] to exit back to the menu.");

            userMenuSelection = UserInput.GetUserInput(voiceMode);
            if (userMenuSelection != "exit")
            {
                userIdSelection = Validation.ValidateUserIntInput(userMenuSelection, out errorMessage);
                if (errorMessage != "")
                {
                    AnsiConsole.MarkupLine(errorMessage);
                }
                else
                {
                    validIdSelected = true;
                }
            } else
            {
                validIdSelected = true;
            }

        } while (!validIdSelected);

        if (userMenuSelection != "exit")
        {
            startEndTimes = UserInput.ManuallyInputSessionDetailsPrompt(voiceMode, updateSession: true);

            AnsiConsole.MarkupLine($"You have selected to update the record with the [bold yellow]id: {userIdSelection}[/] to have the start time: [bold yellow]{startEndTimes[0].ToString()}[/] and end time: [bold yellow]{startEndTimes[1].ToString()}[/] ");
            _codingSessionController.UpdateSession(userIdSelection, startEndTimes[0], startEndTimes[1]);

            AnsiConsole.MarkupLine(voiceMode ? "\nSay [bold yellow]Continue[/] to continue back to the menu." : "\nPress [bold yellow]Enter[/] to continue back to the menu.");
            UserInput.GetUserInput(voiceMode);
        }
    }

    internal void DeletePastSessionRecord(bool voiceMode)
    {
        List<CodingSession> allSessions = _codingSessionController.ReadAllPastSessions();
        string userMenuSelection = "";
        bool validIdSelected = false;
        string errorMessage = "";
        int userIdSelection = -1;

        do
        {
            Console.Clear();
            AnsiConsole.MarkupLine("You've selected to delete a record. Below are all the records of the past coding sessions.\n");
            AnsiConsole.Write(ShowPastRecordsPanel(allSessions));
            AnsiConsole.MarkupLine("Please enter the [bold yellow]id[/] (##) of the record you would like to delete.\nOR enter [bold yellow]Exit[/] to exit back to the menu.");

            userMenuSelection = UserInput.GetUserInput(voiceMode);
            if (userMenuSelection != "exit")
            {
                userIdSelection = Validation.ValidateUserIntInput(userMenuSelection, out errorMessage);
                if (errorMessage != "")
                {
                    AnsiConsole.MarkupLine(errorMessage);
                }
                else
                {
                    AnsiConsole.MarkupLine($"\nOkay, we're deleting the record with the id: [bold yellow]{userIdSelection}[/] from the database.");
                    validIdSelected = true;
                }

                AnsiConsole.MarkupLine(voiceMode ? "Say anything to continue." : "Press the [yellow]Enter[/] key to continue.");
                UserInput.GetUserInput(voiceMode);

            } else
            {
                validIdSelected = true;
            }
        } while (!validIdSelected);

        if (userMenuSelection != "exit")
        {
            _codingSessionController.DeleteSession(userIdSelection);
        }
    }

    internal void DisplayTimeUntilGoal(bool voiceMode, int[] hoursAndDaysLeft)
    {
        int codingHoursGoal = hoursAndDaysLeft[0];
        int daysToCodeGoal = hoursAndDaysLeft[1];
        TimeSpan[] totalAverageLeft = _codingSessionController.CalculateHoursUntilGoal(codingHoursGoal, daysToCodeGoal);

        AnsiConsole.MarkupLine($"\nBased on your goal of [bold yellow]{codingHoursGoal}[/] hours over [bold yellow]{daysToCodeGoal}[/] days, we've calculated that you need: \n");

        AnsiConsole.MarkupLine($"\t[bold yellow]{Math.Floor(totalAverageLeft[0].TotalHours)} hours, {totalAverageLeft[0].Minutes} minutes, and {totalAverageLeft[0].Seconds} seconds[/] until you reach your goal!");
        AnsiConsole.MarkupLine($"\tThis means coding [bold yellow]{Math.Floor(totalAverageLeft[1].TotalHours)} hours, {totalAverageLeft[1].Minutes} minutes, and {totalAverageLeft[1].Seconds} seconds[/] every day that you have left.");

        AnsiConsole.MarkupLine(voiceMode ? "\nSay anything to continue." : "\nPress the [bold yellow]Enter[/] key to continue.");

        UserInput.GetUserInput(voiceMode);
    }
}
