using AniSharp.Console.Models;

namespace AniSharp.Console.History;

public class HistoryManager
{
    private string AppDataFolder { get; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/AniSharp";
    private const string HistoryFileName = "history.txt";

    private string HistoryFile => AppDataFolder + "/" + HistoryFileName;

    public void ClearHistory()
    {
        if (File.Exists(HistoryFile))
        {
            File.WriteAllText(HistoryFile, string.Empty);
        }
    }

    public void AddToHistory(string showId, string showName, int episode)
    {
        if (!File.Exists(HistoryFile))
        {
            if (!Directory.Exists(AppDataFolder))
            {
                Directory.CreateDirectory(AppDataFolder);
            }

            File.Create(HistoryFile);
        }

        var lines = File.ReadAllLines(HistoryFile).ToList();
        var showLine = lines.FirstOrDefault(l => l.StartsWith(showId));

        ShowHistory history;
        if (showLine != null)
        {
            lines.Remove(showLine);
            history = ShowHistory.ParseFromString(showLine);
            if (!history.WatchedEpisodes.Contains(episode))
            {
                history.WatchedEpisodes.Add(episode);
                history.WatchedEpisodes.Sort();
            }
        }
        else
        {
            history = new ShowHistory()
            {
                ShowId = showId,
                ShowName = showName,
                WatchedEpisodes = [episode],
            };
        }
        
        lines.Insert(0, history.SerializeToString());
        File.WriteAllLines(HistoryFile, lines);
    }
}