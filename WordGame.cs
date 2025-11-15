using System.Text;
using System.Text.Json;
public class WordGame
{
    private const string SessionFilePath = "game_session.json";
    private const string ResultsFilePath = "game_results.json";
    private const int TIME_LIMIT_MS = 15000;

    private WordValidator _wordValidator;
    private TranslationManager _translation;
    private GameTimer _timer;
    private GameConsoleView _view;
    private HashSet<string> _usedWords;

    private int _currentPlayer;
    private bool _gameFinished;
    private bool _timeExpired;
    public string StartWord;

    public WordGame()
    {
        _wordValidator = new WordValidator();
        _translation = new TranslationManager();
        _timer = new GameTimer(TIME_LIMIT_MS);
        _view = new GameConsoleView(_translation);
        _usedWords = new HashSet<string>();
        _currentPlayer = 1;
        _timer.OnTimeExpired = OnTimeExpired;
    }

    public void Start()
    {
        ChooseLanguage();

        if (File.Exists(SessionFilePath))
        {
            FinalizeInterruptedGame();
        }

        SetupNewGame();

        while (_gameFinished == false)
        {
            PlayRound();
        }

        _view.ShowExitMessage(_timeExpired);
    }

    private void FinalizeInterruptedGame()
    {
        try
        {
            UTF8Encoding encoding = new UTF8Encoding(true);
            string json = File.ReadAllText(SessionFilePath, encoding);
            GameState session = JsonSerializer.Deserialize<GameState>(json);

            if (session != null)
            {
                if (session.GameFinished == false)
                {
                    string langToUse = "ru-RU";
                    if (session.Language != null)
                    {
                        langToUse = session.Language;
                    }

                    _translation.SetLanguage(langToUse);
                    _wordValidator.CurrentCulture = _translation.CurrentCulture;

                    StartWord = session.StartWord;
                    if (StartWord == null)
                    {
                        StartWord = "";
                    }

                    _usedWords = new HashSet<string>();
                    if (session.UsedWords != null)
                    {
                        string[] wordsArray = new string[session.UsedWords.Count];
                        session.UsedWords.CopyTo(wordsArray);
                        for (int i = 0; i < wordsArray.Length; i++)
                        {
                            _usedWords.Add(wordsArray[i]);
                        }
                    }

                    int interruptedPlayer = session.CurrentPlayer;
                    int winnerPlayer = 1;
                    if (interruptedPlayer == 1)
                    {
                        winnerPlayer = 2;
                    }

                    string winnerMessage = _translation.GetText("Winner");
                    winnerMessage = string.Format(winnerMessage, winnerPlayer);

                    string loserMessage = _translation.GetText("PlayerLostSimple");
                    loserMessage = string.Format(loserMessage, interruptedPlayer);

                    GameResult result = new GameResult();
                    result.StartWord = StartWord;
                    result.UsedWords = new HashSet<string>();
                    if (_usedWords.Count > 0)
                    {
                        string[] wordsArray = new string[_usedWords.Count];
                        _usedWords.CopyTo(wordsArray);
                        for (int i = 0; i < wordsArray.Length; i++)
                        {
                            result.UsedWords.Add(wordsArray[i]);
                        }
                    }
                    result.WinnerMessage = winnerMessage;
                    result.LoserMessage = loserMessage;
                    result.TotalWords = _usedWords.Count - 1;
                    result.FinishedAt = DateTime.Now;

                    SaveResultToHistory(result);

                    Console.Clear();
                    string msg = _translation.GetText("GameWasInterrupted");
                    _view.ShowMessage(msg);
                    _view.ShowMessage(winnerMessage);
                    _view.ShowMessage(_translation.GetText("PressAnyKeyToContinue"));
                    Console.ReadKey();
                }
            }
        }
        catch
        {
        }
        finally
        {
            if (File.Exists(SessionFilePath))
            {
                File.Delete(SessionFilePath);
            }
        }
    }

    private void ChooseLanguage()
    {
        _view.ShowMessage(_translation.GetText("ChooseLanguage"));
        _view.ShowMessage(_translation.GetText("LanguageOption1"));
        _view.ShowMessage(_translation.GetText("LanguageOption2"));
        _view.ShowPrompt(_translation.GetText("LanguageChoice"));

        string choice = Console.ReadLine();
        if (choice == "1")
        {
            _translation.SetLanguage("en-US");
        }
        else
        {
            _translation.SetLanguage("ru-RU");
        }
        _wordValidator.CurrentCulture = _translation.CurrentCulture;
    }

    private void SetupNewGame()
    {
        _usedWords.Clear();
        _currentPlayer = 1;
        _gameFinished = false;
        SetupGame();
    }

    private void SetupGame()
    {
        bool validWord = false;

        while (validWord == false)
        {
            _view.ShowPrompt(_translation.GetText("EnterStartWord"));
            string inputWord = Console.ReadLine();
            if (inputWord != null)
            {
                inputWord = inputWord.ToLower().Trim();
            }
            else
            {
                inputWord = "";
            }

            CheckResult result = _wordValidator.CheckStartWord(inputWord);

            if (result.IsValid == true)
            {
                StartWord = inputWord;
                _wordValidator.StartWord = inputWord;
                _usedWords.Add(StartWord);
                validWord = true;

                string confirmation = _translation.GetText("StartWordSet");
                confirmation = string.Format(confirmation, StartWord);
                _view.ShowMessage(confirmation);
            }
            else
            {
                string errorMessage = _translation.GetText(result.ErrorKey);
                _view.ShowMessage(errorMessage);
            }
        }

        _view.ShowMessage(_translation.GetText("PressAnyKeyToStart"));
        Console.ReadKey();
    }

    private void PlayRound()
    {
        SaveActiveSession();

        Console.Clear();
        _view.ShowGameHeader(StartWord);
        _timeExpired = false;

        int wordsUsedCount = _usedWords.Count - 1;
        _view.ShowTurnInfo(_currentPlayer, wordsUsedCount);
        _view.ShowUsedWords(_usedWords, StartWord);

        string inputPrompt = _translation.GetText("EnterWordPrompt");
        inputPrompt = string.Format(inputPrompt, _currentPlayer);
        _view.ShowPrompt(inputPrompt);

        _timer.Start();
        string word = Console.ReadLine();
        if (word != null)
        {
            word = word.ToLower().Trim();
        }
        else
        {
            word = "";
        }
        _timer.Stop();

        if (_timeExpired == true)
        {
            string message = _translation.GetText("TimeExpired");
            message = string.Format(message, _currentPlayer);
            EndGame(message);
        }
        else if (word == "")
        {
            string errorMessage = _translation.GetText("ErrorEmptyWord");
            string message = _translation.GetText("PlayerLost");
            message = string.Format(message, _currentPlayer, errorMessage);
            EndGame(message);
        }
        else
        {
            ProcessWord(word);
        }
    }

    private void ProcessWord(string word)
    {
        CheckResult result = _wordValidator.CheckPlayerWord(word, _usedWords);

        if (result.IsValid == true)
        {
            _usedWords.Add(word);
            string successMessage = _translation.GetText("WordAccepted");
            successMessage = string.Format(successMessage, word);
            _view.ShowMessage(successMessage);

            if (_currentPlayer == 1)
            {
                _currentPlayer = 2;
            }
            else
            {
                _currentPlayer = 1;
            }

            _view.ShowMessage(_translation.GetText("PressAnyKeyToContinue"));
            Console.ReadKey();
        }
        else
        {
            string errorMessage = _translation.GetText(result.ErrorKey);
            if (result.ErrorKey == "ErrorInvalidLetters")
            {
                string template = _translation.GetText("ErrorInvalidLetters");
                errorMessage = string.Format(template, StartWord);
            }

            string message = _translation.GetText("PlayerLost");
            message = string.Format(message, _currentPlayer, errorMessage);
            EndGame(message);
        }
    }

    private void OnTimeExpired()
    {
        if (_timeExpired == false && _gameFinished == false)
        {
            _timeExpired = true;
            _view.ShowMessage("");
            _view.ShowMessage(_translation.GetText("TimeExpired"));

            IntPtr stdin = NativeConsole.GetStdHandle(NativeConsole.StdHandle.Stdin);
            NativeConsole.CloseHandle(stdin);
        }
    }

    private void EndGame(string message)
    {
        _timer.Stop();
        _gameFinished = true;

        int winnerPlayer = 1;
        if (_currentPlayer == 1)
        {
            winnerPlayer = 2;
        }

        string winnerMessage = _translation.GetText("Winner");
        winnerMessage = string.Format(winnerMessage, winnerPlayer);

        string loserMessage = _translation.GetText("PlayerLostSimple");
        loserMessage = string.Format(loserMessage, _currentPlayer);

        GameResult result = new GameResult();
        result.StartWord = StartWord;
        result.UsedWords = new HashSet<string>();
        if (_usedWords.Count > 0)
        {
            string[] wordsArray = new string[_usedWords.Count];
            _usedWords.CopyTo(wordsArray);
            for (int i = 0; i < wordsArray.Length; i++)
            {
                result.UsedWords.Add(wordsArray[i]);
            }
        }
        result.WinnerMessage = winnerMessage;
        result.LoserMessage = loserMessage;
        result.TotalWords = _usedWords.Count - 1;
        result.FinishedAt = DateTime.Now;

        SaveResultToHistory(result);

        if (File.Exists(SessionFilePath))
        {
            File.Delete(SessionFilePath);
        }

        Console.Clear();
        _view.ShowGameOver(message, StartWord, _usedWords, _currentPlayer);
    }

    private void SaveActiveSession()
    {
        GameState session = new GameState();
        session.StartWord = StartWord;
        session.UsedWords = new HashSet<string>();
        if (_usedWords.Count > 0)
        {
            string[] wordsArray = new string[_usedWords.Count];
            _usedWords.CopyTo(wordsArray);
            for (int i = 0; i < wordsArray.Length; i++)
            {
                session.UsedWords.Add(wordsArray[i]);
            }
        }
        session.CurrentPlayer = _currentPlayer;
        session.GameFinished = false;
        session.GameStarted = DateTime.Now;
        session.Language = _translation.CurrentCulture.Name;

        JsonSerializerOptions options = new JsonSerializerOptions();
        options.WriteIndented = true;
        options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

        string json = JsonSerializer.Serialize(session, options);
        UTF8Encoding encoding = new UTF8Encoding(true);
        File.WriteAllText(SessionFilePath, json, encoding);
    }

    private void SaveResultToHistory(GameResult result)
    {
        List<GameResult> results = new List<GameResult>();
        if (File.Exists(ResultsFilePath))
        {
            try
            {
                UTF8Encoding encoding = new UTF8Encoding(true);
                string json = File.ReadAllText(ResultsFilePath, encoding);
                results = JsonSerializer.Deserialize<List<GameResult>>(json);
                if (results == null)
                {
                    results = new List<GameResult>();
                }
            }
            catch
            {
                results = new List<GameResult>();
            }
        }

        results.Add(result);

        JsonSerializerOptions options = new JsonSerializerOptions();
        options.WriteIndented = true;
        options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

        string jsonOut = JsonSerializer.Serialize(results, options);
        UTF8Encoding encodingOut = new UTF8Encoding(true);
        File.WriteAllText(ResultsFilePath, jsonOut, encodingOut);
    }
}