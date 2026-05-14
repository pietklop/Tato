namespace Core.Prediction;

public static class Parameters
{
    /// <summary>
    /// predicted chance should be at least this factor larger than bookmaker's
    /// Range of 1.2 - 2 seems to work
    /// The higher, the less bets, so less reliable
    /// </summary>
    public const float MinChanceFactor = 1.3f;

    public static BetStrategy BetStrategy = BetStrategy.Kelly;
    public static Form Form = new Form();
    public static PhysicalFitness PhysicalFitness = new PhysicalFitness();
}

public class Form
{
    public bool IsEnabled = true;
    /// <summary>
    /// [0] is the most recent game
    /// Taking 1 to 3 games into account seems to work
    /// </summary>
    public float[] WeightsLastGames = [2f,1f];
    /// <summary>
    /// Limit the effect
    /// be aware that this is applied to both scored and conceded goals
    /// Range of 0.2 - 0.5 seems to work
    /// </summary>
    public float LimitCorrection = 0.25f;
    /// <summary>
    /// Limit the minimum expected goals due to corrections
    /// </summary>
    public float MinimumNumberOfExpectedGoals = 0.1f;
}

public class PhysicalFitness
{
    public bool IsEnabled = false;
    public float FitnessAfterGame = 0.95f;
}

public enum BetStrategy
{
    None,
    Flat,
    PredictionGap,
    Kelly,
}