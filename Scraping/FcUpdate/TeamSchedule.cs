using Core;
using HtmlAgilityPack;

namespace Scraping.FcUpdate;

public class TeamSchedule
{
    public List<Game> ScrapeGames(string teamName, int startYear, bool readFromFile = false)
    {
        var filePathName = $@"c:\Temp\fcupdate_{teamName}_{startYear}.txt";
        if (startYear < 100) startYear += 2000;

        string[] rawLines;
        if (readFromFile)
        {
            if (!File.Exists(filePathName)) throw new Exception($"File not found: {filePathName}");
            rawLines = File.ReadAllLines(filePathName);
        }
        else
        {
            var url = $"https://www.fcupdate.nl/voetbalteams/nederland/{teamName}/programma-uitslagen/{startYear}-{startYear + 1}";
            var web = new HtmlWeb();
            var doc = web.Load(url);

            var sdoc = doc.DocumentNode.InnerText;
            File.WriteAllText(filePathName, sdoc);
            rawLines = sdoc.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        var lines = new List<string>();

        foreach (var rawLine in rawLines)
        {
            var line = rawLine.Trim();
            if (!string.IsNullOrEmpty(line)) lines.Add(line);
        }

        var step = ParseSequence.FindHeader;
        var lastGameType = GameType.Unknown;
        Game? lastGame = null;
        var games = new List<Game>();
        var teams = new HashSet<Team>();

        LoopOverLines();

        foreach (var game in games)
        {
            if (game.HomeTeam == null && game.AwayTeam == null)
                throw new Exception($"Game on {game.Date} has no recognized team");
        }

        return games;

        Team? AddOrGetTeam(string name, GameType gameType)
        {
            if (gameType == GameType.Unknown)
                throw new Exception("Game type is unknown");
            TeamAliases.TryFindName(name, out string? teamName);
            if (teamName == null)
            {
                if (gameType == GameType.Eredivisie)
                    throw new Exception($"Team not found for Eredivisie game: {name}");
                return null; // could be unknown team from other competition
            }
            var team = teams.FirstOrDefault(t => t.Name == teamName);
            if (team != null)
                return team;

            team = new Team(teamName);
            teams.Add(team);
            return team;
        }

        void LoopOverLines()
        {
            for (int i = 0; i < lines.Count; i++)
            {
                // if (i == 468)
                //     ;

                var line = lines[i];

                switch (step)
                {
                    case ParseSequence.FindHeader:
                        if (line == "Wedstrijd")
                            step = ParseSequence.GameType;
                        break;
                    case ParseSequence.GameType:
                        if (line == "Profiel") // end of schedule
                            return;
                        if (gameTypeDict.TryGetValue(line, out lastGameType))
                            step = ParseSequence.Date;
                        break;
                    case ParseSequence.Date:
                        var d = TryParseDate(line);
                        if (d == null) throw new Exception($"Invalid date format: {line}");
                        lastGame = new Game(d.Value);
                        games.Add(lastGame);
                        step = ParseSequence.HomeTeam;
                        if (lines[i+1].Length == 1)
                            i++; // skip "w", "g", "v" (only available for past games)
                        break;
                    case ParseSequence.HomeTeam:
                        lastGame!.HomeTeam = AddOrGetTeam(line, lastGameType);
                        step = ParseSequence.HomeScore;
                        i++; // skip team logo name
                        break;
                    case ParseSequence.HomeScore:
                        if (IsTime(line)) // game not played yet, so no score
                            step = ParseSequence.AwayTeam;
                        else
                        {
                            lastGame!.GoalsHome = int.Parse(line);
                            step = ParseSequence.Divider;
                        }
                        break;
                    case ParseSequence.Divider:
                        if (line != "-") throw new Exception($"Expected '-' but got: {line}");
                        step = ParseSequence.AwayScore;
                        break;
                    case ParseSequence.AwayScore:
                        lastGame!.GoalsAway = int.Parse(line);
                        step = ParseSequence.AwayTeam;
                        break;
                    case ParseSequence.AwayTeam:
                        if (line.StartsWith("PEN ")) // game ended with penalties, so ignore this line
                            break;
                        lastGame!.AwayTeam = AddOrGetTeam(line, lastGameType);
                        if (lastGameType == GameType.Unknown) throw new Exception($"Game type unknown for away team: {line}");
                        if (lastGameType != GameType.Eredivisie) lastGame.OtherLeague = true;
                        lastGame = null;
                        lastGameType = GameType.Unknown;
                        step = ParseSequence.GameType;
                        i++; // skip team logo name
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    private static bool IsTime(string line)
    {
        if (line.Length != 5) return false;
        if (line[2] == ':') return true;
        return false;
    }

    private static DateOnly? TryParseDate(string line)
    {
        var split = line.Split(' ');
        if (split.Length != 3)
            return null;
        var day = int.TryParse(split[0], out var d) ? d : (int?)null;
        if (day == null)
            return null;
        var month = Months[split[1].Substring(0, 3)];
        var year = int.Parse(split[2]);

        return new DateOnly(year, month, day.Value);
    }
    enum ParseSequence // next to parse
    {
        FindHeader,
        GameType,
        Date,
        HomeTeam,
        HomeScore,
        Divider,
        AwayScore,
        AwayTeam,
    }
    enum GameType
    {
        Unknown,
        JohanCruijffSchaal,
        Eredivisie,
        KNVB_Beker,
        Europees,
    }

    private static readonly Dictionary<string, int> Months = new()
    {
        ["jan"] = 1,
        ["feb"] = 2,
        ["mrt"] = 3,
        ["apr"] = 4,
        ["mei"] = 5,
        ["jun"] = 6,
        ["jul"] = 7,
        ["aug"] = 8,
        ["sep"] = 9,
        ["okt"] = 10,
        ["nov"] = 11,
        ["dec"] = 12
    };

    private static Dictionary<string, GameType> gameTypeDict = new Dictionary<string, GameType>
    {
        {"JOH", GameType.JohanCruijffSchaal},
        {"ERE", GameType.Eredivisie},
        {"TOT", GameType.KNVB_Beker},
        {"UEF", GameType.Europees},
    };

    public static List<string> TeamNames2023 = new List<string>
    {
        "Ajax",
        "Almere",
        "AZ",
        "Excelsior",
        "Feyenoord",
        "Fortuna",
        "Go-Ahead",
        "Heerenveen",
        "Heracles",
        "NEC",
        "PEC",
        "PSV",
        "RKC",
        "Sparta",
        "Twente",
        "Utrecht",
        "Vitesse",
        "Volendam",
    };

    public static List<string> TeamNames2024 = new List<string>
    {
        "Ajax",
        "AZ",
        "Excelsior",
        "Feyenoord",
        "Fortuna",
        "Go-Ahead",
        "Groningen",
        "Heerenveen",
        "Heracles",
        "NAC",
        "NEC",
        "PEC",
        "PSV",
        "RKC",
        "Sparta",
        "Twente",
        "Utrecht",
        "Willem-ii",
    };

}