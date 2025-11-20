public class GameConsoleView
{
    private const int DISPLAY_HISTORY_LIMIT = 10;
    private TranslationManager _translation;

    public GameConsoleView(TranslationManager translation)
    {
        _translation = translation;
    }

    public string ReadInput()
    {
        return Console.ReadLine();
    }

    public void WaitForAnyKey()
    {
        Console.ReadKey();
    }

    public void ClearScreen()
    {
        Console.Clear();
    }

    public void ShowRules()
    {
        ClearScreen();
        Console.WriteLine(_translation.GetText("GameTitle"));
        Console.WriteLine(_translation.GetText("Separator"));
        Console.WriteLine(_translation.GetText("RulesTitle"));
        Console.WriteLine(_translation.GetText("Rule1"));
        Console.WriteLine(_translation.GetText("Rule2"));
        Console.WriteLine(_translation.GetText("Rule3"));
        Console.WriteLine(_translation.GetText("Rule4"));
        Console.WriteLine(_translation.GetText("Separator") + "\n");
    }

    public void ShowGameHeader(string startWord, string player1Name, string player2Name)
    {
        Console.WriteLine(_translation.GetText("GameTitle"));
        string startWordDisplay = string.Format(_translation.GetText("CurrentStartWord"), startWord);
        Console.WriteLine(startWordDisplay);
        string playersInfo = string.Format(_translation.GetText("CurrentPlayers"), player1Name, player2Name);
        Console.WriteLine(playersInfo);
        Console.WriteLine(_translation.GetText("Separator"));
    }

    public void ShowTurnInfo(string currentPlayerName, int wordsUsedCount)
    {
        string playerTurn = string.Format(_translation.GetText("PlayerTurn"), currentPlayerName);
        Console.WriteLine(playerTurn);
        Console.WriteLine(_translation.GetText("TimeLimit"));

        string wordsUsed = string.Format(_translation.GetText("WordsUsedCount"), wordsUsedCount);
        Console.WriteLine(wordsUsed);
    }

    public void ShowUsedWords(HashSet<string> usedWords, string startWord)
    {
        if (usedWords.Count <= 1) return;

        Console.WriteLine(_translation.GetText("WordsHistory"));

        string[] allWords = new string[usedWords.Count];
        usedWords.CopyTo(allWords);

        int startIndex = 0;
        if (allWords.Length > DISPLAY_HISTORY_LIMIT)
        {
            startIndex = allWords.Length - DISPLAY_HISTORY_LIMIT;
        }

        int counter = 1;
        for (int i = startIndex; i < allWords.Length; i++)
        {
            if (allWords[i] != startWord)
            {
                Console.WriteLine(_translation.GetText("WordListFormat"), counter, allWords[i]);
                counter++;
            }
        }
    }

    public void ShowAllWords(HashSet<string> usedWords, string startWord)
    {
        Console.WriteLine(_translation.GetText("CommandShowWords"));

        string[] allWords = new string[usedWords.Count];
        usedWords.CopyTo(allWords);

        int counter = 1;
        for (int i = 0; i < allWords.Length; i++)
        {
            if (allWords[i] != startWord)
            {
                Console.WriteLine(_translation.GetText("WordListFormat"), counter, allWords[i]);
                counter++;
            }
        }
    }

    public void ShowCurrentScore(string player1Name, int player1Score, string player2Name, int player2Score)
    {
        string scoreMessage = string.Format(_translation.GetText("CommandScore"),
            player1Name, player1Score, player2Name, player2Score);
        Console.WriteLine(scoreMessage);
    }

    public void ShowTotalScore(string player1Name, int player1Wins, string player2Name, int player2Wins)
    {
        string totalScoreMessage = string.Format(_translation.GetText("CommandTotalScore"),
            player1Name, player1Wins, player2Name, player2Wins);
        Console.WriteLine(totalScoreMessage);
    }

    public void ShowGameOver(string message, string startWord, HashSet<string> usedWords, string winnerName)
    {
        Console.WriteLine(_translation.GetText("GameOver"));
        Console.WriteLine(_translation.GetText("Separator"));
        Console.WriteLine(message);

        string winnerMessage = string.Format(_translation.GetText("PlayerWins"), winnerName);
        Console.WriteLine(winnerMessage);

        Console.WriteLine(_translation.GetText("GameStatistics"));

        string startWordInfo = string.Format(_translation.GetText("CurrentStartWord"), startWord);
        Console.WriteLine(startWordInfo);

        int wordsUsedCount = usedWords.Count - 1;
        string totalWords = string.Format(_translation.GetText("TotalWordsNamed"), wordsUsedCount);
        Console.WriteLine(totalWords);

        if (usedWords.Count > 1)
        {
            Console.WriteLine(_translation.GetText("AllNamedWords"));
            string[] allWords = new string[usedWords.Count];
            usedWords.CopyTo(allWords);

            int counter = 1;
            for (int i = 0; i < allWords.Length; i++)
            {
                if (allWords[i] != startWord)
                {
                    Console.WriteLine(_translation.GetText("WordListFormat"), counter, allWords[i]);
                    counter++;
                }
            }
        }
    }

    public void ShowExitMessage(bool timeExpired)
    {
        if (timeExpired)
        {
            NativeConsole.AllocConsole();
        }
        Console.WriteLine(_translation.GetText("PressAnyKeyToExit"));
        Thread.Sleep(2000);
    }

    public void ShowMessage(string message)
    {
        Console.WriteLine(message);
    }

    public void ShowPrompt(string prompt)
    {
        Console.Write(prompt);
    }

    public void ShowUnknownCommand()
    {
        Console.WriteLine(_translation.GetText("UnknownCommand"));
    }
}