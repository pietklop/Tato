using Core.Calc;

namespace Core
{
    public class Game
    {
        public DateOnly Date { get; }
        public Team HomeTeam { get; set; }
        public Team AwayTeam { get; set; }
        public int GoalsHome { get; set; }
        public int GoalsAway { get; set; }
        public float ExpectedGoalsHome { get; set; }
        public float ExpectedGoalsAway { get; set; }
        public GameResult Result { get; set; }
        /// <summary>
        /// Game is from another league, probably for another cup
        /// Is only used to indicate schedule business
        /// </summary>
        public bool OtherLeague { get; set; }
        /// <summary>
        /// True in case the game prediction has been made more than 1 week up front
        /// Maybe interesting to track for future analysis to compare any differences
        /// Should maybe move to <see cref="BookOdds"/>
        /// </summary>
        public bool OutOfScope { get; set; }

        public Odd PredictedOdd { get; set; }
        public List<Odd>? BookOdds { get; set; }

        public Game(DateOnly date)
        {
            Date = date;
            GoalsHome = -1;
            GoalsAway = -1;
        }

        public Odd PredictOdds()
        {
            if (HomeTeam.Name.StartsWith("Hee") && AwayTeam.Name.StartsWith("PSV") && Date.Year == 2024)
                ;
            if (OtherLeague) throw new Exception("Game is from another league");


            if (FormCalculations.Enabled)
            {
                this.PredictFormCorrectedOdds();
            }
            else
            {
                ExpectedGoalsHome = (HomeTeam.ExpectedGoalsScoredHome + AwayTeam.ExpectedGoalsConcededAway) / 2;
                ExpectedGoalsAway = (AwayTeam.ExpectedGoalsScoredAway + HomeTeam.ExpectedGoalsConcededHome) / 2;
            }

            var dict = OddCalculator.CalcChances(ExpectedGoalsHome, ExpectedGoalsAway);
            PredictedOdd = new Odd(this, [dict[GameResult.HomeWin], dict[GameResult.Draw], dict[GameResult.AwayWin]]);
            return PredictedOdd;
        }

        public void Validate()
        {
            if (Date == DateOnly.MinValue) throw new Exception($"{nameof(Date)} not set");
            if (HomeTeam == null) throw new Exception($"{nameof(HomeTeam)} not set");
            if (AwayTeam == null) throw new Exception($"{nameof(AwayTeam)} not set");
            if (GoalsHome < 0) throw new Exception($"{nameof(GoalsHome)} not set");
            if (GoalsAway < 0) throw new Exception($"{nameof(GoalsAway)} not set");
            if (GoalsHome > GoalsAway)
                Result = GameResult.HomeWin;
            else if (GoalsHome < GoalsAway)
                Result = GameResult.AwayWin;
            else
                Result = GameResult.Draw;
        }

        public override string ToString()
        {
            if (GoalsHome == -1 && BookOdds != null && BookOdds.Any())
                return $"{Date}: {HomeTeam}-{AwayTeam} {BookOdds[0].Odds[GameResult.HomeWin]:F2}-{BookOdds[0].Odds[GameResult.Draw]:F2}-{BookOdds[0].Odds[GameResult.AwayWin]:F2}  {BookOdds[0].TotalChance:F2}";
            return $"{Date}: {HomeTeam}-{AwayTeam} {GoalsHome}-{GoalsAway}";
        }
    }
}
