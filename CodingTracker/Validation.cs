namespace TSCA.CodingTracker;

internal static class Validation
{
    internal static string ParseDate(DateTime date)
    {
        return "";
    }

    internal static int ValidateUserIntInput(string maybeNumber, out string errorMessage, string typeOfDateUnit ="")
    {
        int realNumberValue = -1;
        errorMessage = "";
        typeOfDateUnit = typeOfDateUnit.Trim().ToLower();

        Dictionary<string, Func<int,string>> validationForDateTime = new Dictionary<string, Func<int, string>>
        {
            {"", num => num >=0 ? "" : "Sorry, but we don't really have a use for negative numbers."},
            {"year", num => num>1500 && num<=2500 ? "" : "Wow! You must live in another age. We can only do years between 1000 and 3000. Please try again." },
            {"month", num=> num >= 0 && num <= 12 ? "" : "Woah! I don't know what cool calendar you're on, but ours only has months between 1 and 12." },
            {"day", num => num >=1 && num <=31 ? "" : "Woah! I don't know which cool time system you're on, but our months only have days between 1 and 31. Please try again."},
            {"hour", num => num >=0 && num <=23 ? "" : "Wow! I would love to live in a world with that many hours in the day. Our clock only goes from 0 to 23 hours." },
            {"minute", num => num >=0 && num <= 59 ? "" : "Wow! That looks like a cool time system, but our clock only includes minutes from 0 to 59." },
            {"second", num => num >= 0 && num <= 59 ? "" : "Wow! That looks like a cool time system, but our clock only includes seconds from 0 to 59."},
            {"days", num=> num >=1 ? "" : "Woah, we can't show you less than the past 1 day." },
            {"weeks", num=> num >= 1 ? "" : "Woah! There seems to be an error. We can't show you less than the past 1 week if you are filtering by weeks." },
            {"months", num => num >=1 ? "" : "Woah, we can't show you than the past 1 month in month filters." },
            {"years", num => num >=1 ? "" : "Woah, we can't show you less than 1 year if you are filtering by year." }
        };

        if (int.TryParse(maybeNumber, out realNumberValue))
        {
            errorMessage = validationForDateTime[typeOfDateUnit](realNumberValue);   
        }
        else
        {
            errorMessage = "I'm sorry, but I didn't understand that number. Please try again.";
        }
        return realNumberValue;
    }
}

    
