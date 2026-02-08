namespace Core;

public static class TeamAliases
{
    public static Dictionary<string, HashSet<string>> Aliases = new Dictionary<string, HashSet<string>>
    {
        { "Ajax", new HashSet<string>(){ "Ajax" } },
        { "AZ", new HashSet<string> { "AZ" } },
        { "Excelsior", new HashSet<string> { "Excelsior" } },
        { "Feyenoord", new HashSet<string> { "Feyenoord" } },
        { "Fortuna", new HashSet<string> { "Fortuna", "Fortuna Sittard" } },
        { "Go Ahead", new HashSet<string> { "Go Ahead", "Go Ahead Eagles" } },
        { "Groningen", new HashSet<string> { "Groningen", "FC Groningen" } },
        { "Heerenveen", new HashSet<string> { "Heerenveen", "sc Heerenveen" } },
        { "Heracles", new HashSet<string> { "Heracles" } },
        { "NAC", new HashSet<string> { "NAC" } },
        { "NEC", new HashSet<string> { "NEC", "N.E.C." } },
        { "PEC", new HashSet<string> { "PEC", "PEC Zwolle" } },
        { "PSV", new HashSet<string> { "PSV" } },
        { "Sparta", new HashSet<string> { "Sparta", "Sparta Rotterdam" } },
        { "Telstar", new HashSet<string> { "Telstar" } },
        { "Twente", new HashSet<string> { "Twente", "FC Twente" } },
        { "Utrecht", new HashSet<string> { "Utrecht", "FC Utrecht" } },
        { "Volendam", new HashSet<string> { "Volendam", "FC Volendam" } },
    };

    public static string FindName(string alias)
    {
        foreach (var kvp in Aliases)
        {
            if (kvp.Value.Contains(alias))
                return kvp.Key;
        }

        throw new Exception($"Team alias '{alias}' not found");
    }
}