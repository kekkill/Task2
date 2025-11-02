using System;
using System.Collections.Generic;

public class WordGame
{
    int TIME_LIMIT_MS = 15000;

    WordChecker _wordChecker;
    TranslationManager _translation;
    GameTimer _timer;
    GameVisual _visual;
    HashSet<string> _usedWords;

    int _currentPlayer;
    bool _gameFinished;
    bool _timeExpired;

    public string StartWord;

    public WordGame()
    {
        _wordChecker = new WordChecker();
        _translation = new TranslationManager();
        _timer = new GameTimer(TIME_LIMIT_MS);
        _visual = new GameVisual(_translation);
        _usedWords = new HashSet<string>();
        _currentPlayer = 1;
        _timer.OnTimeExpired = OnTimeExpired;
    }

    public void Start()
    {
        ChooseLanguage();
        _visual.ShowRules();
        SetupGame();

        while (!_gameFinished)
        {
            PlayRound();
        }

        _visual.ShowExitMessage(_timeExpired);
    }

    private void ChooseLanguage()
    {
        _visual.ShowMessage(_translation.GetText("ChooseLanguage"));
        _visual.ShowMessage(_translation.GetText("LanguageOption1"));
        _visual.ShowMessage(_translation.GetText("LanguageOption2"));
        _visual.ShowPrompt(_translation.GetText("LanguageChoice"));

        string choice = Console.ReadLine();
        if (choice == "1")
        {
            _translation.SetLanguage("en-US");
        }
        else
        {
            _translation.SetLanguage("ru-RU");
        }
        _wordChecker.CurrentCulture = _translation.CurrentCulture;
    }

    private void SetupGame()
    {
        bool validWord = false;

        while (!validWord)
        {
            _visual.ShowPrompt(_translation.GetText("EnterStartWord"));
            string inputWord = Console.ReadLine().ToLower().Trim();

            var result = _wordChecker.CheckStartWord(inputWord);

            if (result.IsValid)
            {
                StartWord = inputWord;
                _wordChecker.StartWord = inputWord;
                _usedWords.Add(StartWord);
                validWord = true;

                string confirmation = string.Format(_translation.GetText("StartWordSet"), StartWord);
                _visual.ShowMessage(confirmation);
            }
            else
            {
                string errorMessage = _translation.GetText(result.ErrorKey);
                _visual.ShowPrompt(errorMessage);
            }
        }

        _visual.ShowMessage(_translation.GetText("PressAnyKeyToStart"));
        Console.ReadKey();
    }

    private void PlayRound()
    {
        Console.Clear();
        _visual.ShowGameHeader(StartWord);
        _timeExpired = false;

        int wordsUsedCount = _usedWords.Count - 1;
        _visual.ShowTurnInfo(_currentPlayer, wordsUsedCount);
        _visual.ShowUsedWords(_usedWords, StartWord);

        string inputPrompt = string.Format(_translation.GetText("EnterWordPrompt"), _currentPlayer);
        _visual.ShowPrompt(inputPrompt);

        _timer.Start();
        string word = Console.ReadLine().ToLower().Trim();
        _timer.Stop();

        if (_timeExpired)
        {
            string message = string.Format(_translation.GetText("TimeExpired"), _currentPlayer);
            EndGame(message);
        }
        else if (string.IsNullOrEmpty(word))
        {
            string message = string.Format(_translation.GetText("PlayerLost"),
                                         _currentPlayer,
                                         _translation.GetText("ErrorEmptyWord"));
            EndGame(message);
        }
        else
        {
            ProcessWord(word);
        }
    }

    private void ProcessWord(string word)
    {
        var result = _wordChecker.CheckPlayerWord(word, _usedWords);

        if (result.IsValid)
        {
            _usedWords.Add(word);
            string successMessage = string.Format(_translation.GetText("WordAccepted"), word);
            _visual.ShowMessage(successMessage);

            if (_currentPlayer == 1)
            {
                _currentPlayer = 2;
            }
            else
            {
                _currentPlayer = 1;
            }

            _visual.ShowMessage(_translation.GetText("PressAnyKeyToContinue"));
            Console.ReadKey();
        }
        else
        {
            string errorMessage = _translation.GetText(result.ErrorKey);
            if (result.ErrorKey == "ErrorInvalidLetters")
            {
                errorMessage = string.Format(errorMessage, StartWord);
            }

            string message = string.Format(_translation.GetText("PlayerLost"), _currentPlayer, errorMessage);
            EndGame(message);
        }
    }

    private void OnTimeExpired()
    {
        if (!_timeExpired && !_gameFinished)
        {
            _timeExpired = true;
            _visual.ShowMessage("");
            _visual.ShowMessage(_translation.GetText("TimeExpired"));

            IntPtr stdin = NativeConsole.GetStdHandle(NativeConsole.StdHandle.Stdin);
            NativeConsole.CloseHandle(stdin);
        }
    }

    private void EndGame(string message)
    {
        _timer.Stop(); 

        Console.Clear();
        _visual.ShowGameOver(message, StartWord, _usedWords, _currentPlayer);
        _gameFinished = true;
    }
}