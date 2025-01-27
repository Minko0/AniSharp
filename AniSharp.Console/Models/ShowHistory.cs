namespace AniSharp.Console.Models;

public class ShowHistory
{
    public string ShowName { get; set; }
    public string ShowId { get; set; }
    public List<int> WatchedEpisodes { get; set; }

    public static ShowHistory ParseFromString(string line)
    {
        var splits = line.Split("---");
        var result = new ShowHistory()
        {
            ShowId = splits[0],
            ShowName = splits[1],
            WatchedEpisodes = splits[2].Split(",").Select(int.Parse).ToList()
        };
        return result;
    }

    public string SerializeToString()
    {
        return $"{ShowId}---{ShowName}---{string.Join(',', WatchedEpisodes)}";
    }
}