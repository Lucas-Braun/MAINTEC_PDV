using System.Windows;

namespace PDV.App.Themes;

public static class ThemeManager
{
    private const string ColorsPrefix = "Themes/Fiori/Colors.";
    private const string ColorsSuffix = ".xaml";

    /// <summary>
    /// Troca o tema em runtime. Nomes validos: "MorningHorizon", "EveningHorizon"
    /// </summary>
    public static void ApplyTheme(string themeName)
    {
        var uri = new Uri($"{ColorsPrefix}{themeName}{ColorsSuffix}", UriKind.Relative);
        var newColors = new ResourceDictionary { Source = uri };

        var app = Application.Current;
        var mergedDicts = app.Resources.MergedDictionaries;

        // FioriTheme.xaml e o primeiro MergedDictionary do App
        // Dentro dele, Colors e o primeiro sub-dictionary
        if (mergedDicts.Count > 0 && mergedDicts[0].MergedDictionaries.Count > 0)
        {
            mergedDicts[0].MergedDictionaries[0] = newColors;
        }
    }

    public static string CurrentTheme
    {
        get
        {
            var app = Application.Current;
            var mergedDicts = app.Resources.MergedDictionaries;
            if (mergedDicts.Count > 0 && mergedDicts[0].MergedDictionaries.Count > 0)
            {
                var source = mergedDicts[0].MergedDictionaries[0].Source?.OriginalString ?? "";
                if (source.Contains("EveningHorizon")) return "EveningHorizon";
            }
            return "MorningHorizon";
        }
    }
}
