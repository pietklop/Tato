using Core.Calc;

namespace Core.Prediction;

public class GameSelector
{
    private readonly List<Odd> _predictedGames;
    private readonly List<Game> _bookMakerGames;

    public GameSelector(List<Odd> predictedGames, List<Game> bookMakerGames)
    {
        _predictedGames = predictedGames;
        _bookMakerGames = bookMakerGames;
    }

    public List<Bet> SelectGames(Season season)
    {
        var bets = new List<Bet>();
        foreach (var bookMakerGame in _bookMakerGames)
        {
            bookMakerGame.HomeTeam = season.Teams.Single(t => t.Name == bookMakerGame.HomeTeam.Name);
            bookMakerGame.AwayTeam = season.Teams.Single(t => t.Name == bookMakerGame.AwayTeam.Name);
            bookMakerGame.PredictFormCorrectedOdds();
            var predictedGame = bookMakerGame.PredictOdds();
            Bet? bestBet = null;
            TryAddBet(ref bestBet, predictedGame, bookMakerGame, GameResult.HomeWin);
            TryAddBet(ref bestBet, predictedGame, bookMakerGame, GameResult.Draw);
            TryAddBet(ref bestBet, predictedGame, bookMakerGame, GameResult.AwayWin);
            if (bestBet != null) bets.Add(bestBet);
        }

        return bets;
    }

    /// <summary>
    /// To analyze
    /// </summary>
    public List<Bet> SelectFromPlayedGames()
    {
        var bets = new List<Bet>();

        var notMatchedGames = new List<Game>();
        foreach (var predictedGame in _predictedGames)
        {
            var bookMakerGame = _bookMakerGames.FirstOrDefault(g => g.Date == predictedGame.Game.Date && g.HomeTeam.Name == predictedGame.Game.HomeTeam.Name && g.AwayTeam.Name == predictedGame.Game.AwayTeam.Name);
            if (bookMakerGame == null)
            {
                notMatchedGames.Add(predictedGame.Game);
                continue;
            }

            Bet? bestBet = null;
            TryAddBet(ref bestBet, predictedGame, bookMakerGame, GameResult.HomeWin);
            TryAddBet(ref bestBet, predictedGame, bookMakerGame, GameResult.Draw);
            TryAddBet(ref bestBet, predictedGame, bookMakerGame, GameResult.AwayWin);
            if (bestBet != null) bets.Add(bestBet);
        }

        // // print game result stats
        // var h = bets.Count(b => b.PredictedOdd.Game.Result == GameResult.HomeWin);
        // Console.WriteLine($"Home: {h} {(float)h / bets.Count:P0}");
        // var a = bets.Count(b => b.PredictedOdd.Game.Result == GameResult.AwayWin);
        // Console.WriteLine($"Away: {a} {(float)a / bets.Count:P0}");
        // var d = bets.Count(b => b.PredictedOdd.Game.Result == GameResult.Draw);
        // Console.WriteLine($"Draw: {d} {(float)d / bets.Count:P0}");
        //
        // // seems to happen for about 2-3% of the games (most likely an away win converted to a draw or home win)
        // var possEarlyPayout = bets.Select(b => b.PredictedOdd.Game).Where(g => g.GoalsHome >= 2 && g.GoalsAway >= 2).ToList();

        return bets;
    }

    private static void TryAddBet(ref Bet? bestBet, Odd predictedGame, Game bookMakerGame, GameResult prediction)
    {
        float diff = 0f;
        float factor = 0f;
        if (Parameters.UseAbsoluteChanceDiff)
        {
            diff = predictedGame.Chances[prediction] - bookMakerGame.BookOdds[0].Chances[prediction];
            if (diff < Parameters.MinChanceDiff)
                return;
        }
        else
        {
            factor = predictedGame.Chances[prediction] / bookMakerGame.BookOdds[0].Chances[prediction];
            if (factor < Parameters.MinChanceFactor)
                return;
        }

        if (bestBet == null || FactorImprovement(factor, bestBet) || DiffImprovement(diff, bestBet))
        {
            switch (Parameters.BetStrategy)
            {
                case BetStrategy.Flat:
                    bestBet = new Bet(predictedGame, bookMakerGame, prediction, 1);
                    break;
                case BetStrategy.PredictionGap:
                    bestBet = new Bet(predictedGame, bookMakerGame, prediction, Parameters.UseAbsoluteChanceDiff ? diff : factor);
                    break;
                case BetStrategy.Kelly:
                    bestBet = new Bet(predictedGame, bookMakerGame, prediction, KellyBetSize(predictedGame.Chances[prediction], bookMakerGame.BookOdds[0].Chances[prediction]));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        float KellyBetSize(float p, float odd) => (p * odd - 1) / (odd - 1);

        bool FactorImprovement(float factor, Bet bestBet)
        {
            if (Parameters.UseAbsoluteChanceDiff) return false;
            return factor > bestBet.PredictedOdd.Chances[bestBet.Prediction] /
                bestBet.BookmakersGame.BookOdds[0].Chances[bestBet.Prediction];
        }

        bool DiffImprovement(float diff, Bet bestBet)
        {
            if (!Parameters.UseAbsoluteChanceDiff) return false;
            return diff > bestBet.PredictedOdd.Chances[bestBet.Prediction] -
                bestBet.BookmakersGame.BookOdds[0].Chances[bestBet.Prediction];
        }
    }
}