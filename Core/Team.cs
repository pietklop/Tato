using Core.Calc;
using Core.Prediction;

namespace Core;

public class Team : IEquatable<Team>
{
    public string Name { get; }
    public SortedDictionary<DateOnly, Game> Games { get; set; } = new();
    public TeamStats Stats { get; set; }
    public float ExpectedGoalsScoredHome { get; set; }
    public float ExpectedGoalsScoredAway { get; set; }
    public float ExpectedGoalsConcededHome { get; set; }
    public float ExpectedGoalsConcededAway { get; set; }
    public float ExpectedGoalsScoredTotal { get; set; }
    public float ExpectedGoalsConcededTotal { get; set; }
    /// <summary>
    /// Positive value means team is scoring more than expected recently
    /// </summary>
    public float GoalsScoredFormCorrection { get; set; }
    /// <summary>
    /// Positive value means team is conceding more than expected recently
    /// </summary>
    public float GoalsConcededFormCorrection { get; set; }
    public float Form => GoalsScoredFormCorrection - GoalsConcededFormCorrection;

    public Team(string name)
    {
        Name = name;
    }

    public void ClearGamesAndStats()
    {
        Games.Clear();
        Stats = null;
        ExpectedGoalsScoredHome = 0;
        ExpectedGoalsScoredAway = 0;
        ExpectedGoalsConcededHome = 0;
        ExpectedGoalsConcededAway = 0;
        ExpectedGoalsScoredTotal = 0;
        ExpectedGoalsConcededTotal = 0;
    }

    public void CalcStrength(DateOnly? endDate = null)
    {
        Stats = new TeamStats(this, Games, endDate);
        ExpectedGoalsScoredHome = Stats.AvgGoalsScoredHome;
        ExpectedGoalsScoredAway = Stats.AvgGoalsScoredAway;
        ExpectedGoalsConcededHome = Stats.AvgGoalsConcededHome;
        ExpectedGoalsConcededAway = Stats.AvgGoalsConcededAway;
        ExpectedGoalsScoredTotal = Stats.TotalGoalsScored / (float)(Stats.HomeGames + Stats.AwayGames);
        ExpectedGoalsConcededTotal = Stats.TotalGoalsConceded / (float)(Stats.HomeGames + Stats.AwayGames);
    }

    /// <summary>
    /// Depends on the past game schedule
    /// 1 means team had enough rest
    /// </summary>
    public float CalculatePhysicalFitness(DateOnly gameDate)
    {
        var previousGameDates = Games.Where(g => g.Key >= gameDate.AddDays(-10) && g.Key < gameDate)
            .Select(g => (DateOnly)g.Key).OrderBy(d => d).ToList();

        if (previousGameDates.Count == 0) return 1;

        Parameters.PhysicalFitness.FitnessAfterGame = 0.95f;
        var fitness = 1f;

        if (previousGameDates.Count > 1)
            ;

        foreach (var date in previousGameDates)
        {
            var followUpGame = previousGameDates.Any(d => d > date) ? previousGameDates.First(d => d > date) : gameDate;
            var daysAGo = (followUpGame.ToDateTime(TimeOnly.MinValue) - date.ToDateTime(TimeOnly.MinValue)).Days;
            var correction = Fitness(daysAGo);
            fitness *= correction;
        }

        return fitness;

        // exponential recovery
        float Fitness(int daysOfRest) => (float)Math.Exp((Parameters.PhysicalFitness.FitnessAfterGame -1) / daysOfRest);
    }

    public override string ToString() => $"{Name} (ExGoals: {ExpectedGoalsScoredTotal:F2}/{ExpectedGoalsConcededTotal:F2}) Form:{Form:F2}";

    public bool Equals(Team? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Team)obj);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}

public class TeamStats
{
    public Team Team { get; }
    /// <summary>
    /// Used games to calculate the stats
    /// Could contain duplicate games to give more weight to recent games
    /// </summary>
    internal List<Game> Games { get; }
    public DateOnly? EndDate { get; }

    public int HomeGames { get; set; }
    public int HomeWins { get; set; }
    public int HomeDraws { get; set; }
    public int AwayGames { get; set; }
    public int AwayWins { get; set; }
    public int AwayDraws { get; set; }
    public int TotalWins => HomeWins + AwayWins;
    public int TotalDraws => HomeDraws + AwayDraws;
    public int GoalsScoredHome { get; set; }
    public int GoalsScoredAway { get; set; }
    public int GoalsConcededHome { get; set; }
    public int GoalsConcededAway { get; set; }
    public int TotalGoalsScored => GoalsScoredHome + GoalsScoredAway;
    public int TotalGoalsConceded => GoalsConcededHome + GoalsConcededAway;
    public float AvgGoalsScoredHome => HomeGames == 0 ? 0 : GoalsScoredHome / (float)HomeGames;
    public float AvgGoalsConcededHome => HomeGames == 0 ? 0 : GoalsConcededHome / (float)HomeGames;
    public float AvgGoalsScoredAway => AwayGames == 0 ? 0 : GoalsScoredAway / (float)AwayGames;
    public float AvgGoalsConcededAway => AwayGames == 0 ? 0 : GoalsConcededAway / (float)AwayGames;

    public TeamStats(Team team, SortedDictionary<DateOnly, Game> games, DateOnly? endDate = null)
    {
        Team = team;
        EndDate = endDate;
        Games = endDate.HasValue ?
            games.Where(kv => kv.Key <= endDate).Select(kv => kv.Value).ToList() : [..games.Values];

        Games = Games.CompetitionOnly();
        foreach (var game in Games)
        {
            if (game.HomeTeam == Team)
            {
                HomeGames++;
                GoalsScoredHome += game.GoalsHome;
                GoalsConcededHome += game.GoalsAway;
                if (game.Result == GameResult.HomeWin) HomeWins++;
                if (game.Result == GameResult.Draw) HomeDraws++;
            }
            else
            {
                AwayGames++;
                GoalsScoredAway += game.GoalsAway;
                GoalsConcededAway += game.GoalsHome;
                if (game.Result == GameResult.AwayWin) AwayWins++;
                if (game.Result == GameResult.Draw) AwayDraws++;
            }
        }
    }
}