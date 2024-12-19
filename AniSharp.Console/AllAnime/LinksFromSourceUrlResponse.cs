namespace AniSharp.Console.AllAnime;

public class LinksFromSourceUrlResponse
{
    public class LinkType
    {
        public string Link { get; set; }
    }

    public IEnumerable<LinkType> Links { get; set; }
}