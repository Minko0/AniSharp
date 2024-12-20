using System.Diagnostics;
using AniSharp.Console.AllAnime;

var allAnimeClient = new AllAnimeClient();
var result = await allAnimeClient.SearchAnime("natsume yuujinchou");

if (result != null)
{
    var episode = result.Data.Shows.Edges.ToList()[0];
    var sources = await allAnimeClient.GetEpisodeSources(episode.Id, 1);

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
}