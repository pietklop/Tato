using Core;
using Scraping.FcUpdate;

namespace Toto.Console;

public static class Processor
{
    public static Season ReadSeason(string fileName)
    {
        var lines = ReadFiles.ReadGames(fileName);

        return SeasonParser.ParseGames(lines);
    }
}