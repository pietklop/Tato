namespace Core;

public static class GameHelper
{
    public static List<Game> CompetitionOnly(this List<Game> games)
    {
        return games.Where(g => !g.OtherLeague).ToList();
    }
}