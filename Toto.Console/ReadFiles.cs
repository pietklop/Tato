namespace Toto.Console;

public static class ReadFiles
{
    public static string[] ReadGames(string fileName)
    {
        string path = @"c:\Projects\Toto\Data\";
        string pathFile = Path.Combine(path, fileName);
        return System.IO.File.ReadAllLines(pathFile);
    }
}