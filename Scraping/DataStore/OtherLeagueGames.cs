using Core;

namespace Scraping.DataStore;

public static class OtherLeagueGames
{
    public static List<Game> Read(int startYear)
    {
        var filePath = PathConstants.OtherLeagueMatchesFile(startYear);
        if (!File.Exists(filePath)) return new List<Game>();
        var lines = File.ReadAllLines(filePath);
        var games = new List<Game>();
        var teams = new List<Team>();

        foreach (var line in lines)
        {
            var game = GameParser.FromSingleLine(line, teams);
            games.Add(game);
        }

        return games;
    }

    public static void Write(List<Game> games, int startYear)
    {
        games = games.OrderBy(g => g.Date).ThenBy(g => g.HomeTeam?.Name ?? "").ThenBy(g => g.AwayTeam?.Name ?? "").ToList();
        // Remove duplicates based on Date, HomeTeam, and AwayTeam
        for (int i = 1; i < games.Count; i++)
        {
            if (games[i].Equals(games[i - 1]))
            {
                games.RemoveAt(i);
                i--; // step back because list shifted
            }
        }

        var filePath = PathConstants.OtherLeagueMatchesFile(startYear);
        var lines = games.Select(game => game.ToSingleLine()).ToArray();
        File.Delete(filePath);
        File.WriteAllLines(filePath, lines);
    }
}