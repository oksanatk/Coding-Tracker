using System.Configuration;
namespace TSCA.CodingTracker;
class Program
{
    private static readonly DatabaseManager _databaseManager = new DatabaseManager();
    private static readonly UserInterface _userInterface = new UserInterface();
    public static void Main(string[] args)
    {
        bool speechRecognitionMode = false;

        if (!File.Exists("CodingHours.db"))
        {
            File.Create("CodingHours.db").Close();
        }

        if (args.Contains("--voice-input"))
        {
            speechRecognitionMode = true;
        }

        _userInterface.ShowMainMenu(speechRecognitionMode);





        string? sAttr = ConfigurationManager.AppSettings.Get("Key0");
        Console.WriteLine($"the value of key0 is: {sAttr}");

    }
}


