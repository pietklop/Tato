using Scraping.DataStore;

namespace Toto.Console;

public static class ReadFiles
{
    public static string[] ReadGames(int startYear)
    {
        return File.ReadAllLines(PathConstants.SeasonGamesFile(startYear));
    }
}