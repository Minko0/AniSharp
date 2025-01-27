using System.Net;
using System.Text.Json;
using AniSharp.Console.AllAnime;
using Bogus;
using Moq;
using Moq.Protected;

namespace AniSharp.Console.Tests.AllAnime;

public class AllAnimeClientTests
{
    [Fact]
    public async Task SearchAnimeHappyPath()
    {
        // Arrange
        var faker = new Faker<SearchAnimeResponseModel.DataType.ShowsType.EdgesType>()
            .RuleFor(e => e.Id, f => f.Lorem.Letter(10))
            .RuleFor(e => e.AvailableEpisodes, f => new SearchAnimeResponseModel.DataType.ShowsType.EdgesType.AvailableEpisodesType()
            {
                Sub = f.Random.Number(1, 120),
            })
            .RuleFor(e => e.Name, f => f.Name.FullName())
            .RuleFor(e => e.Type, f => f.PickRandom("Show", "Movie"));

        var query = "zenshuu.";
        var requestUri =
            "https://api.allanime.day/api?variables={\"search\":{\"allowAdult\":true,\"allowUnknown\":true,\"query\":\"zenshuu.\"},\"limit\":40,\"page\":1,\"translationType\":\"sub\",\"countryOfOrigin\":\"ALL\"}&query=\r\nquery(\r\n$search:SearchInput,\r\n$limit:Int,\r\n$page:Int,\r\n$translationType:VaildTranslationTypeEnumType,\r\n$countryOrigin:VaildCountryOriginEnumType\r\n){\r\nshows(\r\nsearch:$search,\r\nlimit:$limit,\r\npage:$page,\r\ntranslationType:$translationType,\r\ncountryOrigin:$countryOrigin\r\n){\r\nedges{\r\n_id\r\nname\r\navailableEpisodes\r\n__typename\r\n}\r\n}\r\n}";
        
        var response = new SearchAnimeResponseModel()
        {
            Data = new SearchAnimeResponseModel.DataType()
            {
                Shows = new SearchAnimeResponseModel.DataType.ShowsType()
                {
                    Edges = faker.Generate(5)
                }
            }
        };
        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });
        
        var handlerMock = CreateHttpHandlerMock(new StringContent(jsonResponse));

        var httpClient = new HttpClient(handlerMock.Object);
        var sut = new AllAnimeClient(httpClient);

        // Act
        var result = await sut.SearchAnime(query);
        
        // Assert
        handlerMock
            .Protected()
            .Verify(
                "SendAsync", 
                Times.Exactly(1), 
                ItExpr.Is<HttpRequestMessage>(m => ValidateRequestMessage(m, requestUri)), 
                ItExpr.IsAny<CancellationToken>()
            );

        Assert.True(result.HasValue);
        Assert.Equivalent(result.Value, response);
    }

    [Fact]
    public async Task SearchAnimeNotSuccessful()
    {
        // Arrange
        const string query = "Sousou no frieren";
        var handlerMock = CreateHttpHandlerMock(new StringContent("Server error"), HttpStatusCode.InternalServerError);
        var httpClient = new HttpClient(handlerMock.Object);
        var sut = new AllAnimeClient(httpClient);
        
        // Act
        var result = await sut.SearchAnime(query);
        
        // Assert
        Assert.False(result.HasValue);
    }

    [Fact]
    public async Task GetLinksFromSourceUrlHappyPath()
    {
        // Arrange
        var sourceUrl = "--" + Convert.ToHexString([23]) + new Faker().Random.Hexadecimal(20, "");

        var faker = new Faker<LinksFromSourceUrlResponseModel.LinkType>()
            .RuleFor(l => l.Link, f => f.Internet.Url())
            .RuleFor(l => l.ResolutionStr, "Mp4");
        var response = new LinksFromSourceUrlResponseModel()
        {
            Links = faker.Generate(5),
        };
        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        var handlerMock = CreateHttpHandlerMock(new StringContent(jsonResponse));
        var httpClient = new HttpClient(handlerMock.Object);
        var sut = new AllAnimeClient(httpClient);

        // Act
        var result = await sut.GetLinksFromSourceUrl(sourceUrl);
        
        // Assert
        handlerMock
            .Protected()
            .Verify(
                "SendAsync", 
                Times.Exactly(1), 
                ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<CancellationToken>()
            );
        
        Assert.Equivalent(result, response);
    }
    
    [Fact]
    public async Task GetLinksFromSourceUrlSourceUrlWithoutDashThrowsException()
    {
        // Arrange
        var sourceUrl = "--" + new Faker().Random.Hexadecimal(20, "");
        
        var handlerMock = CreateHttpHandlerMock(new StringContent(""));
        var httpClient = new HttpClient(handlerMock.Object);
        var sut = new AllAnimeClient(httpClient);

        // Act
        var act = () => sut.GetLinksFromSourceUrl(sourceUrl);
        
        // Assert
        await Assert.ThrowsAsync<NotSupportedException>(act);
    }
    
    private static Mock<HttpMessageHandler> CreateHttpHandlerMock(HttpContent content, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = statusCode,
                Content = content
            })
            .Verifiable();
        return handlerMock;
    }

    private bool ValidateRequestMessage(HttpRequestMessage message, string? requestUri = null)
    {
        if (requestUri == null)
        {
            return message.Method == HttpMethod.Get && message.Headers.Referrer == new Uri("allanime.to");
        }
        
        return message.Method == HttpMethod.Get && 
               message.RequestUri!.ToString().Replace(" ", "").Equals(requestUri.Replace(" ", "")); // Remove all whitespace
    }
}