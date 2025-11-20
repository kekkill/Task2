public class Player
{
    public string Name { get; set; }
    public int Score { get; set; }
    public int TotalWins { get; set; }

    public Player(string name)
    {
        Name = name;
        Score = 0;
        TotalWins = 0;
    }
}