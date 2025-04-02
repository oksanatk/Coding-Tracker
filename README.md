# Coding Tracker
Console based CRUD application to track time spent coding. Developed using C#, SQLite (mapped using Dapper microORM), and Azure's Cogntitive Speech Recognizer service.

### To Use Voice-Input Mode:
Run the project with the command-line argument `--voice-input`

# Requirements:
 - [x] Use Spectre.Console to spice up / add color and panels to UI
 - [x] Required to have multiple classes in different files split up by Separation Of Concerns
 - [x] Control and validate the input and format that users are allowed to enter dates in
 - [x] Create a configuration file with my database path and connection strings
 - [x] Create a separate CodingSession.cs class to store and work with the properties of coding sessions
 - [x] Option to input the start and end times manually
 - [x] Let users sort and filter their coding records (ie, per days, weeks, or years)
 - [x] Create reports where users can see their total and average coding time per period.
 - [x] Create goals where users can calculate how much coding time they would need to reach their goal, and how much they would need to average per day.

# Features:
 - An SQLite Database connection to persist the data
 - Console-based ui with color and panels for prettier formatting of data
 - An auto-updating timer for starting a new coding session now in real-time. It auto-updates every half-second to constantly show how much time has passed since the start of the session.
 - Voice input mode with voice recognition using Azure's Speech Recognition services
 - Filter records by time-period (days, weeks, months, years)
   - Sort records in period by recency or duration of the coding sessions
   - Basic report of total and average time of coding sessions
 - Calculate hours still left to reach goals, and necessary time average coding time each day to reach this goal.


# Challenges:
 - This was the first time I've really had to do OOP on bigger project with multiple files. This was a struggle in changing my mindset, and involved me asking ChatGPT **lots** of questions about whether my code followed good conventions or not.
   
 - New Project, New Syntax: previous project HabitLogger used ADO.NET to access my sqlite database with close-to raw SQL. Dapper is easier to use, but still a learning curve for the first time I've ever used it.

 - The live timer that displays how long it's been since a new session had started
   - This threw me into lots of googling trying to figure out what Events, EventHandlers, and the like really are. I'm not an expert yet, but I knew absolutely nothing before creating a timer that fired off an event every half-second, and I'm glad for the learning experience. Will rock this with more reps!

# Lessons Learned / Wins:
- Clean object-oriented code doesn't seem as scary now that I've gotten some practice with events, event handlers, and having methods return variables to be used in other methods.
  
- Per chatgpt's suggestion, I created a dictionary to connect input errors to error message strings and shorten the amount of code necessary for validation. Dictionary of errors is reusable and can be used w/ any interface
  
- I used git branching to write a couple of new features, as opposed to committing everything to the main master branch directly. This was very cool, and great practice!

# Areas to Improve (Focus on For Next Time):
- I wanted to add a simple GUI using .NET MAUI. However, this proved to be an extra layer of complexity that I wasn't ready for. I have not yet made anything with 
.NET MAUI, and it would've been too much on top of the OOP organization concepts. Adding a GUI, or potentially even creating 2 UIs, is a great to-do for next time, though!

- Maybe next time I could split the UserInterface.cs code up into 2 parts: UserInterfaceController.cs and 1 or more UserInterfaces (SpectreConsoleInterface, MAUIGuiInterface, etc) . Could even use a base class UserInterface or an IUserInferface interface to make sure functionality remains across interfaces.
  
- What is a good way to organize a project that might take multiple command-line args? Where do I store these variable (such as a potential `--voice-mode` or a `--gui`? Do I put it in the config file? I've been passing the argument `bool voiceMode` around to many methods so far.

# Resources Used:
- ChatGPT
- The [C# Academy's Project Requirements](https://www.thecsharpacademy.com/project/13/coding-tracker)
- Microsoft's [How to Make a Configuration File tutorial](https://learn.microsoft.com/en-us/troubleshoot/developer/visualstudio/csharp/language-compilers/store-custom-information-config-file)
-  [Spectre.Console documentation](https://spectreconsole.net/)
-  [Dapper microORM tutorials](https://www.learndapper.com/)
  
