using Microsoft.CognitiveServices.Speech;
using Spectre.Console;
using System.Text.RegularExpressions;

namespace CodingTracker;

internal class UserInput
{
    private static string? speechKey = Environment.GetEnvironmentVariable("Azure_SpeechSDK_Key");
    private static string? speechRegion = Environment.GetEnvironmentVariable("Azure_SpeechSDK_Region");

    internal static void FilterSortPastSessionsPrompt(bool voiceMode, out int customPeriodLength, out string periodUnit, out string sortType)
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

        for (int i = 0; i < inputPrompts.Length; i++) //iterate through and switch on prompts
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
                    }
                    else
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
                    }
                    else
                    {
                        AnsiConsole.MarkupLine(errorMessage);
                        i = 1;
                    }
                    break;

                case 3:
                    if (sortByUnits.Contains(userInput))
                    {
                        sortType = userInput;
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("I'm sorry, but I didn't understand how you want to sort the coding session records. Please try again.");
                        i = 2;
                    }
                    break;
            }
        }
    }

    internal static int[] GoalCalculationPrompt(bool voiceMode)
    {
        bool validTimeSelected;
        string errorMessage = "";
        int codingHoursGoal = -1;
        int daysToCodeGoal = -1;

        string[] prompts = new string[]
        {
             "Please enter your coding goal, expressed in [yellow]hours[/] of time coding. (##)",
             "Please enter how many [yellow]days[/] (##) you have left to meet this goal."
        };

        Console.Clear();

        for (int i = 0; i < prompts.Length; i++)
        {
            validTimeSelected = false;
            errorMessage = "";

            while (!validTimeSelected)
            {
                AnsiConsole.MarkupLine(prompts[i]);

                if (i == 0)
                {
                    codingHoursGoal = Validation.ValidateUserIntInput(UserInput.GetUserInput(voiceMode), out errorMessage);
                }
                else
                {
                    daysToCodeGoal = Validation.ValidateUserIntInput(UserInput.GetUserInput(voiceMode), out errorMessage);
                }
                validTimeSelected = errorMessage == "" ? true : false;

                if (errorMessage != "")
                {
                    AnsiConsole.MarkupLine(errorMessage);
                    AnsiConsole.MarkupLine(voiceMode ? "\nSay anything to continue." : "Press the [bold yellow]Enter[/] key to continue.");
                    GetUserInput(voiceMode);
                }
            }
        }
        return new int[] { codingHoursGoal, daysToCodeGoal };
    }

    internal static DateTime[] ManuallyInputSessionDetailsPrompt(bool voiceMode, bool updateSession = false)
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

        for (int i = 0; i < (sessionInputPrompts.Length * 2); i++)
        {
            do
            {
                AnsiConsole.MarkupLine(sessionInputPrompts[i % sessionInputPrompts.Length], i < sessionInputPrompts.Length ? "start" : "end");

                string currentDateTimeUnit = dateTimeUnits[i % 5];
                userInputtedDateOrTime = Validation.ValidateUserIntInput(UserInput.GetUserInput(voiceMode), out errorMessage, typeOfDateUnit: currentDateTimeUnit);

                if (!String.IsNullOrEmpty(errorMessage))
                {
                    AnsiConsole.MarkupLine(errorMessage);
                }
                else
                {
                    if (currentDateTimeUnit != "year" && userInputtedDateOrTime < 10)
                    {
                        dateTimePieces.Add("0" + userInputtedDateOrTime.ToString());
                    }
                    else
                    {
                        dateTimePieces.Add(userInputtedDateOrTime.ToString());
                    }
                }
            } while (!String.IsNullOrEmpty(errorMessage));

            if (i == sessionInputPrompts.Length - 1)
            {
                string maybeDate = String.Join('-', dateTimePieces);
                if (DateTime.TryParseExact(maybeDate, dateTimeFormat, null, System.Globalization.DateTimeStyles.None, out startTime))
                {
                    dateTimePieces.Clear();
                }
                else
                {
                    AnsiConsole.MarkupLine("That date was invalid for some reason. Please try again.");
                    dateTimePieces.Clear();
                    i = -1;
                }
            }
            else if (i == sessionInputPrompts.Length * 2 - 1)
            {
                string maybeDate = String.Join('-', dateTimePieces);
                if (DateTime.TryParseExact(maybeDate, dateTimeFormat, null, System.Globalization.DateTimeStyles.None, out endTime))
                {
                    dateTimePieces.Clear();
                }
                else
                {
                    AnsiConsole.MarkupLine("That date was invalid for some reason. Please try again.");
                    dateTimePieces.Clear();
                    i = sessionInputPrompts.Length - 1;
                }

                if (DateTime.Compare(startTime, endTime) >= 0)
                {
                    AnsiConsole.MarkupLine($"For some reason, [maroon]the start time was the same as or after your end time[/]. \n\tYour start time is: {startTime.ToString()}\nPlease input the end time again.");
                    i = sessionInputPrompts.Length - 1;
                }
            }
        }

        AnsiConsole.MarkupLine(voiceMode ? "\nSay [yellow]Continue[/] to continue." : "\nPress the [bold yellow]Enter[/] key to continue.");
        GetUserInput(voiceMode);

        return new DateTime[] { startTime, endTime };
    }

    internal static String GetUserInput(bool voiceMode)
    {
        string? readResult;
        string userInput = "";

        if (voiceMode)
        {
            try
            {
                userInput = GetVoiceInput().Result;
            }
            catch (AggregateException ex)
            {
                foreach (Exception ex2 in ex.InnerExceptions) { Console.WriteLine(ex2.InnerException); }
            }
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
                Thread.Sleep(1500);

                userVoiceInput = userVoiceInput.Trim().ToLower();
                userVoiceInput = Regex.Replace(userVoiceInput, @"[^a-z0-9\s-]", "");

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
