using System.Configuration;
using System.Data;
using System.Windows;

namespace AcademyJornal.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private bool _isDark = true;

        public void ToggleTheme()
        {
            _isDark = !_isDark;
            var uri = _isDark
                ? new Uri("Themes/DarkTheme.xaml", UriKind.Relative)
                : new Uri("Themes/LightTheme.xaml", UriKind.Relative);

            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(new ResourceDictionary { Source = uri });
        }

        public bool IsDarkTheme => _isDark;
    }

}
