using System.Text.Json.Serialization;

namespace AniSharp.Console.AllAnime;

public class SearchAnimeResponseModel
{
    public class DataType
    {
        public class ShowsType
        {
            public class EdgesType
            {
                [JsonPropertyName("_id")] 
                public string Id { get; set; }
                public string Name { get; set; }

                public class AvailableEpisodesType
                {
                    public int Sub { get; set; }
                    public int Dub { get; set; }
                    public int Raw { get; set; }
                }
                public AvailableEpisodesType AvailableEpisodes { get; set; }
                
                [JsonPropertyName("__typename")]
                public string Type { get; set; }
            }

            public IEnumerable<EdgesType> Edges { get; set; }
        }

        public ShowsType Shows { get; set; }
    }

    public DataType Data { get; set; }
}