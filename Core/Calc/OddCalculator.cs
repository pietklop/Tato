namespace Core.Calc;

public static class OddCalculator
{
    private static Dictionary<int, double> factorialCache = null;

    public static Dictionary<GameResult, float> CalcChances(float expHomeGoals, float expAwayGoals)
    {
        var hp = new float[5]; // home probabilities per goal
        var ap = new float[5]; // away probabilities per goal

        for (int i = 0; i < 4; i++)
        {
            hp[i] = PoissonProbability(i, expHomeGoals);
            ap[i] = PoissonProbability(i, expAwayGoals);
        }
        hp[4] = 1 - (hp[0] + hp[1] + hp[2] + hp[3]); // 4 or more goals
        ap[4] = 1 - (ap[0] + ap[1] + ap[2] + ap[3]); // 4 or more goals

        var homeWinProb = hp[1] * ap[0] +
                          hp[2] * ap[0] + hp[2] * ap[1] +
                          hp[3] * ap[0] + hp[3] * ap[1] + hp[3] * ap[2] +
                          hp[4] * ap[0] + hp[4] * ap[1] + hp[4] * ap[2] + hp[4] * ap[3];
        var drawProb = hp[0] * ap[0] +
                       hp[1] * ap[1] +
                       hp[2] * ap[2] +
                       hp[3] * ap[3] +
                       hp[4] * ap[4];
        var awayWinProb = hp[0] * ap[1] +
                          hp[0] * ap[2] + hp[1] * ap[2] +
                          hp[0] * ap[3] + hp[1] * ap[3] + hp[2] * ap[3] +
                          hp[0] * ap[4] + hp[1] * ap[4] + hp[2] * ap[4] + hp[3] * ap[4];

        var chances = new Dictionary<GameResult, float>
        {
            { GameResult.HomeWin, homeWinProb},
            { GameResult.Draw, drawProb },
            { GameResult.AwayWin, awayWinProb }
        };
        return chances;
    }

    private static float PoissonProbability(int k, double lambda) =>
        (float)(Math.Pow(lambda, k) * Math.Exp(-lambda) / Factorial(k));

    private static void FillFactorialCache()
    {
        factorialCache = new Dictionary<int, double>();
        factorialCache[0] = 1;
        double fact = 1;
        for (int i = 1; i <= 4; i++)
        {
            fact *= i;
            factorialCache[i] = fact;
        }
    }

    private static double Factorial(int n)
    {
        if (factorialCache == null)
            FillFactorialCache();
        return factorialCache![n];
    }
}