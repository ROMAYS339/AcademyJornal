using System.IO;
using System.Windows;

namespace AcademyJornal.UI.Views
{
    public partial class TextEditorWindow : Window
    {
        public string FilePath { get; private set; }
        public string FileName => Path.GetFileName(FilePath);
        public string Content { get; set; }

        public TextEditorWindow(string filePath, string initialContent)
        {
            InitializeComponent();
            FilePath = filePath;
            Content = initialContent;
            DataContext = this;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(FilePath, Content);
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}