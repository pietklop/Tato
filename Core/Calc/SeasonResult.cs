namespace Core.Calc;

public class SeasonResult
{
    public List<Odd> PredictedOdds { get; } = new List<Odd>();
    public float AvgScore { get; private set; }
    public int GameCount { get; set; }
    public Odd? LowestGameScore { get; set; }
    public Odd? HighestGameScore { get; set; }
    public float AvgGoalsHome { get; private set; }
    public float AvgGoalsAway { get; private set; }
    public float AvgExpectedGoalsHome { get; private set; }
    public float AvgExpectedGoalsAway { get; private set; }
    public float DiffGoalsHome { get; set; }
    public float DiffGoalsAway { get; set; }
    private float _totalGoalsHome;
    private float _totalGoalsAway;
    private float _totalExpectedGoalsHome;
    private float _totalExpectedGoalsAway;

    public SeasonResult(params List<Odd>[] predictedOdds)
    {
        foreach (var odds in predictedOdds)
        {
            PredictedOdds.AddRange(odds);
        }
    }

    public void CalcStats()
    {
        var scores = PredictedOdds.Select(o => o.GetPredictionScore()).ToList();
        AvgScore = scores.Count > 0 ? scores.Average() : 0;
        GameCount = scores.Count;
        LowestGameScore = PredictedOdds.OrderBy(o => o.GetPredictionScore()).First();
        HighestGameScore = PredictedOdds.OrderByDescending(o => o.GetPredictionScore()).First();

        foreach (var odd in PredictedOdds)
        {
            _totalGoalsHome += odd.Game.GoalsHome;
            _totalGoalsAway += odd.Game.GoalsAway;
            _totalExpectedGoalsHome += odd.Game.ExpectedGoalsHome;
            _totalExpectedGoalsAway += odd.Game.ExpectedGoalsAway;
        }

        AvgGoalsHome = _totalGoalsHome / GameCount;
        AvgGoalsAway = _totalGoalsAway / GameCount;
        AvgExpectedGoalsHome = _totalExpectedGoalsHome / GameCount;
        AvgExpectedGoalsAway = _totalExpectedGoalsAway / GameCount;
        DiffGoalsHome = AvgExpectedGoalsHome - AvgGoalsHome;
        DiffGoalsAway = AvgExpectedGoalsAway - AvgGoalsAway;
    }
}