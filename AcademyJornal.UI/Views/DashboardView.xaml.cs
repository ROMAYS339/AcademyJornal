using AcademyJornal.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AcademyJornal.UI.Views
{
    /// <summary>
    /// Логика взаимодействия для DashboardView.xaml
    /// </summary>
    public partial class DashboardView : UserControl
    {
        private readonly List<StudentSummary> _allData = SampleData.GetSummaries();
        private readonly AppDbContext _db = new();
        private readonly GradeCalculatorService _calc;

        public DashboardView()
        {
            InitializeComponent();
            Loaded += (_, _) => InitFilters();
        }

        private void InitFilters()
        {
            var groups = _allData.Select(s => s.GroupName).Distinct().OrderBy(g => g).ToList();
            var modules = _allData.Select(s => s.ModuleName).Distinct().OrderBy(m => m).ToList();

            CbGroup.ItemsSource = new[] { "Все" }.Concat(groups).ToList();
            CbModule.ItemsSource = new[] { "Все" }.Concat(modules).ToList();
            CbGroup.SelectedIndex = 0;
            CbModule.SelectedIndex = 0;

            RefreshDashboard();
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
            => RefreshDashboard();

        private void RefreshDashboard()
        {
            if (CbGroup == null) return;

            var filtered = _allData.AsEnumerable();
            if (CbGroup.SelectedItem is string g && g != "Все")
                filtered = filtered.Where(s => s.GroupName == g);
            if (CbModule.SelectedItem is string m && m != "Все")
                filtered = filtered.Where(s => s.ModuleName == m);

            var list = filtered.ToList();

            // Карточки
            TbStudentCount.Text = list.Count.ToString();
            TbAvgScore.Text = list.Count > 0 ? list.Average(s => s.FinalScore).ToString("F1") : "—";
            TbHwRate.Text = list.Count > 0 ? list.Average(s => s.HwPercent).ToString("F0") + "%" : "—";
            TbFailing.Text = list.Count(s => s.FinalScore < 5).ToString();

            // Таблица по модулям
            GridModules.ItemsSource = list.OrderBy(s => s.StudentName).ToList();

            // Диаграмма
            DrawChart(list);
        }

        private void DrawChart(List<StudentSummary> data)
        {
            ChartCanvas.Children.Clear();
            if (data.Count == 0) return;

            double canvasW = ChartCanvas.ActualWidth > 0 ? ChartCanvas.ActualWidth : 800;
            double canvasH = ChartCanvas.ActualHeight;
            double padL = 40, padB = 40, padT = 10, padR = 10;
            double chartW = canvasW - padL - padR;
            double chartH = canvasH - padT - padB;

            double maxScore = 10;
            int count = data.Count;
            double barWidth = Math.Min(40, chartW / count - 8);

            // Горизонтальные линии (2, 4, 6, 8, 10)
            for (int score = 2; score <= 10; score += 2)
            {
                double y = padT + chartH - chartH * score / maxScore;
                var line = new Line
                {
                    X1 = padL,
                    X2 = canvasW - padR,
                    Y1 = y,
                    Y2 = y,
                    Stroke = new SolidColorBrush(Color.FromArgb(40, 200, 200, 255)),
                    StrokeThickness = 0.7,
                    StrokeDashArray = new DoubleCollection { 4, 4 },
                };
                ChartCanvas.Children.Add(line);

                var lbl = new TextBlock
                {
                    Text = score.ToString(),
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Color.FromArgb(120, 200, 200, 255)),
                };
                Canvas.SetLeft(lbl, 2);
                Canvas.SetTop(lbl, y - 8);
                ChartCanvas.Children.Add(lbl);
            }

            // Столбцы
            for (int i = 0; i < count; i++)
            {
                var s = data[i];
                double x = padL + i * (chartW / count) + (chartW / count - barWidth) / 2;
                double barH = chartH * s.FinalScore / maxScore;
                double y = padT + chartH - barH;

                var color = s.FinalScore >= 8 ? Color.FromRgb(127, 119, 221)
                          : s.FinalScore >= 6 ? Color.FromRgb(29, 158, 117)
                          : s.FinalScore >= 4 ? Color.FromRgb(239, 159, 39)
                          : Color.FromRgb(226, 75, 74);

                var rect = new Rectangle
                {
                    Width = barWidth,
                    Height = barH,
                    Fill = new SolidColorBrush(color),
                    RadiusX = 4,
                    RadiusY = 4,
                    Opacity = 0.85,
                };
                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y);
                ChartCanvas.Children.Add(rect);

                // Значение над столбцом
                var valLbl = new TextBlock
                {
                    Text = s.FinalScore.ToString("F1"),
                    FontSize = 10,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(color),
                };
                Canvas.SetLeft(valLbl, x + barWidth / 2 - 10);
                Canvas.SetTop(valLbl, y - 16);
                ChartCanvas.Children.Add(valLbl);

                // Имя под столбцом
                var nameLbl = new TextBlock
                {
                    Text = s.StudentName.Split(' ').FirstOrDefault() ?? s.StudentName,
                    FontSize = 9,
                    Foreground = new SolidColorBrush(Color.FromArgb(180, 200, 200, 255)),
                    Width = barWidth + 20,
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center,
                };
                Canvas.SetLeft(nameLbl, x - 10);
                Canvas.SetTop(nameLbl, padT + chartH + 4);
                ChartCanvas.Children.Add(nameLbl);
            }
        }

        private StudentSummary calcFinalScore(Student student, Module module)
        {
            decimal finScore = _calc.CalculateModuleAverage(student.Id, module.Id);
            StudentSummary summary = new StudentSummary { StudentName = student.FullName, GroupName = student.Group, ModuleName = module.Name, FinalScore = Convert.ToDouble(finScore), HwPercent = 80};
            return summary;
        }
    }

    // ── Вспомогательные классы ─────────────────────────────────────────────

    public class StudentSummary
    {
        public string StudentName { get; set; } = "";
        public string GroupName { get; set; } = "";
        public string ModuleName { get; set; } = "";
        public double FinalScore { get; set; }
        public double HwPercent { get; set; }
        public double Score => FinalScore;
    }

    /// <summary>Демо-данные для разработки без подключения к БД.</summary>
    public static class SampleData
    {
        public static List<StudentSummary> GetSummaries() => new()
    {
        new(){ StudentName="Алиева Диана",   GroupName="9А", ModuleName="Тестирование", FinalScore=8.5, HwPercent=90 },
        new(){ StudentName="Борисов Кирилл", GroupName="9А", ModuleName="Тестирование", FinalScore=5.2, HwPercent=60 },
        new(){ StudentName="Волкова Анна",   GroupName="9А", ModuleName="Тестирование", FinalScore=9.1, HwPercent=100 },
        new(){ StudentName="Громов Илья",    GroupName="9А", ModuleName="БД",           FinalScore=6.8, HwPercent=75 },
        new(){ StudentName="Дмитриева Соня", GroupName="9А", ModuleName="БД",           FinalScore=4.3, HwPercent=45 },
        new(){ StudentName="Захаров Михаил", GroupName="9Б", ModuleName="Тестирование", FinalScore=7.6, HwPercent=80 },
        new(){ StudentName="Иванова Лиза",   GroupName="9Б", ModuleName="Тестирование", FinalScore=5.9, HwPercent=65 },
        new(){ StudentName="Козлов Артём",   GroupName="9Б", ModuleName="Курсовой",     FinalScore=8.0, HwPercent=85 },
        new(){ StudentName="Лебедева Вика",  GroupName="10А",ModuleName="Курсовой",     FinalScore=9.4, HwPercent=95 },
        new(){ StudentName="Морозов Денис",  GroupName="10А",ModuleName="БД",           FinalScore=3.5, HwPercent=40 },
    };
    }

}

