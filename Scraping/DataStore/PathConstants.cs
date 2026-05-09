namespace Scraping.DataStore;

public static class PathConstants
{
    private const string BasePath = @"c:\Projects\Toto\Data";

    public static string SeasonBase(int startYear) => Path.Combine(BasePath, $"Season{startYear % 1000}");

    public static string SeasonGamesFile(int startYear) => Path.Combine(SeasonBase(startYear), "Eredivisie.txt");

    public static string OtherLeagueMatchesFile(int startYear)
    {
        if (startYear > 2000) startYear -= 2000;
        return Path.Combine(SeasonBase(startYear), "OtherLeagueMatches.txt");
    }
}