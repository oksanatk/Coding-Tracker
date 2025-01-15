using Microsoft.CognitiveServices.Speech;
using Spectre.Console;
using System.Text.RegularExpressions;

namespace TSCA.CodingTracker;
internal class UserInterface
{
    private readonly CodingSessionController _codingSessionController;
    private static string? speechKey = Environment.GetEnvironmentVariable("Azure_SpeechSDK_Key");
    private static string? speechRegion = Environment.GetEnvironmentVariable("Azure_SpeechRegion_Key");

    internal UserInterface()
    {
        _codingSessionController = new CodingSessionController(this);
    }

    internal void ShowMainMenu(bool voiceMode) // TODO if not gui mode, then spectre.console. if gui mode, then maui
    {
        bool endApp = false;
        string userMainMenuOption = "";

        while (!endApp)
        {
            Panel mainMenuPanel = MainMenuPanel();

            AnsiConsole.Clear();
            AnsiConsole.Write(mainMenuPanel);

            userMainMenuOption = GetUserInput(voiceMode);
            switch (userMainMenuOption)
            {
                case "1":
                case "one":
                    CreateNewSessionMenu(voiceMode);
                    break;
                case "2": case "two":
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
            userMenuChoice = GetUserInput(voiceMode);

            switch (userMenuChoice)
            {
                case "1":
                case "one":
                    StartNewSessionNow(voiceMode);
                    break;

                case "2":
                case "two":
                    ManuallyInputSessionDetailsPrompt(voiceMode);
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
            stopString = GetUserInput(voiceMode);
        }).Start();

        while(timerIsRunning)
        {
            if (stopString.StartsWith("s"))
            {
                _codingSessionController.StopCurrentLiveSession();     
                timerIsRunning = false;
            }
        }
    }

    internal void DisplayMessage(string message, bool clearConsole=false)
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
        AnsiConsole.MarkupLine($"Time elapsed since you began this coding session: {elapsedTime:hh\\:mm\\:ss}");
        AnsiConsole.MarkupLine("Enter [bold aqua]stop[/] to stop the session.");
    }

    internal void CreateNewCustomSession(bool voiceMode)
    {
        DateTime[] startEndTime = ManuallyInputSessionDetailsPrompt(voiceMode);
        _codingSessionController.WriteSessionToDatabase(startEndTime[0],startEndTime[1]);
    }

    internal DateTime[] ManuallyInputSessionDetailsPrompt(bool voiceMode, bool updateSession=false)
    {
        string errorMessage = "";
        int userInputtedDateOrTime = -1;
        List<String> dateTimePieces = new();

        string dateTimeFormat = "yyyy-MM-dd-HH-mm";
        DateTime startTime = new();
        DateTime endTime = new();

        string[] sessionInputPrompts = new string[]
        {
            "\nPlease enter the [bold yellow]year[/] (yyyy) of the {0} time.",
            "\nPlease enter the [bold yellow]month[/] (MM) of the {0} time.",
            "\nPlease enter the [bold yellow]day[/] (dd) of the {0} time.",
            "\nPlease enter the [bold yellow]hour[/] (HH) of the {0} time.",
            "\nPlease enter the [bold yellow]minute[/] (mm) of the {0} time.",
        };

        string[] dateTimeUnits = new string[] { "year", "month", "day", "hour", "minute" };

        AnsiConsole.Clear();
        AnsiConsole.MarkupLine(updateSession ? "Please manually input the new start and end times you would like to change the entry to.\n" : "You are choosing to manually input the start and end times of the coding session.\n");

        for(int i=0; i<(sessionInputPrompts.Length * 2); i++)
        {
            do
            {
                AnsiConsole.MarkupLine(sessionInputPrompts[i % sessionInputPrompts.Length], i < sessionInputPrompts.Length ? "start" : "end");

                string currentDateTimeUnit = dateTimeUnits[i % 5];
                userInputtedDateOrTime = Validation.ValidateUserIntInput(GetUserInput(voiceMode), out errorMessage, typeOfDateUnit:currentDateTimeUnit);

                if (!String.IsNullOrEmpty(errorMessage))
                {
                    AnsiConsole.MarkupLine(errorMessage);
                } else
                {
                    if (currentDateTimeUnit != "year" && userInputtedDateOrTime < 10)
                    {
                        dateTimePieces.Add("0" + userInputtedDateOrTime.ToString());
                    } else
                    {
                        dateTimePieces.Add(userInputtedDateOrTime.ToString());
                    }
                }
            } while (!String.IsNullOrEmpty(errorMessage));

            if (i == sessionInputPrompts.Length-1) 
            {
                string maybeDate = String.Join('-',dateTimePieces);
                if (DateTime.TryParseExact(maybeDate, dateTimeFormat, null, System.Globalization.DateTimeStyles.None, out startTime))
                {
                    dateTimePieces.Clear();
                } else
                {
                    AnsiConsole.MarkupLine("That date was invalid for some reason. Please try again.");
                    dateTimePieces.Clear();
                    i = -1;
                }
            } else if (i == sessionInputPrompts.Length * 2 - 1)
            {
                string maybeDate = String.Join('-', dateTimePieces);
                if (DateTime.TryParseExact(maybeDate, dateTimeFormat, null, System.Globalization.DateTimeStyles.None, out endTime))
                {
                    dateTimePieces.Clear();
                }
                else
                {
                    AnsiConsole.MarkupLine("That date was invalid for some reason. Please try again.");
                    // TODO - possible infinitely stuck loop here. Create escape back to main menu if needed
                    dateTimePieces.Clear();
                    i = sessionInputPrompts.Length-1;
                }

                if (DateTime.Compare(startTime,endTime) >= 0)
                {
                    AnsiConsole.MarkupLine("For some reason, [yellow]start time was the same as or [bold yellow]after[/] your end time[/]. Please input the end time again.");
                    i = sessionInputPrompts.Length-1;
                }
            }
            // TODO -- maybe confirm the start / end before writing to the database?
        }

        return new DateTime[] {startTime, endTime}; 
    }

    internal void ViewEditPastSessionsMenu(bool voiceMode)
    {
        string userMenuChoice = "";
        bool exitToMainMenu = false;

        while (!exitToMainMenu)
        {
            Console.Clear();
            AnsiConsole.Write(ShowExistingRecordsMenuPanel());
            userMenuChoice = GetUserInput(voiceMode);

            switch (userMenuChoice)
            {
                case "1": case "one":
                    ShowFilteredPastRecords(voiceMode);
                    break;

                case "2": case "two":
                    UpdatePastSessionRecord(voiceMode); 
                    break;

                case "3": case "three":
                    DeletePastSessionRecord(voiceMode);    
                    break;

                case "4": case "four":
                    // CalculateCodingGoal()
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
        grid.AddRow("[aqua]4[/] - Calculate hours needed to meet your coding goal"); // result ouputted in hours of coding per day, per week, per month, per year 
        grid.AddEmptyRow();
        grid.AddRow("OR enter [aqua]exit[/] to exit back to the main menu.");

        return new Panel(grid)
        {
            Border = BoxBorder.Square,
            Header = new PanelHeader("Past Coding Sessions")
        };
    }

    internal void FilterSortPastSessionsPrompt(bool voiceMode, out int customPeriodLength, out string periodUnit, out string sortType)
    {
        customPeriodLength = 1;
        periodUnit = "";
        sortType = "";
        string userInput = "";
        int userIntInput = -1;
        string errorMessage = "";

        string[] inputPrompts =
        {
            "Did you want to show all past records? We could alternatively filter them by time periods. [bold yellow](y/n)[/]",
            "We can filter by a custom number of one of the following: [bold yellow]days[/], [bold yellow]weeks[/], [bold yellow]months[/], or [bold yellow]years[/].",
            "How many periods did you want to view? (##) Default is 1.",
            "Did you want to sort by [bold yellow]shortest[/] first, [bold yellow]longest[/] first, [bold yellow]newest[/] first, or [bold yellow]oldest[/] first? You can enter [bold yellow]no[/] if you would like to see them in order of id."
        };

        string[] timePeriodUnits =
        {
            "days","weeks","months","years"
        };

        string[] sortByUnits =
        {
            "shortest","longest", "newest", "oldest", "no"
        };

        for (int i =0;i<inputPrompts.Length;i++)
        {
            AnsiConsole.MarkupLine(inputPrompts[i]);
            userInput = GetUserInput(voiceMode);

            switch (i)
            {
                case 0:
                    if (userInput.StartsWith("y"))
                    {
                        i = 2;
                    }
                    break;

                case 1: 
                    if (timePeriodUnits.Contains(userInput))
                    {
                        periodUnit = userInput;
                    } else
                    {
                        AnsiConsole.MarkupLine("Sorry, but I didn't recognize that period of time. Please try again.");
                        i = 0;
                    }
                    break;

                case 2:
                    userIntInput = Validation.ValidateUserIntInput(userInput, out errorMessage, periodUnit);
                    if (String.IsNullOrEmpty(errorMessage))
                    {
                        customPeriodLength = 1;
                    } else
                    {
                        AnsiConsole.MarkupLine(errorMessage);
                        i = 1;
                    }
                    break;

                case 3:
                    if (sortByUnits.Contains(userInput))
                    {
                        sortType = userInput;
                    } else
                    {
                        AnsiConsole.MarkupLine("I'm sorry, but I didn't understand how you want to sort the coding session records. Please try again.");
                        i = 2;
                    }
                    break;
            }
        }
    }

    internal Panel ShowPastRecordsPanel(List<CodingSession> sessions, TimeSpan[]? totalAverageTimes=null)
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
            grid.AddRow(new string[] { session.id.ToString(), session.startTime.ToString(), session.endTime.ToString(), session.duration.ToString("hh\\:mm\\:ss") });
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
        FilterSortPastSessionsPrompt(voiceMode, out periodLengthCount, out periodLengthUnit, out sortType);

        AnsiConsole.WriteLine();
        sessions = _codingSessionController.FilterSortPastRecordsToBeViewed(out totalAverageTimes, periodLengthUnit, periodLengthCount, sortType);

        AnsiConsole.Write(ShowPastRecordsPanel(sessions, totalAverageTimes));

        AnsiConsole.MarkupLine(voiceMode ? "\nSay [bold yellow]Continue[/] to continue back to the menu." : "\nPress [bold yellow]Enter[/] to continue back to the menu.");
        GetUserInput(voiceMode);
    }

    internal void UpdatePastSessionRecord(bool voiceMode)
    {
        int userIdSelection;
        string errorMessage = "";
        bool validIdSelected = false;
        DateTime[] startEndTimes;

        do
        {
            Console.Clear();
            AnsiConsole.MarkupLine("You are choosing to update a record.Below are all of the current records.\n");
            AnsiConsole.Write(ShowPastRecordsPanel(_codingSessionController.ReadAllPastSessions()));
            AnsiConsole.MarkupLine("\nPlease enter the [bold yellow]id[/] (##) of the record you would like to update.");

            userIdSelection = Validation.ValidateUserIntInput(GetUserInput(voiceMode), out errorMessage);

            if (errorMessage != "")
            {
                AnsiConsole.MarkupLine(errorMessage);
            } else
            {
                validIdSelected = true;
            }
        } while (!validIdSelected);

        startEndTimes = ManuallyInputSessionDetailsPrompt(voiceMode, updateSession: true);

        AnsiConsole.MarkupLine($"You have selected to update the record with the [bold yellow]id: {userIdSelection}[/] to have the start time: [bold yellow]{startEndTimes[0].ToString()}[/] and end time: [bold yellow]{startEndTimes[1].ToString()}[/] ");
        _codingSessionController.UpdateSession(userIdSelection, startEndTimes[0], startEndTimes[1]);

        AnsiConsole.MarkupLine(voiceMode ? "\nSay [bold yellow]Continue[/] to continue back to the menu." : "\nPress [bold yellow]Enter[/] to continue back to the menu.");
        GetUserInput(voiceMode);
    }

    internal void DeletePastSessionRecord(bool voiceMode)
    {
        List<CodingSession> allSessions = _codingSessionController.ReadAllPastSessions();
        bool validIdSelected = false;
        string errorMessage = "";
        int userIdSelection;

        do
        {
            Console.Clear();
            AnsiConsole.MarkupLine("You've selected to delete a record. Below are all the records of the past coding sessions.\n");
            AnsiConsole.Write(ShowPastRecordsPanel(allSessions));
            AnsiConsole.MarkupLine("Please enter the [bold yellow]id[/] (##) of the record you would like to delete.");
            
            userIdSelection = Validation.ValidateUserIntInput(GetUserInput(voiceMode), out errorMessage);
            if (errorMessage != "")
            {
                AnsiConsole.MarkupLine(errorMessage);
            }else
            {
                AnsiConsole.MarkupLine($"\nOkay, we're deleting the record with the id: [bold yellow]{userIdSelection}[/] from the database.");
                validIdSelected = true;
            }

            AnsiConsole.MarkupLine(voiceMode ? "Say anything to continue." : "Press the [yellow]Enter[/] key to continue.");
            GetUserInput(voiceMode);

        } while (!validIdSelected);

        _codingSessionController.DeleteSession(userIdSelection);
    }

    internal String GetUserInput(bool voiceMode)
    {
        string? readResult;
        string userInput = "";

        if (voiceMode)
        {
            userInput = GetVoiceInput().Result;
        }
        else
        {
            readResult = Console.ReadLine();
            if (readResult != null)
            {
                userInput = readResult.Trim().ToLower();
            }
        }
        return userInput;
    }

    internal async Task<String> GetVoiceInput()
    {
        int repeatCounter = 0;
        RecognitionResult result;
        SpeechConfig speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
        speechConfig.SpeechRecognitionLanguage = "en-US";

        using SpeechRecognizer recognizer = new SpeechRecognizer(speechConfig);

        do
        {
            result = await recognizer.RecognizeOnceAsync();

            if (result.Reason == ResultReason.RecognizedSpeech)
            {
                string userVoiceInput = result.Text;
                AnsiConsole.MarkupLine($"[bold yellow]Speech Input Recognized:[/] {userVoiceInput}");

                userVoiceInput = userVoiceInput.Trim().ToLower();
                userVoiceInput = Regex.Replace(userVoiceInput, @"[^a-z0-9\s]", "");

                return userVoiceInput;
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                AnsiConsole.MarkupLine($"An error occured during speech recognition: \n\t{CancellationDetails.FromResult(result)}");
            }
            else
            {
                if (repeatCounter < 1)
                {
                    AnsiConsole.MarkupLine("I'm sorry, but I didn't understand what you said. Please try again.");
                }
                repeatCounter++;
            }
        } while (result.Reason != ResultReason.RecognizedSpeech);

        return "UnexpectedVoiceResult Error";
    }
}
