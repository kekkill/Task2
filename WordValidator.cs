using System.Globalization;
public class WordValidator
{
    private const int MIN_WORD_LENGTH = 8;
    private const int MAX_WORD_LENGTH = 30;

    public string StartWord;
    public CultureInfo CurrentCulture;

    public CheckResult CheckStartWord(string word)
    {
        string trimmedWord = word.ToLower().Trim();

        if (trimmedWord.Length < MIN_WORD_LENGTH || trimmedWord.Length > MAX_WORD_LENGTH)
            return new CheckResult(false, "ErrorWordLength");

        if (!IsCorrectLanguage(trimmedWord, CurrentCulture))
            return new CheckResult(false, "LanguageError");

        return new CheckResult(true);
    }

    public CheckResult CheckPlayerWord(string word, HashSet<string> usedWords)
    {
        if (string.IsNullOrEmpty(word))
            return new CheckResult(false, "ErrorEmptyWord");

        if (!IsCorrectLanguage(word, CurrentCulture))
            return new CheckResult(false, "LanguageError");

        if (!CanMakeWordFromLetters(word))
            return new CheckResult(false, "ErrorInvalidLetters");

        if (usedWords.Contains(word))
            return new CheckResult(false, "ErrorWordUsed");

        return new CheckResult(true);
    }

    private bool CanMakeWordFromLetters(string wordToCheck)
    {
        char[] sourceLetters = StartWord.ToCharArray();
        bool[] usedLetters = new bool[sourceLetters.Length];

        for (int i = 0; i < wordToCheck.Length; i++)
        {
            char currentChar = wordToCheck[i];
            bool foundLetter = false;

            for (int j = 0; j < sourceLetters.Length; j++)
            {
                if (sourceLetters[j] == currentChar && !usedLetters[j])
                {
                    usedLetters[j] = true;
                    foundLetter = true;
                    break;
                }
            }

            if (!foundLetter) return false;
        }
        return true;
    }

    private bool IsCorrectLanguage(string word, CultureInfo culture)
    {
        if (culture.Name == "ru-RU")
            return HasOnlyRussianLetters(word);
        else
            return HasOnlyEnglishLetters(word);
    }

    private bool HasOnlyRussianLetters(string word)
    {
        for (int i = 0; i < word.Length; i++)
        {
            char c = word[i];
            if (!((c >= 'à' && c <= 'ÿ') || (c >= 'À' && c <= 'ß') || c == '¸' || c == '¨'))
                return false;
        }
        return true;
    }

    private bool HasOnlyEnglishLetters(string word)
    {
        for (int i = 0; i < word.Length; i++)
        {
            char c = word[i];
            if (!((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')))
                return false;
        }
        return true;
    }
}

public class CheckResult
{
    public bool IsValid;
    public string ErrorKey;

    public CheckResult(bool isValid, string errorKey = "")
    {
        IsValid = isValid;
        ErrorKey = errorKey;
    }
}