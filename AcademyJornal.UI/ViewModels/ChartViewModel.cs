using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AcademyJornal.Data;
using AcademyJournal.Core.Models;
using AcademyJournal.Core.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace AcademyJornal.UI.ViewModels
{
    public class ChartViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _db = new();
        private readonly GradeCalculatorService _calc;

        // Списки для фильтра
        public ObservableCollection<string> Groups { get; } = new();

        private string _selectedGroup = "Все";
        public string SelectedGroup
        {
            get => _selectedGroup;
            set { _selectedGroup = value; OnPropertyChanged(); LoadCharts(); }
        }

        public ObservableCollection<Module> Modules { get; } = new();

        private Module? _selectedModule;
        public Module? SelectedModule
        {
            get => _selectedModule;
            set { _selectedModule = value; OnPropertyChanged(); LoadCharts(); }
        }

        // Серии диаграмм
        private ISeries[] _barSeries = Array.Empty<ISeries>();
        public ISeries[] BarSeries
        {
            get => _barSeries;
            private set { _barSeries = value; OnPropertyChanged(); }
        }

        private Axis[] _barXAxes = Array.Empty<Axis>();
        public Axis[] BarXAxes
        {
            get => _barXAxes;
            private set { _barXAxes = value; OnPropertyChanged(); }
        }

        public Axis[] BarYAxes { get; } = new[]
        {
        new Axis { MinLimit = 0, MaxLimit = 12, LabelsPaint = new SolidColorPaint(SKColors.Gray) }
    };

        private ISeries[] _pieSeries = Array.Empty<ISeries>();
        public ISeries[] PieSeries
        {
            get => _pieSeries;
            private set { _pieSeries = value; OnPropertyChanged(); }
        }

        private string _statsText = "Загрузка...";
        public string StatsText
        {
            get => _statsText;
            private set { _statsText = value; OnPropertyChanged(); }
        }

        private string _errorMessage = "";
        public string ErrorMessage
        {
            get => _errorMessage;
            private set { _errorMessage = value; OnPropertyChanged(); HasError = !string.IsNullOrEmpty(value); }
        }

        private bool _hasError;
        public bool HasError
        {
            get => _hasError;
            private set { _hasError = value; OnPropertyChanged(); }
        }

        public ChartViewModel()
        {
            _calc = new GradeCalculatorService(_db);
            LoadFilters();
            if (!HasError)
                LoadCharts();
            else
                StatsText = "Нет данных";
        }

        private void LoadFilters()
        {
            try
            {
                Groups.Add("Все");
                foreach (var g in _db.Student.Select(s => s.Group).Distinct().OrderBy(g => g))
                    Groups.Add(g);

                foreach (var m in _db.Modules.OrderBy(m => m.Name))
                    Modules.Add(m);

                _selectedModule = Modules.FirstOrDefault();
            }
            catch
            {
                ErrorMessage = "Нет подключения к БД.";
            }
        }

        private void LoadCharts()
        {
            try
            {
                if (SelectedModule == null) return;

                var students = _db.Student.ToList();
                if (SelectedGroup != "Все")
                    students = students.Where(s => s.Group == SelectedGroup).ToList();

                var scores = students
                    .Select(s => new { s.FullName, Score = _calc.CalculateModuleAverage(s.Id, SelectedModule.Id) })
                    .ToList();

                // Столбчатая диаграмма
                BarSeries = new ISeries[]
                {
                new ColumnSeries<double>
                {
                    Values      = scores.Select(s => (double)s.Score).ToArray(),
                    Name        = "Средний балл",
                    Fill        = new SolidColorPaint(SKColor.Parse("#7F77DD")),
                    Stroke      = null,
                    MaxBarWidth = 40,
                }
                };

                BarXAxes = new[]
                {
                new Axis
                {
                    Labels      = scores.Select(s => s.FullName.Split(' ')[0]).ToArray(),
                    LabelsPaint = new SolidColorPaint(SKColors.Gray),
                    TextSize    = 11,
                }
            };

                // Круговая диаграмма
                int excellent = scores.Count(s => s.Score >= 9);
                int good = scores.Count(s => s.Score >= 7 && s.Score < 9);
                int average = scores.Count(s => s.Score >= 5 && s.Score < 7);
                int poor = scores.Count(s => s.Score < 5);

                PieSeries = new ISeries[]
                {
                new PieSeries<int> { Values = new[]{ excellent }, Name = "Отлично (9-12)", Fill = new SolidColorPaint(SKColor.Parse("#7F77DD")) },
                new PieSeries<int> { Values = new[]{ good },      Name = "Хорошо (7-8)",  Fill = new SolidColorPaint(SKColor.Parse("#1D9E75")) },
                new PieSeries<int> { Values = new[]{ average },   Name = "Средне (5-6)",  Fill = new SolidColorPaint(SKColor.Parse("#EF9F27")) },
                new PieSeries<int> { Values = new[]{ poor },      Name = "Слабо (<5)",    Fill = new SolidColorPaint(SKColor.Parse("#E24B4A")) },
                };

                // Статистика
                if (scores.Any())
                {
                    double avg = (double)scores.Average(s => s.Score);
                    int fail = scores.Count(s => s.Score < 5);
                    StatsText = $"Студентов: {scores.Count}   |   Средний балл: {avg:F1}   |   Неуспевающих: {fail}   |   Модуль: {SelectedModule.Name}";
                }
                else
                {
                    StatsText = "Нет данных";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки: {ex.Message}";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

}
