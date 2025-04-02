namespace CodingTracker;
class Program
{
    private static readonly UserInterface _userInterface = new UserInterface();
    public static void Main(string[] args)
    {
        bool speechRecognitionMode = false;

        if (!File.Exists(System.Configuration.ConfigurationManager.AppSettings.Get("PathToDatabase")))
        {
            File.Create(System.Configuration.ConfigurationManager.AppSettings.Get("PathToDatabase")).Close();
        }

        if (args.Contains("--voice-input"))
        {
            speechRecognitionMode = true;
        }

        _userInterface.ShowMainMenu(speechRecognitionMode);
    }
}


