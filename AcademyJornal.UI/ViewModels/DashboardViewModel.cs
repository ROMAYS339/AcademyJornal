using AcademyJornal.Data;
using AcademyJournal.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace AcademyJornal.UI.ViewModels
{
    public class StudentRankRow
    {
        public int Rank { get; init; }
        public string FullName { get; init; } = "";
        public string Group { get; init; } = "";
        public string AvgScore { get; init; } = "";
        public string ScoreColor { get; init; } = "#7F77DD";
    }

    /// <summary>Карточка-метрика на дашборде</summary>
    public class MetricCard
    {
        public string Icon { get; init; } = "";
        public string Title { get; init; } = "";
        public string Value { get; init; } = "";
        public string Sub { get; init; } = "";
        public string Color { get; init; } = "#7F77DD";
    }

    /// <summary>ViewModel для вкладки «Главная»</summary>
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _db;
        private readonly GradeCalculatorService _calc;

        public ObservableCollection<MetricCard> Metrics { get; } = new();
        public ObservableCollection<StudentRankRow> TopStudents { get; } = new();

        private string _welcomeText = "Добро пожаловать в AcademyJornal!";
        public string WelcomeText
        {
            get => _welcomeText;
            private set { _welcomeText = value; OnPropertyChanged(); }
        }

        private string _summaryText = "";
        public string SummaryText
        {
            get => _summaryText;
            private set { _summaryText = value; OnPropertyChanged(); }
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

        public DashboardViewModel()
        {
            _db = new AppDbContext();
            _calc = new GradeCalculatorService(_db);
            Load();
        }

        private void Load()
        {
            try
            {
                var students = _db.Student.ToList();
                var modules = _db.Modules.ToList();
                var grades = _db.Grades.ToList();
                var reviews = _db.Review.ToList();

                int totalStudents = students.Count;
                int totalModules = modules.Count;
                int totalGrades = grades.Count;
                int totalReviews = reviews.Count;

                // Подсчёт средних по всем студентам/модулям
                var scores = new List<(string Name, string Group, double Score)>();
                foreach (var s in students)
                {
                    foreach (var m in modules)
                    {
                        var hasGrades = grades.Any(g => g.StudentId == s.Id && g.ModuleId == m.Id);
                        if (!hasGrades) continue;
                        var avg = (double)_calc.CalculateModuleAverage(s.Id, m.Id);
                        scores.Add((s.FullName, s.Group, avg));
                    }
                }

                double globalAvg = scores.Any() ? scores.Average(x => x.Score) : 0;
                int excellent = scores.Count(x => x.Score >= 9);
                int poor = scores.Count(x => x.Score < 5 && x.Score > 0);

                // Метрики
                Metrics.Add(new MetricCard { Icon = "👥", Title = "Студентов", Value = totalStudents.ToString(), Sub = "в базе данных", Color = "#7F77DD" });
                Metrics.Add(new MetricCard { Icon = "📚", Title = "Модулей", Value = totalModules.ToString(), Sub = "учебных программ", Color = "#1D9E75" });
                Metrics.Add(new MetricCard { Icon = "📝", Title = "Оценок", Value = totalGrades.ToString(), Sub = "записей выставлено", Color = "#EF9F27" });
                Metrics.Add(new MetricCard { Icon = "⭐", Title = "Средний балл", Value = $"{globalAvg:F1}", Sub = "по всем модулям", Color = "#D4537E" });
                Metrics.Add(new MetricCard { Icon = "🏆", Title = "Отличников", Value = excellent.ToString(), Sub = "оценок ≥ 9", Color = "#1D9E75" });
                Metrics.Add(new MetricCard { Icon = "⚠️", Title = "Неуспевающих", Value = poor.ToString(), Sub = "оценок < 5", Color = "#E24B4A" });

                // Топ студентов (берём лучший средний балл по всем модулям)
                var byStudent = scores
                    .GroupBy(x => (x.Name, x.Group))
                    .Select(g => (g.Key.Name, g.Key.Group, Avg: g.Average(x => x.Score)))
                    .OrderByDescending(x => x.Avg)
                    .Take(10)
                    .ToList();

                int rank = 1;
                foreach (var (name, group, avg) in byStudent)
                {
                    string color = avg >= 9 ? "#1D9E75" : avg >= 7 ? "#7F77DD" : avg >= 5 ? "#EF9F27" : "#E24B4A";
                    TopStudents.Add(new StudentRankRow
                    {
                        Rank = rank++,
                        FullName = name,
                        Group = group,
                        AvgScore = $"{avg:F1}",
                        ScoreColor = color,
                    });
                }

                SummaryText = $"Данные обновлены • {totalStudents} студентов • {totalModules} модулей • {scores.Count} результатов";
                WelcomeText = $"Добро пожаловать!  Средний балл по системе: {globalAvg:F1} / 12";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Не удалось подключиться к БД: {ex.Message}. Проверьте строку подключения.";
                WelcomeText = "AcademyJornal — Журнал преподавателя";
                SummaryText = "Нет подключения к базе данных";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

}
