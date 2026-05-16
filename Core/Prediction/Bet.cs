namespace Core.Prediction;

public class Bet
{
    public Odd PredictedOdd { get; }
    public Game BookmakersGame { get; }
    public GameResult Prediction { get; }
    public float Stake { get; private set; }
    public float Profit { get; private set; }

    public Bet(Odd predictedOdd, Game bookmakersGame, GameResult prediction, float stake)
    {
        PredictedOdd = predictedOdd;
        BookmakersGame = bookmakersGame;
        Prediction = prediction;
        Stake = stake;
        if (prediction == predictedOdd.Game.Result)
            Profit = Stake * bookmakersGame.BookOdds[0].Odds[prediction] - Stake;
        else
            Profit = -Stake;
    }

    public override string ToString()
    {
        var game = PredictedOdd.Game;
        return $"{game.HomeTeam.Name}-{game.AwayTeam.Name} {game.GoalsHome}-{game.GoalsAway}, Me:{Prediction} {PredictedOdd.Chances[Prediction]:P0}, BM: {BookmakersGame.BookOdds[0].Chances[Prediction]:P0}";
    }

    public string PrintPreBetInfo()
    {
        var game = PredictedOdd.Game;
        return $"{game.HomeTeam.Name}-{game.AwayTeam.Name} Me:{Prediction} {PredictedOdd.Chances[Prediction]:P0}/{BookmakersGame.BookOdds[0].Chances[Prediction]:P0} Odd:{BookmakersGame.BookOdds[0].Odds[Prediction]:F2} ({PredictedOdd.Game.ExpectedGoalsHome:F1}-{PredictedOdd.Game.ExpectedGoalsAway:F1})";
    }

    public string PrintOdds()
    {
        var game = PredictedOdd.Game;
        return $"{game.HomeTeam.Name}-{game.AwayTeam.Name} Odds:{BookmakersGame.BookOdds[0].Odds[GameResult.HomeWin]:F2} {BookmakersGame.BookOdds[0].Odds[GameResult.Draw]:F2} {BookmakersGame.BookOdds[0].Odds[GameResult.AwayWin]:F2}";
    }
}