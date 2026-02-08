namespace Core;

public class Season
{
    public int StartYear { get; }
    public List<Game> Games { get; }
    public List<Team> Teams { get; private set; }
    public float HomeWins { get; set; }
    public float Draws { get; set; }
    public float AwayWins { get; set; }

    public Season(int startYear, List<Game> games, List<Team> teams)
    {
        StartYear = startYear;
        Games = games;
        Teams = teams;

    }

    public void CalculateTeamStrengths(DateOnly endDate)
    {
        Teams.ForEach(t => t.ClearGamesAndStats());

        // assign games to teams
        foreach (var game in Games)
        {
            game.HomeTeam.Games.Add(game.Date, game);
            game.AwayTeam.Games.Add(game.Date, game);
        }

        //Validate();
        Teams.ForEach(team => team.CalcStrength(endDate));
        Teams = Teams.OrderByDescending(team => team.ExpectedGoalsScoredTotal - team.ExpectedGoalsConcededTotal).ToList();
    }

    public void CalculateSeasonWinStatistics()
    {
        HomeWins = Games.Count(g => g.Result == GameResult.HomeWin) / (float)Games.Count;
        Draws = Games.Count(g => g.Result == GameResult.Draw) / (float)Games.Count;
        AwayWins = Games.Count(g => g.Result == GameResult.AwayWin) / (float)Games.Count;
    }

    private void Validate()
    {
        var expectedMatches = Teams.Count * (Teams.Count - 1);
        ValidateMatchRoster();
    }

    private void ValidateMatchRoster()
    {
        foreach (var a in Teams)
        {
            foreach (var b in Teams)
            {
                if (a == b) continue;
                var n = a.Games.Count(g => g.Value.HomeTeam == a && g.Value.AwayTeam == b);
                if (n != 1)
                    throw new Exception($"Invalid number of matches {n} between {a} and {b}. Expected 1.");
                n = a.Games.Count(g => g.Value.HomeTeam == b && g.Value.AwayTeam == a);
                if (n != 1)
                    throw new Exception($"Invalid number of matches {n} between {b} and {a}. Expected 1.");
            }
        }
    }
}