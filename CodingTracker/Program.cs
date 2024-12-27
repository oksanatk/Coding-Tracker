using Spectre.Console;

namespace TSCA.CodingTracker
{
    class CodingTracker
    {
        public static void Main(string[] args)
        {
            if (!File.Exists("CodingHours.db"))
            {
                File.Create("CodingHours.db").Close();
            }




        }
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
 * 


