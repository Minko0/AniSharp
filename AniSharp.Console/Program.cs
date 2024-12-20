using System.Diagnostics;
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
    .PageSize(results.Count)
    .AddChoices(results);
selectShowPrompt.Converter = type => $"{type.Name} ({type.AvailableEpisodes.Sub} episodes)";

var episode = AnsiConsole.Prompt(selectShowPrompt);

AnsiConsole.Clear();
var selectEpisodePrompt = new SelectionPrompt<int>()
    .Title("Select episode:")
    .PageSize(episode.AvailableEpisodes.Sub);

for (var i = 1; i <= episode.AvailableEpisodes.Sub; i++)
{
    selectEpisodePrompt.AddChoice(i);
}
var episodeNumber = AnsiConsole.Prompt(selectEpisodePrompt);
var sources = await allAnimeClient.GetEpisodeSources(episode.Id, episodeNumber);
var rightSource = sources.Data.Episode.SourceUrls.First(s => s.SourceName == "S-mp4");
var urls = await allAnimeClient.GetLinksFromSourceUrl(rightSource.SourceUrl);

if (urls != null)
{
    var url = urls.Links.First();

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