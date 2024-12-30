using System.Configuration;
namespace TSCA.CodingTracker;
class CodingTracker
{
    internal readonly UserInterface _userInterface = new UserInterface();

    public static void Main(string[] args)
    {
        if (!File.Exists("CodingHours.db"))
        {
            File.Create("CodingHours.db").Close();
        }
        string? sAttr = ConfigurationManager.AppSettings.Get("Key0");

        Console.WriteLine($"the value of key0 is: {sAttr}");

        DatabaseManager databaseManager = new();

        databaseManager.CreateTable();



    }
}





// what's the layout, what's the layout???


/* hmmmm....
 * 
 * so I'd need an enum? on the options???
 * 
 * 
 * hang on... please hold.... 
 * 
 * so the user interface and options to work around.....
 * 
 * 1. Begin coding session
 *      (starts a stopwatch, and pops up option to end the coding session, too
 * 2. View report for 
 * 
 * */


