# Coding Tracker
Console based CRUD application to track time spent coding. Developed using C#, SQLite, and Azure's Cogntitive Speech Recognizer service.

## To Use Voice-Input Mode:
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
 - an SQLite Database connection to persist the data
 - 
 - an auto-updating timer for starting a new coding session now in real-time. It auto-updates every half-second to show how much time has passed since the start of the session.
 - voice input mode with voice recognition using Azure's Speech Recognition services
 - 


# Challenges:
 - First time I've really had to do OOP on bigger project with multiple files. This was a struggle in changing my mindset, and involved me asking ChatGPT lots of questions about whether my code followed good conventions or not.
 - 
# Lessons Learned / Wins:
- Clean object-oriented code doesn't seem as scary now that I've gotten some practice with events, event handlers, and having methods return variables to be used in other methods.
- 


# Areas to Improve (Focus on For Next Time):
- I wanted to add a simple GUI using .NET MAUI. However, this proved to be an extra layer of complexity that I wasn't ready for. I have not yet made anything with 
.NET MAUI, and it would've been too much on top of the OOP organization concepts. Great to-do for next time, though!



# Resources Used:
- ChatGPT
- The [C# Academy's Project Requirements](https://www.thecsharpacademy.com/project/13/coding-tracker)
- Microsoft's [How to Make a Configuration File tutorial](https://learn.microsoft.com/en-us/troubleshoot/developer/visualstudio/csharp/language-compilers/store-custom-information-config-file)
-  
