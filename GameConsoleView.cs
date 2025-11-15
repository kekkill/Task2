public class GameConsoleView
{
    private const int DISPLAY_HISTORY_LIMIT = 10;
    private TranslationManager _translation;

    public GameConsoleView(TranslationManager translation)
    {
        _translation = translation;
    }

    public void ShowRules()
    {
        Console.Clear();
        Console.WriteLine(_translation.GetText("GameTitle"));
        Console.WriteLine(_translation.GetText("Separator"));
        Console.WriteLine(_translation.GetText("RulesTitle"));
        Console.WriteLine(_translation.GetText("Rule1"));
        Console.WriteLine(_translation.GetText("Rule2"));
        Console.WriteLine(_translation.GetText("Rule3"));
        Console.WriteLine(_translation.GetText("Rule4"));
        Console.WriteLine(_translation.GetText("Separator") + "\n");
    }

    public void ShowGameHeader(string startWord)
    {
        Console.WriteLine(_translation.GetText("GameTitle"));
        string startWordDisplay = string.Format(_translation.GetText("CurrentStartWord"), startWord);
        Console.WriteLine(startWordDisplay);
        Console.WriteLine(_translation.GetText("Separator"));
    }

    public void ShowTurnInfo(int currentPlayer, int wordsUsedCount)
    {
        string playerTurn = string.Format(_translation.GetText("PlayerTurn"), currentPlayer);
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

    public void ShowGameOver(string message, string startWord, HashSet<string> usedWords, int currentPlayer)
    {
        Console.WriteLine(_translation.GetText("GameOver"));
        Console.WriteLine(_translation.GetText("Separator"));
        Console.WriteLine(message);
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

        int winner = 0;
        if (currentPlayer == 1)
        {
            winner = 2;
        }
        else
        {
            winner = 1;
        }
        string winnerMessage = string.Format(_translation.GetText("Winner"), winner);
        Console.WriteLine(winnerMessage);
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
}