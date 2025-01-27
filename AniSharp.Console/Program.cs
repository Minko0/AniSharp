using AniSharp.Console;
using AniSharp.Console.History;

var arguments = Environment.GetCommandLineArgs();
var historyManager = new HistoryManager();

if (arguments.Length == 1)
{
    await SearchAndPlay();
    return;
}

switch (arguments[1])
{
    case "--clear" or "-c":
        historyManager.ClearHistory();
        break;
}

return;


async Task SearchAndPlay()
{
    var steps = new AniSharpProgramSteps();

    var results = await steps.SearchAnime();
    var show = steps.SelectShow(results);
    var episodeNumber = steps.SelectEpisode(show);
    var links = await steps.GetAnimeLinksForEpisode(show, episodeNumber);

    steps.PlayAnime(show, episodeNumber, links);
}