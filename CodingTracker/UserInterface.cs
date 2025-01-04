using Microsoft.CognitiveServices.Speech;
using Spectre.Console;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace TSCA.CodingTracker;
internal class UserInterface
{
    private readonly CodingSessionController _codingSessionController;
    private static string? speechKey = Environment.GetEnvironmentVariable("Azure_SpeechSDK_Key");
    private static string? speechRegion = Environment.GetEnvironmentVariable("Azure_SpeechRegion_Key");

    internal UserInterface(bool voiceMode)
    {
        _codingSessionController = new CodingSessionController();
        ShowMainMenu(voiceMode);
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
                    ViewEditPastSessions(voiceMode);
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

    internal void CreateNewSessionMenu(bool voiceMode)
    {
        string userMenuChoice = "";
        bool exitToMainMenu = false;

        AnsiConsole.Clear();
        while (!exitToMainMenu)
        {
            AnsiConsole.Write(CreateNewSessionPanel());
            userMenuChoice = GetUserInput(voiceMode);

            switch (userMenuChoice)
            {
                case "1":
                case "one":

                    // multiple methods here: new coding session now in controller lauchnes new stopwatch on screen
                        // 1. accept input: press any key to start, then any key again to stop. After stopping, coding session will finalize for that length of time.
                        // 2. while session, keep printing, deleting, and re-printing the current session time.

                        // System.Diagnositics.Stopwatch stopwatch = new Stopwatch.StartNew()
                    break;

                case "2":
                case "two":
                    ManuallyInputNewSessionDetails(voiceMode);
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

    internal void StartSessionNow(bool voiceMode)
    {
        DateTime startTime = DateTime.Now;
        Stopwatch stopwatch = Stopwatch.StartNew();

        // some logic to continuously display the stopwatch here
        do
        {
            AnsiConsole.MarkupLine(stopwatch.Elapsed.ToString());
        } while (GetUserInput(voiceMode) != null);

        DateTime endTime = DateTime.Now;
    }

    internal void ViewEditPastSessions(bool voiceMode)
    {
        string userMenuChoice = "";
        bool exitToMainMenu = false;

        while (!exitToMainMenu)
        {
            AnsiConsole.Write(ShowPastSessionsPanel());
            userMenuChoice = GetUserInput(voiceMode);

            switch (userMenuChoice)
            {
                case "1":
                case "one":
                    // ViewPastSessionsAndStats()  --> split method into print past sessions and controller CalculateStats
                    break;
                case "2":
                case "two":
                    // UpdatePastSessionRecord() 
                    break;
                case "3":
                case "three":
                    // DeletePastSessionRecord() --> Split into mutliple methods: PrintPastSessions: ValidateIntInput(GetUserInput()) : then send to 
                    break;
                case "4":
                case "four":
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

    internal void ManuallyInputNewSessionDetails(bool voiceMode)
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
        AnsiConsole.MarkupLine("You are choosing to manually input the start and end times of the coding session.\n");

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

        _codingSessionController.CreateSession(startTime, endTime);
    }

    internal Panel CreateNewSessionPanel()
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

    internal Panel ShowPastSessionsPanel()
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

    internal string GetUserInput(bool voiceMode)
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
