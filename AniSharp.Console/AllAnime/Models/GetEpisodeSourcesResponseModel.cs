namespace AniSharp.Console.AllAnime;

public class GetEpisodeSourcesResponseModel
{
    public class DataModel
    {
        public class EpisodeModel
        {
            public class SourceUrlModel
            {
                public string SourceUrl { get; set; }
                public float Priority { get; set; }
                public string SourceName { get; set; }
                public string Type { get; set; }
                public string StreamerId { get; set; }
            }
            
            public string EpisodeString { get; set; }
            public IEnumerable<SourceUrlModel> SourceUrls { get; set; }
        }

        public EpisodeModel Episode { get; set; }
    }
    
    public DataModel Data { get; set; }
}