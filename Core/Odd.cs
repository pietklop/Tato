namespace Core;

public class Odd
{
    public Bookmaker Bookmaker { get; }
    public Game Game { get; }
    public Dictionary<GameResult, float> Odds { get; }
    public Dictionary<GameResult, float> Chances { get; }
    /// <summary>
    /// The chance corresponding to the actual result of the game
    /// </summary>
    public float ResultChance { get; set; }
    /// <summary>
    /// For bookmakers this is usually above 1, indicating their margin
    /// </summary>
    public float TotalChance { get; set; }

    public Odd(Bookmaker bookmaker, Game game, float[] odds)
    {
        Bookmaker = bookmaker;
        Game = game;
        if (odds.Length != 3)
            throw new ArgumentException("Odds array must have exactly 3 elements for HomeWin, Draw, and AwayWin.");
        Odds = new Dictionary<GameResult, float>
        {
            { GameResult.HomeWin, odds[0] },
            { GameResult.Draw, odds[1] },
            { GameResult.AwayWin, odds[2] }
        };
        Chances = new Dictionary<GameResult, float>
        {
            { GameResult.HomeWin, 1 / odds[0] },
            { GameResult.Draw, 1 / odds[1] },
            { GameResult.AwayWin, 1 / odds[2] }
        };

        TotalChance = 1 / odds[0] + 1 / odds[1] + 1 / odds[2];

        Validate(odds);
        if (Game.Result != GameResult.None)
            ResultChance = Chances[Game.Result];
    }

    public Odd(Game game, float[] chances)
    {
        Game = game;
        if (chances.Length != 3)
            throw new ArgumentException("Chances array must have exactly 3 elements for HomeWin, Draw, and AwayWin.");
        Odds = new Dictionary<GameResult, float>
        {
            { GameResult.HomeWin, 1 / chances[0] },
            { GameResult.Draw, 1 / chances[1] },
            { GameResult.AwayWin, 1 / chances[2] }
        };
        Chances = new Dictionary<GameResult, float>
        {
            { GameResult.HomeWin, chances[0] },
            { GameResult.Draw, chances[1] },
            { GameResult.AwayWin, chances[2] }
        };

        TotalChance = chances[0] + chances[1] + chances[2];

        Validate([1 / chances[0], 1 / chances[1], 1 / chances[2]]);
        ResultChance = Chances[Game.Result];
    }

    public float GetPredictionScore()
    {
        if (Game.Result == GameResult.None) throw new Exception($"{Game} result not set");
        return ResultChance;
    }

    private void Validate(float[] odds)
    {
        if (Game == null) throw new Exception($"{nameof(Game)} not set");

        foreach (var odd in odds)
        {
            if (odd <= 1) throw new ArgumentException($"{Game} Odd must be larger than 1");
        }

        if (Bookmaker == null)
        {   // own prediction
            if (TotalChance < 0.999f || TotalChance > 1.001) throw new ArgumentException($"{Game} Total chance {TotalChance:F2} out of range");
        }
        else
        {
            if (TotalChance <= 1f || TotalChance > 1.1f) throw new ArgumentException($"{Game} Total chance {TotalChance:F2} out of range");
            if (odds[1] <= odds[0] && odds[1] <= odds[2]) // it has happened in prediction
                throw new ArgumentException($"{Game} Highly unlikely that a draw has the largest chance");
        }

    }

    public override string ToString() => $"{Game}: {string.Join(", ", Chances.Select(o => $"{o.Key}: {o.Value}"))}";
}