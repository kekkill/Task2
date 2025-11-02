using System.Globalization;
using System.Resources;
using System.Reflection;

public class TranslationManager
{
    private CultureInfo _currentCulture;
    private ResourceManager _resourceManager;

    public CultureInfo CurrentCulture => _currentCulture;

    public TranslationManager()
    {
        _resourceManager = new ResourceManager("Tasks.Resources", Assembly.GetExecutingAssembly());
    }

    public void SetLanguage(string cultureName)
    {
        _currentCulture = new CultureInfo(cultureName);
    }

    public string GetText(string key)
    {
        string result = _resourceManager.GetString(key, _currentCulture);
        if (result != null)
        {
            return result;
        }
        return key;
    }
}