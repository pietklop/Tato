using Core;
using Scraping.FcUpdate;

namespace Toto.Console;

public static class Processor
{
    public static Season ReadSeason(int startYear)
    {
        var lines = ReadFiles.ReadGames(startYear);

        return SeasonParser.ParseGames(lines);
    }
}