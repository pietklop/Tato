namespace Core.Calc;

public static class FormCalculations
{
    public static bool Enabled = true;

    public static void PredictFormCorrectedOdds(this Game game)
    {
        if (game.OtherLeague) throw new Exception("Game is from another league");
        game.ExpectedGoalsHome = (game.HomeTeam.ExpectedGoalsScoredHome + game.HomeTeam.GoalsScoredFormCorrection + game.AwayTeam.ExpectedGoalsConcededAway + game.AwayTeam.GoalsConcededFormCorrection) / 2;
        game.ExpectedGoalsHome = Math.Max(0.1f, game.ExpectedGoalsHome);
        game.ExpectedGoalsAway = (game.AwayTeam.ExpectedGoalsScoredAway + game.AwayTeam.GoalsScoredFormCorrection + game.HomeTeam.ExpectedGoalsConcededHome + game.HomeTeam.GoalsConcededFormCorrection) / 2;
        game.ExpectedGoalsAway = Math.Max(0.1f, game.ExpectedGoalsAway);
    }

    public static void CalcFormCorrections(this Season season)
    {
        if (!Enabled) return;
        var weights = new[] { 1f, 1f };

        foreach (var team in season.Teams)
        {
            for (int i = 0; i < weights.Length; i++)
            {
                var game = team.Stats.Games[^(i + 1)];
                if (game.ExpectedGoalsHome == 0) game.PredictOdds();
            }
            team.Stats.CalcFormCorrections(weights);
        }
    }

    private static void CalcFormCorrections(this TeamStats teamStats, float[] weights)
    {
        var team = teamStats.Team;
        var games = teamStats.Games;

        if (team.Name.StartsWith("PSV"))
            ;

        var diffScored = 0f;
        var diffConceded = 0f;
        for (int i = 0; i < weights.Length; i++)
        {
            var game = games[^(i+1)];
            var homeGame = game.HomeTeam == team;

            var homeDiff = (game.GoalsHome - game.ExpectedGoalsHome) * weights[i];
            var awayDiff = (game.GoalsAway - game.ExpectedGoalsAway) * weights[i];
            diffScored += homeGame ? homeDiff : awayDiff;
            diffConceded += homeGame ? awayDiff : homeDiff;
        }
        team.GoalsScoredFormCorrection = diffScored / weights.Sum();
        team.GoalsConcededFormCorrection = diffConceded / weights.Sum();
    }

    private static float LimitTo3(this float value)
    {
        if (value > 3f) return 3f;
        if (value < -3f) return -3f;
        return value;
    }
}