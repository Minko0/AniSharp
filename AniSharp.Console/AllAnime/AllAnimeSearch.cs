namespace AniSharp.Console.AllAnime;

public class AllAnimeSearch
{
    public class SearchType
    {
        public bool AllowAdult { get; set; }
        public bool AllowUnknown { get; set; }
        public string Query { get; set; }
    }

    public SearchType Search { get; set; }
    public int Limit { get; set; }
    public int Page { get; set; }
    public string TranslationType { get; set; }
    public string CountryOfOrigin { get; set; }
}