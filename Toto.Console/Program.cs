// See https://aka.ms/new-console-template for more information

using Core;
using Core.Calc;
using Core.Prediction;
using Scraping.Bet365;
using Scraping.DataStore;
using Scraping.FcUpdate;
using Toto.Console;

Console.WriteLine("This is Toto.Console project.");
Console.WriteLine();

ShowMenu();

void ShowMenu()
{
    while (true)
    {
        Console.WriteLine("=== Main Menu ===");
        Console.WriteLine("1. Scrape Team Schedules");
        Console.WriteLine("2. Calculate Season Benchmark");
        Console.WriteLine("3. Predict");
        Console.WriteLine("0. Exit");
        Console.WriteLine();
        Console.Write("Select an option: ");

        var key = Console.ReadKey(intercept: true);
        Console.WriteLine(key.KeyChar);
        Console.WriteLine();

        switch (key.KeyChar)
        {
            case '1':
                ScrapeTeamSchedules();
                break;
            case '2':
                CalculateSeasonBenchmark();
                break;
            case '3':
                Predict(2025);
                break;
            case '0':
                return;
            default:
                Console.WriteLine("Invalid option. Please try again.");
                break;
        }

        Console.WriteLine();
    }
}

void Predict(int startYear)
{
    var season = Processor.ReadSeason(2025);
    var predictedOdds = OddValidator.SimulateLastSeasonPart(season);
    var wkNr = 7;
    var bet365Games = new List<Game>();
    for (int i = wkNr; i < 20; i++)
    {
        bet365Games.AddRange(GameReader.ReadRawGames(PathConstants.Bet365WeekOdds(2025, i), GameDataController.GetFridayFromWeek(2026, i)));
    }
    var gameSelector = new GameSelector(predictedOdds, bet365Games);
    var bets = gameSelector.SelectGames();

    var totalStake = bets.Sum(b => b.Stake);
    var totalProfit = bets.Sum(b => b.Profit);
    Console.WriteLine($"{bets.Count(b => b.Profit > 0)} wins of {bets.Count} bets.");
    Console.WriteLine($"Total stake:{totalStake:F2} Total profit: {totalProfit:F2}  {totalProfit / totalStake:P0}");
}

void ScrapeTeamSchedules()
{
    var scraper = new TeamSchedule();

    int year = 2024;
    scraper.ScrapeGames("Ajax", year, true);


    List<Game> games = new List<Game>();
    foreach (var name in TeamSchedule.TeamNames2024.Skip(0))
    {
        games = scraper.ScrapeGames(name, year).Where(g => g.OtherLeague).ToList();
        games.AddRange(OtherLeagueGames.Read(year));
        OtherLeagueGames.Write(games, year);
    }
}

void CalculateSeasonBenchmark()
{
    //GameDataController.ProcessFiles(@"c:\Projects\Toto\Data\Games25\Bet365");

    var season23 = Processor.ReadSeason(2023);
    var predictedOdds = OddValidator.SimulateLastSeasonPart(season23);

    var season24 = Processor.ReadSeason(2024);
    var predictedOdds24 = OddValidator.SimulateLastSeasonPart(season24);

    var totalResult = new SeasonResult(predictedOdds, predictedOdds24);
    totalResult.CalcStats();
    Console.WriteLine($"{totalResult.AvgScore:F4}");
    Console.ReadKey();

    //OddValidator.RunCompleteSeason(season24);
}