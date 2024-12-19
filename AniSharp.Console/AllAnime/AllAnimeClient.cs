using System.Text;
using System.Text.Json;

namespace AniSharp.Console.AllAnime;

public class AllAnimeClient
{
    private readonly HttpClient _client;
    private const string BaseUrl = "https://api.allanime.day/api";

    public AllAnimeClient()
    {
        _client = new HttpClient();
        _client.BaseAddress = new Uri(BaseUrl);
    }
    
    public async Task<AllAnimeSearchResponse?> SearchAnime(string anime)
    {
        var query = new AllAnimeSearch()
        {
            Search = new AllAnimeSearch.SearchType()
            {
                AllowAdult = true,
                AllowUnknown = true,
                Query = anime
            },
            Limit = 40,
            Page = 1,
            TranslationType = "sub",
            CountryOfOrigin = "ALL",
        };

        var jsonString = JsonSerializer.Serialize(query, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });
        
        const string graphQlQuery = @"
        query(
            $search: SearchInput,
            $limit: Int,
            $page: Int,
            $translationType: VaildTranslationTypeEnumType,
            $countryOrigin: VaildCountryOriginEnumType
        ) {
            shows(
                search: $search,
                limit: $limit,
                page: $page,
                translationType: $translationType,
                countryOrigin: $countryOrigin
            ) {
                edges {
                    _id
                    name
                    availableEpisodes
                    __typename
                }
            }
        }";
        var url = new StringBuilder(BaseUrl)
            .Append($"?variables={jsonString}")
            .Append($"&query={graphQlQuery}")
            .ToString();
        
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Referrer = new Uri("https://allmanga.to/");
        
        var response = await _client.SendAsync(request);
        var responseText = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<AllAnimeSearchResponse>(responseText, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });
        
        return result;
    }

    public async Task<GetEpisodeSourcesResponse?> GetEpisodeSources(string showId, int episode)
    {
        var query = new GetEpisodeUrlModel()
        {
            ShowId = showId,
            EpisodeString = episode.ToString(),
            TranslationType = "sub",
        };
        
        var jsonString = JsonSerializer.Serialize(query, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });
        
        const string graphQlQuery = @"
        query(
            $showId: String!, 
            $translationType: VaildTranslationTypeEnumType!, 
            $episodeString: String!
        ) {    
            episode(        
                showId: $showId        
                translationType: $translationType        
                episodeString: $episodeString    
        ) {        
            episodeString sourceUrls    }
        }";
        
        var url = new StringBuilder(BaseUrl)
            .Append($"?variables={jsonString}")
            .Append($"&query={graphQlQuery}")
            .ToString();
        
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Referrer = new Uri("https://allmanga.to/");
        
        var response = await _client.SendAsync(request);
        var responseText = await response.Content.ReadAsStringAsync();
        
        var result = JsonSerializer.Deserialize<GetEpisodeSourcesResponse>(responseText, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        return result;
    }

    public async Task<LinksFromSourceUrlResponse?> GetLinksFromSourceUrl(string sourceUrl)
    {
        if (sourceUrl.StartsWith("--"))
        {
            sourceUrl = OneDigitSymmetricXor(56, sourceUrl[2..]);
        }

        if (!sourceUrl.StartsWith($"/"))
        {
            throw new NotSupportedException("Type of source url not supported");
        }

        sourceUrl = sourceUrl.Replace("clock", "clock.json");
        
        
        var url = new StringBuilder("https://allanime.day")
            .Append(sourceUrl)
            .ToString();
        
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Referrer = new Uri("https://allmanga.to/");
        
        var response = await _client.SendAsync(request);
        var responseText = await response.Content.ReadAsStringAsync();
        
        var result = JsonSerializer.Deserialize<LinksFromSourceUrlResponse>(responseText, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        return result;
    }

    private static string OneDigitSymmetricXor(int password, string target)
    {
        var targetBytes = Convert.FromHexString(target);
        
        for (var i = 0; i < targetBytes.Length; i++)
        {
            targetBytes[i] ^= (byte)password;
        }
        
        return Encoding.UTF8.GetString(targetBytes);
    }
    
}