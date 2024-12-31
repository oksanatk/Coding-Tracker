using Microsoft.CognitiveServices.Speech;
using Spectre.Console;
using System.Text.RegularExpressions;

namespace TSCA.CodingTracker;
internal class UserInterface
{
    private readonly CodingSessionController _codingSessionController = new();
    private readonly DatabaseManager _databaseManager = new();
    private static string? speechKey = Environment.GetEnvironmentVariable("Azure_SpeechSDK_Key");
    private static string? speechRegion = Environment.GetEnvironmentVariable("Azure_SpeechRegion_Key");

    internal UserInterface(bool voiceMode)
    {
        ShowMainMenu(voiceMode);
    }

    internal void ShowMainMenu(bool voiceMode) //if not gui mode, then spectre.console. if gui mode, then maui
    {
        bool endApp = false;
        string userMainMenuOption = "";

        while (!endApp)
        {
            Panel mainMenuPanel = MainMenuPanel();
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

    internal static void CreateNewSessionMenu(bool voiceMode)
    {
        string userMenuChoice = "";
        bool exitToMainMenu = false;

        while (!exitToMainMenu)
        {
            AnsiConsole.Write(CreateNewSessionPanel());
            userMenuChoice = GetUserInput(voiceMode);

            switch (userMenuChoice)
            {
                case "1":
                case "one":
                    // multiple methods here: new coding session now in controller lauchnes new stopwatch on screen
                    break;
                case "2":
                case "two":
                    // CreateNewManualCodingSession: get user input here,
                    // send to be validated in validation.cs,
                    // then send validated data to controller to create new object,
                    // add it to the current list of sessions,
                    // and then send formatted and parsed DateTime string to the DatabaseManager to actually write to the sqlite database
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

    internal static void ViewEditPastSessions(bool voiceMode)
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

    internal static Panel MainMenuPanel()
    {
        Table table = new Table();
        table.AddColumn("Your Options:");
        table.AddRow("[aqua]1[/] - Create new coding session");
        table.AddRow("[aqua]2[/] - View past coding sessions");
        table.AddEmptyRow();
        table.AddRow("OR enter [aqua]exit[/] to exit the app");


        return new Panel(table)
        {
            Border = BoxBorder.Square,
            Header = new PanelHeader("Welcome to the Coding Tracker!")
        };
    }

    internal static Panel CreateNewSessionPanel()
    {
        Table table = new();
        table.AddColumn("Your Options:");
        table.AddRow("[aqua]1[/] - Start a new session [yellow]now[/].");
        table.AddRow("[aqua]2[/] - Manually input the start and end times of a new record.");
        table.AddEmptyRow();
        table.AddRow("OR enter [aqua]exit[/] to exit back to the main menu.");

        return new Panel(table)
        {
            Border = BoxBorder.Square,
            Header = new PanelHeader("Create a New Coding Session")
        };
    }

    internal static Panel ShowPastSessionsPanel()
    {
        Table table = new Table();
        table.AddColumn("Your Options:");
        table.AddRow("[aqua]1[/] - View all past coding sessions and associated stats");
        table.AddRow("[aqua]2[/] - Update a past coding session");
        table.AddRow("[aqua]3[/] - Delete a past coding session");
        table.AddRow("[aqua]4[/] - Calculate hours needed to meet your coding goal"); // result ouputted in hours of coding per day, per week, per month, per year 
        table.AddEmptyRow();
        table.AddRow("OR enter [aqua]exit[/] to exit back to the main menu.");

        return new Panel(table)
        {
            Border = BoxBorder.Square,
            Header = new PanelHeader("Past Coding Sessions")
        };
    }

    internal static string GetUserInput(bool voiceMode)
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

    internal static async Task<String> GetVoiceInput()
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
