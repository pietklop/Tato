namespace Scraping.Bet365;

public static class GameDataController
{
    public static void ProcessFiles(string folderPath)
    {
        var rawFolder = Path.Combine(folderPath, "Raw");
        var files = Directory.GetFiles(rawFolder, "*wk*.txt");

        foreach (var file in files)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var split = fileName.Split("wk");
            var year = int.Parse(split[0]);
            var wkNr = int.Parse(split[1]);

            DateOnly friday = GetFridayFromWeek(year, wkNr);

            GameReader.ReadRawGames(file, year, friday);
            File.Move(file, Path.Combine(rawFolder, "Processed", Path.GetFileName(file)));
        }
    }

    private static DateOnly GetFridayFromWeek(int year, int weekNumber)
    {
        var date = System.Globalization.ISOWeek.ToDateTime(year, weekNumber, DayOfWeek.Friday);
        return DateOnly.FromDateTime(date);
    }
}