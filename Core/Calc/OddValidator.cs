using Core.Infra;

namespace Core.Calc;

public static class OddValidator
{
    public static List<Odd> SimulateLastSeasonPart(Season season)
    {
        var endDate = DateHelper.DateOfNthWeekOfTheMonthDate(season.StartYear+1,2,2, DayOfWeek.Wednesday);
        var lastGameDate = season.Games.CompetitionOnly().Last().Date;
        var predictedOdds = new List<Odd>();

        while (endDate < lastGameDate)
        {
            season.CalculateTeamStrengths(endDate);
            season.CalcFormCorrections();
            var gamesThisWeek = season.Games.Where(g => g.Date > endDate && g.Date <= endDate.AddDays(7)).ToList();
            foreach (var game in gamesThisWeek.CompetitionOnly())
            {
                game.PredictOdds();
                predictedOdds.Add(game.PredictedOdd);
            }

            endDate = endDate.AddDays(7);
        }

        return predictedOdds;
    }

    public static void RunCompleteSeason(Season season)
    {
        var totalChance = 0f;
        var nChances = 0;

        foreach (var game in season.Games)
        {
            var odds = OddCalculator.CalcChances((game.HomeTeam.ExpectedGoalsScoredHome + game.AwayTeam.ExpectedGoalsConcededAway) / 2,(game.AwayTeam.ExpectedGoalsScoredAway + game.HomeTeam.ExpectedGoalsConcededHome) / 2);
            var chance = odds[game.Result];
            totalChance += chance;
            nChances++;
        }

        var avgChance = totalChance / (float)nChances;
    }
}