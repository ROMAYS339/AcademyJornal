using AcademyJournal.Core.Models;
using AcademyJournal.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AcademyJornal.UI.Views
{
    /// <summary>
    /// Логика взаимодействия для GradesInputView.xaml
    /// </summary>
    public partial class GradesInputView : UserControl
    {
        public GradesInputView()
        {
            InitializeComponent();
        }

        private void Parse_Click(object sender, RoutedEventArgs e)
        {
            List<StudentGradeInput> students = TextParserService.Parse(GradeInputBox.Text);

            while (GradeGrid.Columns.Count > 1)
            {
                GradeGrid.Columns.RemoveAt(1);
            }

            if (students == null || students.Count == 0) return;

            // 2. Находим, какое максимальное количество оценок есть у кого-то из студентов
            int maxGradesCount = students.Max(s => s.grades?.Count ?? 0);

            // 3. Генерируем новые колонки оценок в столбик
            for (int i = 0; i < maxGradesCount; i++)
            {
                var gradeColumn = new DataGridTextColumn
                {
                    Header = $"Оценка {i + 1}",
                    // Важно: привязываемся через свойство Student, так как мы изменим ItemsSource ниже
                    Binding = new Binding($"Student.grades[{i}]"),
                    Width = new DataGridLength(80),
                };

                var textStyle = new Style(typeof(TextBlock));
                textStyle.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));
                textStyle.Setters.Add(new Setter(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center));
                textStyle.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.Bold));
                gradeColumn.ElementStyle = textStyle;

                GradeGrid.Columns.Add(gradeColumn);
            }

            // 4. ДОПИСЫВАЕМ СТОЛБЕЦ СРЕДНЕГО БАЛЛА
            var avgColumn = new DataGridTextColumn
            {
                Header = "Средний балл",
                Binding = new Binding("Average") { StringFormat = "F1" }, // Округляем до 1 знака
                Width = new DataGridLength(110)
            };

            var avgStyle = new Style(typeof(TextBlock));
            avgStyle.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));
            avgStyle.Setters.Add(new Setter(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center));
            avgStyle.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.Bold));
            avgStyle.Setters.Add(new Setter(TextBlock.ForegroundProperty, Application.Current.Resources["AccentPurpleLightBrush"]));
            avgColumn.ElementStyle = avgStyle;

            GradeGrid.Columns.Add(avgColumn);

            // 5. ДОПИСЫВАЕМ ПОДСЧЕТ СРЕДНЕГО ЗНАЧЕНИЯ НА ЛЕТУ
            // Передаем в таблицу анонимный объект, где средний балл уже вычислен через LINQ
            GradeGrid.ItemsSource = students.Select(s => new
            {
                fullName = s.fullName, // Для первой колонки "Студент"
                Student = s,           // Для динамических колонок оценок
                Average = s.grades != null && s.grades.Count > 0 ? s.grades.Average() : 0.0
            }).ToList();
        }
    }
}
