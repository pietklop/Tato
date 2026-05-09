using Core.Calc;
using Core.Prediction;

namespace Core
{
    public class Game : IEquatable<Game>
    {
        public DateOnly Date { get; }
        public Team? HomeTeam { get; set; }
        public Team? AwayTeam { get; set; }
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

        public Game(DateOnly date, Team homeTeam, Team awayTeam, int goalsHome, int goalsAway)
        {
            Date = date;
            HomeTeam = homeTeam;
            AwayTeam = awayTeam;
            GoalsHome = goalsHome;
            GoalsAway = goalsAway;
        }

        public Odd PredictOdds()
        {
            if (HomeTeam.Name.StartsWith("Hee") && AwayTeam.Name.StartsWith("PSV") && Date.Year == 2024)
                ;
            if (OtherLeague) throw new Exception("Game is from another league");


            if (Parameters.Form.IsEnabled)
                this.PredictFormCorrectedOdds();
            else
            {
                ExpectedGoalsHome = (HomeTeam.ExpectedGoalsScoredHome + AwayTeam.ExpectedGoalsConcededAway) / 2;
                ExpectedGoalsAway = (AwayTeam.ExpectedGoalsScoredAway + HomeTeam.ExpectedGoalsConcededHome) / 2;
            }

            if (Parameters.PhysicalFitness.IsEnabled)
            {
                var homeFitness = HomeTeam.CalculatePhysicalFitness(Date);
                var awayFitness = AwayTeam.CalculatePhysicalFitness(Date);
                var fitnessBalance = homeFitness / awayFitness;
                ExpectedGoalsHome *= fitnessBalance;
                ExpectedGoalsAway /= fitnessBalance;
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

        public bool Equals(Game? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Date.Equals(other.Date) && (HomeTeam == null && other.HomeTeam == null || HomeTeam.Equals(other.HomeTeam)) && (AwayTeam == null && other.AwayTeam == null || AwayTeam.Equals(other.AwayTeam));
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Game)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Date.GetHashCode();
                hashCode = (hashCode * 397) ^ HomeTeam.GetHashCode();
                hashCode = (hashCode * 397) ^ AwayTeam.GetHashCode();
                return hashCode;
            }
        }
    }
}
