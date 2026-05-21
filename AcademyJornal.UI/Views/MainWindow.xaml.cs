using AcademyJornal.UI.Views;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AcademyJornal.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DashboardView _dashboard = new();
        private readonly GradesInputView _grades = new();
        private readonly MaterialsView _materials = new();
        private readonly ReviewsView _reviews = new();

        public MainWindow()
        {
            InitializeComponent();
            MainContent.Content = _dashboard;
        }

        private void Nav_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && MainContent != null)
            {
                MainContent.Content = rb.Tag?.ToString() switch
                {
                    "Dashboard" => _dashboard,
                    "Grades" => _grades,
                    "Materials" => _materials,
                    "Reviews" => _reviews,
                    _ => _dashboard,
                };
            }
        }

        private void BtnToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).ToggleTheme();
        }
    }
}