using Core.Prediction;

namespace Core.Calc;

public static class FormCalculations
{
    public static void PredictFormCorrectedOdds(this Game game)
    {
        if (game.OtherLeague) throw new Exception("Game is from another league");
        game.ExpectedGoalsHome = (game.HomeTeam.ExpectedGoalsScoredHome + game.HomeTeam.GoalsScoredFormCorrection + game.AwayTeam.ExpectedGoalsConcededAway + game.AwayTeam.GoalsConcededFormCorrection) / 2;
        game.ExpectedGoalsHome = Math.Max(Parameters.Form.MinimumNumberOfExpectedGoals, game.ExpectedGoalsHome);
        game.ExpectedGoalsAway = (game.AwayTeam.ExpectedGoalsScoredAway + game.AwayTeam.GoalsScoredFormCorrection + game.HomeTeam.ExpectedGoalsConcededHome + game.HomeTeam.GoalsConcededFormCorrection) / 2;
        game.ExpectedGoalsAway = Math.Max(Parameters.Form.MinimumNumberOfExpectedGoals, game.ExpectedGoalsAway);
    }

    public static void CalcFormCorrections(this Season season)
    {
        if (!Parameters.Form.IsEnabled) return;

        foreach (var team in season.Teams)
        {
            for (int i = 0; i < Parameters.Form.WeightsLastGames.Length; i++)
            {
                var game = team.Stats.Games[^(i + 1)];
                if (game.ExpectedGoalsHome == 0) game.PredictOdds();
            }
            team.Stats.CalcFormCorrections(Parameters.Form.WeightsLastGames, Parameters.Form.LimitCorrection);
        }
    }

    private static void CalcFormCorrections(this TeamStats teamStats, float[] weights, float limit)
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
        if (team.GoalsScoredFormCorrection > limit) team.GoalsScoredFormCorrection = limit;
        if (team.GoalsScoredFormCorrection < -limit) team.GoalsScoredFormCorrection = -limit;
        team.GoalsConcededFormCorrection = diffConceded / weights.Sum();
        if (team.GoalsConcededFormCorrection > limit) team.GoalsConcededFormCorrection = limit;
        if (team.GoalsConcededFormCorrection < -limit) team.GoalsConcededFormCorrection = -limit;
    }

    private static float LimitTo3(this float value)
    {
        if (value > 3f) return 3f;
        if (value < -3f) return -3f;
        return value;
    }
}