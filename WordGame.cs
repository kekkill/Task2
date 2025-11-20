public class WordGame
{
    private const int TIME_LIMIT_MS = 15000;

    private WordValidator _wordValidator;
    private TranslationManager _translation;
    private GameTimer _timer;
    private GameConsoleView _view;
    private GameSessionStorage _storage;
    private HashSet<string> _usedWords;

    private List<Player> _players;
    private int _currentPlayerIndex;
    private bool _gameFinished;
    private bool _timeExpired;
    public string StartWord;

    public WordGame()
    {
        _wordValidator = new WordValidator();
        _translation = new TranslationManager();
        _timer = new GameTimer(TIME_LIMIT_MS);
        _view = new GameConsoleView(_translation);
        _storage = new GameSessionStorage();
        _usedWords = new HashSet<string>();
        _players = new List<Player>();
        _currentPlayerIndex = 0;
        _timer.OnTimeExpired = OnTimeExpired;
    }

    public void Start()
    {
        ChooseLanguage();

        AskForPlayers();

        if (TryRestoreInterruptedGame())
        {
            return;
        }

        SetupNewGame();

        while (_gameFinished == false)
        {
            PlayRound();
        }

        _view.ShowExitMessage(_timeExpired);
    }

    private void AskForPlayers()
    {
        var savedPlayers = _storage.LoadPlayersStats();

        if (savedPlayers.Count >= 2)
        {
            _view.ShowPrompt(_translation.GetText("UseSavedPlayers"));
            string choice = _view.ReadInput().ToLower().Trim();

            if (choice == "y" || choice == "yes" || choice == "д" || choice == "да")
            {
                _players = savedPlayers;
                _view.ShowMessage(string.Format(_translation.GetText("CurrentPlayers"),
                    _players[0].Name, _players[1].Name));
                return;
            }
        }

        SetupNewPlayers();
    }

    private void SetupNewPlayers()
    {
        _players.Clear();

        for (int i = 1; i <= 2; i++)
        {
            _view.ShowPrompt(string.Format(_translation.GetText("EnterNewPlayerName"),
                string.Format(_translation.GetText("PlayerName"), i)));
            string name = _view.ReadInput().Trim();

            if (string.IsNullOrEmpty(name))
            {
                name = string.Format(_translation.GetText("PlayerName"), i);
            }

            var savedPlayers = _storage.LoadPlayersStats();
            var existingPlayer = savedPlayers.FirstOrDefault(p => p.Name == name);

            if (existingPlayer != null)
            {
                _players.Add(existingPlayer);
                _view.ShowMessage($"Found existing player: {name} (Wins: {existingPlayer.TotalWins})");
            }
            else
            {
                _players.Add(new Player(name));
            }
        }

        SavePlayersStats();
    }

    private void SavePlayersStats()
    {
        try
        {
            var allPlayers = _storage.LoadPlayersStats();

            foreach (var player in _players)
            {
                var existingPlayer = allPlayers.FirstOrDefault(p => p.Name == player.Name);
                if (existingPlayer != null)
                {
                    existingPlayer.TotalWins = player.TotalWins;
                    existingPlayer.Score = player.Score;
                }
                else
                {
                    allPlayers.Add(player);
                }
            }

            _storage.SavePlayersStats(allPlayers);
        }
        catch (Exception ex)
        {
            _view.ShowMessage(_translation.GetText("StatsSaveError"));
            _view.ShowMessage(ex.Message);
        }
    }

    private bool TryRestoreInterruptedGame()
    {
        try
        {
            GameState session = _storage.LoadSession();
            if (session != null && session.GameFinished == false)
            {
                RestoreGameFromSession(session);
                _storage.DeleteSession();
                return true;
            }
        }
        catch (Exception ex)
        {
            _view.ShowMessage(_translation.GetText("SessionLoadError"));
            _view.ShowMessage(ex.Message);
        }

        return false;
    }

    private void RestoreGameFromSession(GameState session)
    {
        string langToUse = session.Language ?? "ru-RU";
        _translation.SetLanguage(langToUse);
        _wordValidator.CurrentCulture = _translation.CurrentCulture;

        StartWord = session.StartWord ?? "";
        _usedWords = session.UsedWords ?? new HashSet<string>();
        _players = session.Players ?? new List<Player>();
        _currentPlayerIndex = session.CurrentPlayerIndex;

        int interruptedPlayerIndex = session.CurrentPlayerIndex;
        int winnerPlayerIndex = interruptedPlayerIndex == 0 ? 1 : 0;

        string winnerMessage = string.Format(_translation.GetText("Winner"), _players[winnerPlayerIndex].Name);
        string loserMessage = string.Format(_translation.GetText("PlayerLostSimple"), _players[interruptedPlayerIndex].Name);

        GameResult result = CreateGameResult(winnerMessage, loserMessage, _players[winnerPlayerIndex].Name, _players[interruptedPlayerIndex].Name);
        _storage.SaveResult(result);

        _players[winnerPlayerIndex].TotalWins++;

        _view.ClearScreen();
        _view.ShowMessage(_translation.GetText("GameWasInterrupted"));
        _view.ShowMessage(winnerMessage);
        _view.ShowMessage(_translation.GetText("PressAnyKeyToContinue"));
        _view.WaitForAnyKey();
    }

    private void ChooseLanguage()
    {
        while (true)
        {
            _view.ShowMessage(_translation.GetText("ChooseLanguage"));
            _view.ShowMessage(_translation.GetText("LanguageOption1"));
            _view.ShowMessage(_translation.GetText("LanguageOption2"));
            _view.ShowPrompt(_translation.GetText("LanguageChoice"));

            string choice = _view.ReadInput();

            if (choice == "1")
            {
                _translation.SetLanguage("en-US");
                break;
            }
            if (choice == "2")
            {
                _translation.SetLanguage("ru-RU");
                break;
            }

            _view.ShowMessage(_translation.GetText("LocalisationError1"));
            _view.ShowMessage(_translation.GetText("LocalisationError2"));
        }

        _wordValidator.CurrentCulture = _translation.CurrentCulture;
    }

    private void SetupNewGame()
    {
        _usedWords.Clear();
        _currentPlayerIndex = 0;
        _gameFinished = false;

        foreach (var player in _players)
        {
            player.Score = 0;
        }

        SetupGame();
    }

    private void SetupGame()
    {
        bool validWord = false;

        while (validWord == false)
        {
            _view.ShowPrompt(_translation.GetText("EnterStartWord"));
            string inputWord = _view.ReadInput().ToLower().Trim();

            CheckResult result = _wordValidator.CheckStartWord(inputWord);

            if (result.IsValid == true)
            {
                StartWord = inputWord;
                _wordValidator.StartWord = inputWord;
                _usedWords.Add(StartWord);
                validWord = true;

                string confirmation = string.Format(_translation.GetText("StartWordSet"), StartWord);
                _view.ShowMessage(confirmation);
            }
            else
            {
                string errorMessage = _translation.GetText(result.ErrorKey);
                _view.ShowMessage(errorMessage);
            }
        }

        _view.ShowMessage(_translation.GetText("PressAnyKeyToStart"));
        _view.WaitForAnyKey();
    }

    private void PlayRound()
    {
        SaveActiveSession();

        _view.ClearScreen();
        _view.ShowGameHeader(StartWord, _players[0].Name, _players[1].Name);
        _timeExpired = false;

        int wordsUsedCount = _usedWords.Count - 1;
        _view.ShowTurnInfo(_players[_currentPlayerIndex].Name, wordsUsedCount);

        _view.ShowMessage(_translation.GetText("AvailableCommands"));

        _view.ShowUsedWords(_usedWords, StartWord);

        string inputPrompt = string.Format(_translation.GetText("EnterWordPrompt"), _players[_currentPlayerIndex].Name);
        _view.ShowPrompt(inputPrompt);

        _timer.Start();
        string input = _view.ReadInput().Trim();
        _timer.Stop();

        if (input.StartsWith("/"))
        {
            ProcessCommand(input);
            return;
        }

        string word = input.ToLower().Trim();

        if (_timeExpired == true)
        {
            string message = string.Format(_translation.GetText("TimeExpired"), _players[_currentPlayerIndex].Name);
            EndGame(message);
        }
        else if (string.IsNullOrEmpty(word))
        {
            string errorMessage = _translation.GetText("ErrorEmptyWord");
            string message = string.Format(_translation.GetText("PlayerLost"), _players[_currentPlayerIndex].Name, errorMessage);
            EndGame(message);
        }
        else
        {
            ProcessWord(word);
        }
    }

    private void ProcessCommand(string command)
    {
        switch (command.ToLower())
        {
            case "/show-words":
                _view.ShowAllWords(_usedWords, StartWord);
                break;
            case "/score":
                _view.ShowCurrentScore(_players[0].Name, _players[0].Score, _players[1].Name, _players[1].Score);
                break;
            case "/total-score":
                _view.ShowTotalScore(_players[0].Name, _players[0].TotalWins, _players[1].Name, _players[1].TotalWins);
                break;
            default:
                _view.ShowUnknownCommand();
                break;
        }

        _view.ShowMessage(_translation.GetText("PressAnyKeyToContinue"));
        _view.WaitForAnyKey();
    }

    private void ProcessWord(string word)
    {
        CheckResult result = _wordValidator.CheckPlayerWord(word, _usedWords);

        if (result.IsValid == true)
        {
            _usedWords.Add(word);
            string successMessage = string.Format(_translation.GetText("WordAccepted"), word);
            _view.ShowMessage(successMessage);

            _players[_currentPlayerIndex].Score++;

            _currentPlayerIndex = _currentPlayerIndex == 0 ? 1 : 0;

            _view.ShowMessage(_translation.GetText("PressAnyKeyToContinue"));
            _view.WaitForAnyKey();
        }
        else
        {
            string errorMessage = GetErrorMessage(result);
            string message = string.Format(_translation.GetText("PlayerLost"), _players[_currentPlayerIndex].Name, errorMessage);
            EndGame(message);
        }
    }

    private string GetErrorMessage(CheckResult result)
    {
        string errorMessage = _translation.GetText(result.ErrorKey);
        if (result.ErrorKey == "ErrorInvalidLetters")
        {
            errorMessage = string.Format(errorMessage, StartWord);
        }
        return errorMessage;
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

        int winnerIndex = _currentPlayerIndex == 0 ? 1 : 0;
        Player winner = _players[winnerIndex];
        Player loser = _players[_currentPlayerIndex];

        winner.TotalWins++;

        SavePlayersStats();

        string winnerMessage = string.Format(_translation.GetText("Winner"), winner.Name);
        string loserMessage = string.Format(_translation.GetText("PlayerLostSimple"), loser.Name);

        GameResult result = CreateGameResult(winnerMessage, loserMessage, winner.Name, loser.Name);

        try
        {
            _storage.SaveResult(result);
        }
        catch (Exception ex)
        {
            _view.ShowMessage(_translation.GetText("ResultSaveError"));
            _view.ShowMessage(ex.Message);
        }

        _storage.DeleteSession();

        _view.ClearScreen();
        _view.ShowGameOver(message, StartWord, _usedWords, winner.Name);
    }

    private GameResult CreateGameResult(string winnerMessage, string loserMessage, string winnerName, string loserName)
    {
        return new GameResult
        {
            StartWord = StartWord,
            UsedWords = new HashSet<string>(_usedWords),
            WinnerMessage = winnerMessage,
            LoserMessage = loserMessage,
            TotalWords = _usedWords.Count - 1,
            FinishedAt = DateTime.Now,
            WinnerName = winnerName,
            LoserName = loserName
        };
    }

    private void SaveActiveSession()
    {
        var session = new GameState
        {
            StartWord = StartWord,
            UsedWords = new HashSet<string>(_usedWords),
            CurrentPlayerIndex = _currentPlayerIndex,
            GameFinished = false,
            GameStarted = DateTime.Now,
            Language = _translation.CurrentCulture.Name,
            Players = _players
        };

        try
        {
            _storage.SaveSession(session);
        }
        catch (Exception ex)
        {
            _view.ShowMessage(_translation.GetText("SessionSaveError"));
            _view.ShowMessage(ex.Message);
        }
    }

}
