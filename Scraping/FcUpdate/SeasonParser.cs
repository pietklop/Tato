using Core;
using Core.Infra;

namespace Scraping.FcUpdate;

public static class SeasonParser
{
    public static Season ParseGames(string[] lines)
    {
        var games = new List<Game>();
        var teams = new HashSet<Team>();
        var step = ParseSequence.Date;
        var monthDict = CreateMonths();

        Game game = null;
        DateOnly gameDate = DateOnly.MinValue;

        for (var i = 0; i < lines.Length; i++)
        {
            if (!lines[i].HasContent()) continue;
            switch (step)
            {
                case ParseSequence.Date:
                    step = ParseSequence.HomeTeam;
                    var date = TryParseDate(lines[i]);
                    if (date == null)
                    {
                        i--; // stay on the same line
                        game = new Game(gameDate);
                        continue;
                    }
                    gameDate = date.Value;
                    game = new Game(gameDate);
                    break;
                case ParseSequence.HomeTeam:
                    game!.HomeTeam = AddOrGetTeam(lines[i++]);
                    step = ParseSequence.HomeGoals;
                    break;
                case ParseSequence.HomeGoals:
                    game!.GoalsHome = int.Parse(lines[i++]);
                    step = ParseSequence.AwayGoals;
                    break;
                case ParseSequence.AwayGoals:
                    game!.GoalsAway = int.Parse(lines[i++]);
                    step = ParseSequence.AwayTeam;
                    break;
                case ParseSequence.AwayTeam:
                    game!.AwayTeam = AddOrGetTeam(lines[i]);
                    game.Validate();
                    games.Add(game);
                    step = ParseSequence.Date;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return new Season(games.First().Date.Year, games, teams.ToList());

        Team AddOrGetTeam(string name)
        {
            var team = teams.FirstOrDefault(t => t.Name == name);
            if (team != null)
                return team;

            team = new Team(name);
            teams.Add(team);
            return team;
        }

        DateOnly? TryParseDate(string line)
        {
            var parts = line.Split(' ');
            if (parts.Length != 4)
                return null;
            var day = int.Parse(parts[1]);
            var month = monthDict[parts[2]];
            var year = int.Parse(parts[3]);
            return new DateOnly(year, month, day);
        }
    }

    private static Dictionary<string, int> CreateMonths()
    {
        return new Dictionary<string, int>
        {
            ["januari"] = 1,
            ["februari"] = 2,
            ["maart"] = 3,
            ["april"] = 4,
            ["mei"] = 5,
            ["juni"] = 6,
            ["juli"] = 7,
            ["augustus"] = 8,
            ["september"] = 9,
            ["oktober"] = 10,
            ["november"] = 11,
            ["december"] = 12
        };
    }
}

enum ParseSequence // next to parse
{
    Date,
    HomeTeam,
    HomeGoals,
    AwayGoals,
    AwayTeam
}