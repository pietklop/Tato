using Core.Infra;

namespace Core;

/// <summary>
/// Single line game parser
/// </summary>
public static class GameParser
{
    public static string ToSingleLine(this Game game)
    {
        return $"{game.Date:yyyyMMdd}|{game.HomeTeam?.Name ?? ""}|{game.AwayTeam?.Name ?? ""}|{game.GoalsHome}|{game.GoalsAway}";
    }

    public static Game FromSingleLine(string line, List<Team> teams)
    {
        var split = line.Split("|");
        if (split.Length != 5) throw new Exception($"Line has incorrect format: {line}");

        var teamNameHome = split[1];
        Team? teamHome = null;
        if (teamNameHome.HasContent())
        {
            teamHome = teams.FirstOrDefault(t => t.Name == teamNameHome);
            if (teamHome == null)
            {
                teamHome = new Team(teamNameHome);
                teams.Add(teamHome);
            }
        }

        var teamNameAway = split[2];
        Team? teamAway = null;
        if (teamNameAway.HasContent())
        {
            teamAway = teams.FirstOrDefault(t => t.Name == teamNameAway);
            if (teamAway == null)
            {
                teamAway = new Team(teamNameAway);
                teams.Add(teamAway);
            }
        }

        return new Game(new DateOnly(int.Parse(split[0].Substring(0, 4)), int.Parse(split[0].Substring(4, 2)), int.Parse(split[0].Substring(6, 2))),
                        teamHome,
                        teamAway,
                        int.Parse(split[3]),
                        int.Parse(split[4]));
    }
}