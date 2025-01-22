using AniSharp.Console;

var steps = new AniSharpProgramSteps();

var results = await steps.SearchAnime();
var show = steps.SelectShow(results);
var episodeNumber = steps.SelectEpisode(show);
var links = await steps.GetAnimeLinksForEpisode(show, episodeNumber);

steps.PlayAnime(links);