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

    public List<Bet> SelectGames()
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

        return bets;

        void TryAddBet(ref Bet? bestBet, Odd predictedGame, Game bookMakerGame, GameResult prediction)
        {
            var factor = predictedGame.Chances[prediction] / bookMakerGame.BookOdds[0].Chances[prediction];
            if (factor < Parameters.MinChanceFactor)
                return;
            if (bestBet == null || factor > bestBet.PredictedOdd.Chances[bestBet.Prediction] /
                bestBet.BookmakersGame.BookOdds[0].Chances[bestBet.Prediction])
            {
                switch (Parameters.BetStrategy)
                {
                    case BetStrategy.Flat:
                        bestBet = new Bet(predictedGame, bookMakerGame, prediction, 1);
                        break;
                    case BetStrategy.PredictionGap:
                        bestBet = new Bet(predictedGame, bookMakerGame, prediction, factor);
                        break;
                    case BetStrategy.Kelly:
                        bestBet = new Bet(predictedGame, bookMakerGame, prediction, KellyBetSize(predictedGame.Chances[prediction], bookMakerGame.BookOdds[0].Chances[prediction]));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        float KellyBetSize(float p, float odd) => (p * odd - 1) / (odd - 1);
    }
}