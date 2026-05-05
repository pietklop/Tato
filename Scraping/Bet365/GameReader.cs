using Core;
using Core.Infra;

namespace Scraping.Bet365;

public static class GameReader
{
    public static List<Game> ReadRawGames(string fileName, int year, DateOnly friday)
    {
        var lines = File.ReadAllLines(fileName);
        var games = new List<Game>();
        var oddsDict = new Dictionary<Game, float[]>();
        var teams = new HashSet<Team>();
        var step = ParseSequence.Date;
        Game game = null;
        DateOnly gameDate = DateOnly.MinValue;
        var gameIndex = 0;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (!line.HasContent()) continue;
            var d = TryParseDate(line, year);
            if (d != null) // new date line
                step = ParseSequence.Date;

            switch (step)
            {
                case ParseSequence.Date:
                    if (d == null) continue;
                    gameDate = d.Value;
                    game = new Game(gameDate);
                    games.Add(game);
                    step = ParseSequence.HomeTeam;
                    i++; // skip time
                    break;
                case ParseSequence.HomeTeam:
                    if (line == "1") // begin of odds
                        step = ParseSequence.HomeWinOdds;
                    else if (IsTime(line))
                    {
                        game = new Game(gameDate);
                        games.Add(game);
                    }
                    else
                    {
                        game.HomeTeam = AddOrGetTeam(line);
                        step = ParseSequence.AwayTeam;
                    }
                    break;
                case ParseSequence.AwayTeam:
                    if (game.HomeTeam == null) throw new Exception("Home team not set before away team");
                    game.AwayTeam = AddOrGetTeam(line);
                    step = ParseSequence.HomeTeam;
                    break;
                case ParseSequence.HomeWinOdds:
                    if (line == "1") continue; // header
                    if (line == "X") {
                        step = ParseSequence.DrawOdds; // header
                        gameIndex = 0;
                        continue;
                    }
                    var odd = ParseToFloat(line);
                    oddsDict.Add(games[gameIndex], new float[3]);
                    oddsDict[games[gameIndex++]][0] = odd;
                    break;
                case ParseSequence.DrawOdds:
                    if (line == "X") continue; // header
                    if (line == "2")
                    {
                        step = ParseSequence.AwayWinOdds; // header
                        gameIndex = 0;
                        continue;
                    }
                    odd = ParseToFloat(line);
                    oddsDict[games[gameIndex++]][1] = odd;
                    break;
                case ParseSequence.AwayWinOdds:
                    if (line == "2") continue; // header
                    odd = ParseToFloat(line);
                    game = games[gameIndex++];
                    oddsDict[game][2] = odd;
                    if (game.BookOdds != null) throw new Exception("Odds already set for game");
                    game.BookOdds = [new Odd(bookmaker, game, oddsDict[game])];
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        ProcessGames(games, friday);

        return games;

        Team AddOrGetTeam(string name)
        {
            name = TeamAliases.FindName(name);
            var team = teams.FirstOrDefault(t => t.Name == name);
            if (team != null)
                return team;

            team = new Team(name);
            teams.Add(team);
            return team;
        }
    }

    private static void ProcessGames(List<Game> games, DateOnly friday)
    {
        foreach (var game in games)
        {
            if (game.Date < friday) throw new Exception($"Game date {game.Date} is before the Friday {friday}");
            if (game.Date > friday.AddDays(4))
                game.OutOfScope = true;
        }
    }

    private static float ParseToFloat(string line) => float.Parse(line.Replace(".", ","));

    private static DateOnly? TryParseDate(string line, int year)
    {
        var split = line.Split(' ');
        if (split.Length != 3)
            return null;
        var day = int.TryParse(split[1], out var d) ? d : (int?)null;
        if (day == null)
            return null;
        var month = Months[split[2]];

        return new DateOnly(year, month, day.Value);
    }

    private static bool IsTime(string line)
    {
        if (line.Length != 5) return false;
        if (line[2] == ':') return true;
        return false;
    }

    private static readonly Dictionary<string, int> Months = new()
    {
        ["jan"] = 1,
        ["feb"] = 2,
        ["maa"] = 3,
        ["apr"] = 4,
        ["mei"] = 5,
        ["jun"] = 6,
        ["jul"] = 7,
        ["aug"] = 8,
        ["sep"] = 9,
        ["okt"] = 10,
        ["nov"] = 11,
        ["dec"] = 12
    };

    private static Bookmaker bookmaker = new Bookmaker("Bet365");

    enum ParseSequence // next to parse
    {
        Date,
        HomeTeam,
        AwayTeam,
        HomeWinOdds,
        DrawOdds,
        AwayWinOdds,
    }
}