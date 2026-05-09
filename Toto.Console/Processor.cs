using Core;
using Scraping.DataStore;
using Scraping.FcUpdate;

namespace Toto.Console;

public static class Processor
{
    public static Season ReadSeason(int startYear)
    {
        var lines = ReadFiles.ReadGames(startYear);

        var season = SeasonParser.ParseGames(lines);

        var games = OtherLeagueGames.Read(startYear);
        season.AddOtherLeagueMatches(games);

        return season;
    }
}