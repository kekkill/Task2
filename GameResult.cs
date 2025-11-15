public class GameResult
{
    public string StartWord { get; set; }
    public HashSet<string> UsedWords { get; set; }
    public string WinnerMessage { get; set; }
    public string LoserMessage { get; set; }
    public int TotalWords { get; set; }
    public DateTime FinishedAt { get; set; }
}