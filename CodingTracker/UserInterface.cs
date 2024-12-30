
namespace TSCA.CodingTracker;
internal class UserInterface
{
    private readonly CodingSessionController _codingSessionController = new();
    private readonly DatabaseManager _databaseManager = new();

    internal static void ShowMainMenu(bool GUIMode = false) //if not gui mode, then spectre.console. if gui mode, then maui
    {

    }

}
