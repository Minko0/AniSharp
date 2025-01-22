using System.Diagnostics;
using System.Text.RegularExpressions;
using AniSharp.Console.AllAnime;
using Spectre.Console;

namespace AniSharp.Console;

public class AniSharpProgramSteps
{
    private readonly AllAnimeClient _allAnimeClient = new();
    
    public async Task<List<SearchAnimeResponseModel.DataType.ShowsType.EdgesType>> SearchAnime()
    {
        var results = new List<SearchAnimeResponseModel.DataType.ShowsType.EdgesType>();

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
            var result = await _allAnimeClient.SearchAnime(query);

            if (result != null)
            {
                results = result.Data.Shows.Edges.ToList();
            }
        }

        return results;
    }
    
    public SearchAnimeResponseModel.DataType.ShowsType.EdgesType SelectShow(List<SearchAnimeResponseModel.DataType.ShowsType.EdgesType> results)
    {
        AnsiConsole.Clear();
        var selectShowPrompt = new SelectionPrompt<SearchAnimeResponseModel.DataType.ShowsType.EdgesType>()
            .Title("Select show:")
            .AddChoices(results);

        if (results.Count >= 3)
        {
            selectShowPrompt.PageSize(results.Count);
        }

        selectShowPrompt.Converter = type => $"{type.Name} ({type.AvailableEpisodes.Sub} episodes)";
        return AnsiConsole.Prompt(selectShowPrompt);
    }
    
    public int SelectEpisode(SearchAnimeResponseModel.DataType.ShowsType.EdgesType show)
    {
        AnsiConsole.Clear();
        var selectEpisodePrompt = new SelectionPrompt<int>()
            .Title("Select episode:");

        if (show.AvailableEpisodes.Sub >= 3)
        {
            selectEpisodePrompt.PageSize(show.AvailableEpisodes.Sub);
        }

        for (var i = 1; i <= show.AvailableEpisodes.Sub; i++)
        {
            selectEpisodePrompt.AddChoice(i);
        }

        return AnsiConsole.Prompt(selectEpisodePrompt);
    }
    
    public async Task<List<LinksFromSourceUrlResponseModel.LinkType>> GetAnimeLinksForEpisode(SearchAnimeResponseModel.DataType.ShowsType.EdgesType show, int episodeNumber)
    {
        var sources = await _allAnimeClient.GetEpisodeSources(show.Id, episodeNumber);

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

        var list = new List<LinksFromSourceUrlResponseModel.LinkType>();
        foreach (var source in validSources)
        {
            try
            {
                var urls = await _allAnimeClient.GetLinksFromSourceUrl(source.SourceUrl);
                list.AddRange(urls.Links.Where(l => l.Link != null).ToList());
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
            }
        }

        return list;
    }

    
    public void PlayAnime(List<LinksFromSourceUrlResponseModel.LinkType> linkTypes)
    {
        linkTypes.Sort((a, b) => GetResolutionPriority(a.ResolutionStr).CompareTo(GetResolutionPriority(b.ResolutionStr)));

        if (linkTypes.Count > 0)
        {
            var url = linkTypes.First();

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

        return;

        int GetResolutionPriority(string type)
        {
            return type switch
            {
                "Mp4" => 1,
                "hls" => 2,
                "hls P" => 3,
                "Alt" => 4,
                _ => 5
            };
        }
    }
}