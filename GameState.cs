public class GameState
{
    public string StartWord { get; set; }
    public HashSet<string> UsedWords { get; set; }
    public int CurrentPlayerIndex { get; set; }
    public bool GameFinished { get; set; }
    public DateTime GameStarted { get; set; }
    public string Language { get; set; }
    public List<Player> Players { get; set; }
}