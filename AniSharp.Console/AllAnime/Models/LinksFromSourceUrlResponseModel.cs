namespace AniSharp.Console.AllAnime;

public class LinksFromSourceUrlResponseModel
{
    public class LinkType
    {
        public string Link { get; set; }
    }

    public IEnumerable<LinkType> Links { get; set; }
}