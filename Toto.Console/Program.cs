// See https://aka.ms/new-console-template for more information

using Core.Calc;
using Scraping.Bet365;
using Toto.Console;

Console.WriteLine("This is Toto.Console project.");

GameReader.ReadGames(@"c:\Projects\Toto\Data\Games25\2026-01-09.txt", 2026);


var season23 = Processor.ReadSeason("Eredivisie2324.txt");
var predictedOdds = OddValidator.SimulateLastSeasonPart(season23);

var season24 = Processor.ReadSeason("Eredivisie2425.txt");
var predictedOdds24 = OddValidator.SimulateLastSeasonPart(season24);

var totalResult = new SeasonResult(predictedOdds, predictedOdds24);
totalResult.CalcStats();
Console.WriteLine($"{totalResult.AvgScore:F4}");
Console.ReadKey();

OddValidator.RunCompleteSeason(season24);