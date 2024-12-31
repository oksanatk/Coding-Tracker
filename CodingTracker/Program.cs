using System.Configuration;
namespace TSCA.CodingTracker;
class Program
{

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

        UserInterface userInterface = new UserInterface(speechRecognitionMode);






        string? sAttr = ConfigurationManager.AppSettings.Get("Key0");
        Console.WriteLine($"the value of key0 is: {sAttr}");

    }
}


