namespace TSCA.CodingTracker;
class Program
{
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
    }
}


