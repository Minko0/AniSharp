using System.Diagnostics;
using System.Text.RegularExpressions;
using AniSharp.Console.AllAnime;
using Spectre.Console;
using static AniSharp.Console.AllAnime.SearchAnimeResponseModel.DataType;

var allAnimeClient = new AllAnimeClient();

var results = new List<ShowsType.EdgesType>();

var query = "";
while (results.Count == 0)
{
    AnsiConsole.Clear();
    AnsiConsole.Write(new FigletText("AniSharp").LeftJustified().Color(Color.Blue));
    if (query != string.Empty)
    {
        AnsiConsole.Write(new Text($"Couldn't find anime '{query}'\n", new Style(Color.Orange1)));
    }
    query = AnsiConsole.Prompt(new TextPrompt<string>("Search anime:"));
    var result = await allAnimeClient.SearchAnime(query);

    results = result.Data.Shows.Edges.ToList();
}

AnsiConsole.Clear();
var selectShowPrompt = new SelectionPrompt<ShowsType.EdgesType>()
    .Title("Select show:")
    .AddChoices(results);

if (results.Count >= 3)
{
    selectShowPrompt.PageSize(results.Count);
}

selectShowPrompt.Converter = type => $"{type.Name} ({type.AvailableEpisodes.Sub} episodes)";

var episode = AnsiConsole.Prompt(selectShowPrompt);

AnsiConsole.Clear();
var selectEpisodePrompt = new SelectionPrompt<int>()
    .Title("Select episode:");

if (episode.AvailableEpisodes.Sub >= 3)
{
    selectShowPrompt.PageSize(episode.AvailableEpisodes.Sub);
}

for (var i = 1; i <= episode.AvailableEpisodes.Sub; i++)
{
    selectEpisodePrompt.AddChoice(i);
}
var episodeNumber = AnsiConsole.Prompt(selectEpisodePrompt);
var sources = await allAnimeClient.GetEpisodeSources(episode.Id, episodeNumber);

var validSourceNames = new[]
{
    "Default",
    "Sak",
    "Kir",
    "S-mp4",
    "Luf-mp4"
};
var validSources = sources.Data.Episode.SourceUrls
    .Where(source => validSourceNames.Contains(source.SourceName))
    .ToList();

var links = new List<LinksFromSourceUrlResponseModel.LinkType>();
foreach (var source in validSources)
{
    try
    {
        var urls = await allAnimeClient.GetLinksFromSourceUrl(source.SourceUrl);
        links.AddRange(urls.Links.Where(l => l.Link != null).ToList());
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
}

var GetResolutionPriority = (string type) =>
{
    return type switch
    {
        "Mp4" => 1,
        "hls" => 2,
        "hls P" => 3,
        "Alt" => 4,
        _ => 5
    };
};
links.Sort((a, b) => GetResolutionPriority(a.ResolutionStr).CompareTo(GetResolutionPriority(b.ResolutionStr)));

if (links.Count > 0)
{
    var url = links.First();

    if (url.Link != null && url.Link.Contains("repackager.wixmp.com"))
    {
        var pattern = @"([^/]*)/mp4";
        var match = Regex.Match(url.Link, pattern);
        var resolutions = Array.Empty<string>();
        var group = string.Empty;
        if (match.Success)
        { 
            group = match.Groups[0].Value.Replace("/mp4", "");
            resolutions = group.Split(",", StringSplitOptions.RemoveEmptyEntries);
        }

        url.Link = url.Link.Replace("repackager.wixmp.com/", "").Replace(group, resolutions.First());
        url.Link = url.Link.Remove(url.Link.IndexOf(".urlset", StringComparison.Ordinal));
    }

    var process = new Process()
    {
        StartInfo = new ProcessStartInfo()
        {
            FileName = "mpv",
            Arguments = url.Link
        }
    };
    process.Start();
    process.WaitForExit();
}