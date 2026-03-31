using System;
using System.Linq;
using System.Windows;
using TMXWinTerminal.Models;

namespace TMXWinTerminal.Helpers
{
    public static class ThemeManager
    {
        private static readonly Uri LightUri = new Uri("Themes/LightTheme.xaml", UriKind.Relative);
        private static readonly Uri DarkUri = new Uri("Themes/DarkTheme.xaml", UriKind.Relative);

        public static void ApplyTheme(ThemeMode mode)
        {
            var dictionaries = Application.Current.Resources.MergedDictionaries;
            var existing = dictionaries.FirstOrDefault(d =>
                d.Source != null &&
                (d.Source.OriginalString.Contains("LightTheme") || d.Source.OriginalString.Contains("DarkTheme")));

            if (existing != null)
            {
                dictionaries.Remove(existing);
            }

            var uri = mode == ThemeMode.Dark ? DarkUri : LightUri;
            dictionaries.Add(new ResourceDictionary { Source = uri });
        }
    }
}
