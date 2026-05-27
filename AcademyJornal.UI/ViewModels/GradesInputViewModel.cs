using AcademyJornal.Data;
using AcademyJournal.Core.Models;
using AcademyJournal.Core.Models.Enums;
using AcademyJournal.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace AcademyJornal.UI.ViewModels
{
    public class ParsedGradeRow
    {
        public string StudentName { get; init; } = "";
        public string Grades { get; init; } = "";
        public string Average { get; init; } = "";
        public bool IsValid { get; init; } = true;
    }

    /// <summary>ViewModel для вкладки «Оценки и парсинг»</summary>
    public class GradesInputViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _db = new();

        // ── Списки для ComboBox ────────────────────────────────────────────
        public ObservableCollection<Module> Modules { get; } = new();
        public ObservableCollection<string> GradeTypes { get; } = new()
        { "lesson — за урок", "homework — за ДЗ", "exam — за экзамен" };

        private Module? _selectedModule;
        public Module? SelectedModule
        {
            get => _selectedModule;
            set { _selectedModule = value; OnPropertyChanged(); }
        }

        private string _selectedGradeType = "lesson — за урок";
        public string SelectedGradeType
        {
            get => _selectedGradeType;
            set { _selectedGradeType = value; OnPropertyChanged(); }
        }

        // ── Текстовый ввод ────────────────────────────────────────────────
        private string _rawInput = "Иван Петров 8 9 7 10\nМария Сидорова 10 11 9 12\nАлексей Козлов 5 6 4 7";
        public string RawInput
        {
            get => _rawInput;
            set { _rawInput = value; OnPropertyChanged(); }
        }

        // ── Результаты парсинга ───────────────────────────────────────────
        public ObservableCollection<ParsedGradeRow> ParsedRows { get; } = new();

        private bool _hasParsed;
        public bool HasParsed
        {
            get => _hasParsed;
            private set { _hasParsed = value; OnPropertyChanged(); }
        }

        // ── Статус / ошибки ────────────────────────────────────────────────
        private string _statusMessage = "";
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        private string _errorMessage = "";
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); HasError = !string.IsNullOrEmpty(value); }
        }

        private bool _hasError;
        public bool HasError
        {
            get => _hasError;
            private set { _hasError = value; OnPropertyChanged(); }
        }

        private bool _saveSuccess;
        public bool SaveSuccess
        {
            get => _saveSuccess;
            private set { _saveSuccess = value; OnPropertyChanged(); }
        }

        // ── Команды ───────────────────────────────────────────────────────
        public ICommand ParseCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ClearCommand { get; }

        public GradesInputViewModel()
        {
            ParseCommand = new RelayCommand(_ => ParseInput());
            SaveCommand = new RelayCommand(_ => SaveGrades(), _ => HasParsed && SelectedModule != null);
            ClearCommand = new RelayCommand(_ => { RawInput = ""; ParsedRows.Clear(); HasParsed = false; ErrorMessage = ""; StatusMessage = ""; SaveSuccess = false; });

            LoadModules();
        }

        private void LoadModules()
        {
            try
            {
                foreach (var m in _db.Modules.OrderBy(m => m.Name))
                    Modules.Add(m);
                SelectedModule = Modules.FirstOrDefault();
            }
            catch
            {
                ErrorMessage = "Нет подключения к БД.";
            }
        }

        private void ParseInput()
        {
            ErrorMessage = "";
            SaveSuccess = false;
            ParsedRows.Clear();
            HasParsed = false;

            if (string.IsNullOrWhiteSpace(RawInput))
            {
                ErrorMessage = "Введите данные для парсинга.";
                return;
            }

            try
            {
                var parsed = TextParserService.Parse(RawInput.Trim());
                foreach (var p in parsed)
                {
                    double avg = p.grades.Any() ? p.grades.Average() : 0;
                    ParsedRows.Add(new ParsedGradeRow
                    {
                        StudentName = p.fullName,
                        Grades = string.Join(", ", p.grades),
                        Average = $"{avg:F1}",
                        IsValid = true,
                    });
                }
                HasParsed = true;
                StatusMessage = $"Распознано {parsed.Count} строк. Выберите модуль и тип оценки, затем нажмите «Сохранить».";
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message switch
                {
                    "GradeGreaterThan12" => "Ошибка: оценка больше 12. Допустимый диапазон: 0–12.",
                    "GradeLessOrEqual0" => "Ошибка: оценка меньше или равна 0. Используйте 0 для обозначения пропуска.",
                    "NotANumberException" => "Ошибка: обнаружен нечисловой символ. Формат: «Имя Фамилия оценка1 оценка2 ...»",
                    _ => $"Ошибка парсинга: {ex.Message}"
                };
            }
        }

        private void SaveGrades()
        {
            if (SelectedModule == null) { ErrorMessage = "Выберите модуль."; return; }

            ErrorMessage = "";
            SaveSuccess = false;

            // Определяем тип оценки
            var gradeType = SelectedGradeType.StartsWith("homework") ? GradeType.homework
                          : SelectedGradeType.StartsWith("exam") ? GradeType.exam
                          : GradeType.lesson;

            try
            {
                var parsed = TextParserService.Parse(RawInput.Trim());
                int saved = 0;

                foreach (var row in parsed)
                {
                    // Ищем студента по имени (или создаём нового)
                    var student = _db.Student.FirstOrDefault(s => s.FullName == row.fullName);
                    if (student == null)
                    {
                        student = new Student { FullName = row.fullName, Group = "Новая" };
                        _db.Student.Add(student);
                        _db.SaveChanges();
                    }

                    foreach (var val in row.grades)
                    {
                        _db.Grades.Add(new Grade
                        {
                            StudentId = student.Id,
                            ModuleId = SelectedModule.Id,
                            Value = val,
                            Type = gradeType,
                        });
                        saved++;
                    }
                }

                _db.SaveChanges();
                SaveSuccess = true;
                StatusMessage = $"Сохранено {saved} оценок в модуль «{SelectedModule.Name}» (тип: {gradeType}).";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка сохранения: {ex.Message}";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

}
