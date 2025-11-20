using System.Text;
using System.Text.Json;

public class GameSessionStorage
{
    private const string SessionFilePath = "game_session.json";
    private const string ResultsFilePath = "game_results.json";
    private const string PlayersStatsPath = "players_stats.json";

    private readonly JsonSerializerOptions _jsonOptions;

    public GameSessionStorage()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }

    public void SaveSession(GameState session)
    {
        try
        {
            string json = JsonSerializer.Serialize(session, _jsonOptions);
            UTF8Encoding encoding = new UTF8Encoding(true);
            File.WriteAllText(SessionFilePath, json, encoding);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to save game session", ex);
        }
    }

    public GameState LoadSession()
    {
        if (!File.Exists(SessionFilePath))
        {
            return null;
        }

        try
        {
            UTF8Encoding encoding = new UTF8Encoding(true);
            string json = File.ReadAllText(SessionFilePath, encoding);
            return JsonSerializer.Deserialize<GameState>(json);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Game session file is corrupted", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to load game session", ex);
        }
    }

    public void DeleteSession()
    {
        if (File.Exists(SessionFilePath))
        {
            File.Delete(SessionFilePath);
        }
    }

    public void SaveResult(GameResult result)
    {
        try
        {
            List<GameResult> results = LoadResultsHistory();
            results.Add(result);

            string json = JsonSerializer.Serialize(results, _jsonOptions);
            UTF8Encoding encoding = new UTF8Encoding(true);
            File.WriteAllText(ResultsFilePath, json, encoding);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to save game result", ex);
        }
    }

    public List<GameResult> LoadResultsHistory()
    {
        if (!File.Exists(ResultsFilePath))
        {
            return new List<GameResult>();
        }

        try
        {
            UTF8Encoding encoding = new UTF8Encoding(true);
            string json = File.ReadAllText(ResultsFilePath, encoding);
            return JsonSerializer.Deserialize<List<GameResult>>(json) ?? new List<GameResult>();
        }
        catch (JsonException)
        {
            return new List<GameResult>();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to load game results history", ex);
        }
    }

    public void SavePlayersStats(List<Player> players)
    {
        try
        {
            string json = JsonSerializer.Serialize(players, _jsonOptions);
            UTF8Encoding encoding = new UTF8Encoding(true);
            File.WriteAllText(PlayersStatsPath, json, encoding);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to save players statistics", ex);
        }
    }

    public List<Player> LoadPlayersStats()
    {
        if (!File.Exists(PlayersStatsPath))
        {
            return new List<Player>();
        }

        try
        {
            UTF8Encoding encoding = new UTF8Encoding(true);
            string json = File.ReadAllText(PlayersStatsPath, encoding);
            return JsonSerializer.Deserialize<List<Player>>(json) ?? new List<Player>();
        }
        catch (JsonException)
        {
            return new List<Player>();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to load players statistics", ex);
        }
    }
}