using AcademyJournal.Core.Models;
using AcademyJournal.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AcademyJornal.UI.ViewModels
{
    public class ReviewItem : INotifyPropertyChanged
    {
        public string StudentName { get; init; } = "";
        public string ModuleName { get; init; } = "";
        public double Score { get; init; }
        public string GeneratedAt { get; init; } = "";

        private string _text = "";
        public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
    public class ReviewViewModel : INotifyPropertyChanged
    {
        private readonly ReviewGeneratorService _gen = new();

        private string _firstName = "Диана";
        public string Name
        {
            get => _firstName;
            set { _firstName = value; OnPropertyChanged(); }
        }

        private string _selectedModule = "Тестирование";
        public string SelectedModule
        {
            get => _selectedModule;
            set { _selectedModule = value; OnPropertyChanged(); }
        }

        private string _scoreText = "8.5";
        public string ScoreText
        {
            get => _scoreText;
            set { _scoreText = value; OnPropertyChanged(); }
        }

        private string _hwPercentText = "80";
        public string HwPercentText
        {
            get => _hwPercentText;
            set { _hwPercentText = value; OnPropertyChanged(); }
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

        public ObservableCollection<ReviewItem> Reviews { get; } = new();

        public string[] Modules { get; } = { "Тестирование", "БД", "Курсовой" };

        public ICommand GenerateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CopyCommand { get; }

        public ReviewViewModel()
        {
            GenerateCommand = new RelayCommand(_ => Generate());
            DeleteCommand = new RelayCommand(item => { if (item is ReviewItem r) Reviews.Remove(r); });
            CopyCommand = new RelayCommand(item => { if (item is ReviewItem r) Clipboard.SetText(r.Text); });
        }

        private void Generate()
        {
            ErrorMessage = "";

            if (string.IsNullOrWhiteSpace(Name))
            { ErrorMessage = "Введите имя студента."; return; }

            if (!double.TryParse(ScoreText.Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out double score) || score < 1 || score > 10)
            { ErrorMessage = "Балл должен быть от 1 до 10."; return; }

            if (!double.TryParse(HwPercentText, out double hwPct) || hwPct < 0 || hwPct > 100)
            { ErrorMessage = "% ДЗ должен быть от 0 до 100."; return; }
            var student = new Student { FullName = Name.Trim() };
            string text = _gen.Generate(student, SelectedModule, score, hwPct);
            Reviews.Insert(0, new ReviewItem
            {
                StudentName = student.FullName,
                ModuleName = SelectedModule,
                Score = score,
                GeneratedAt = DateTime.Now.ToString("HH:mm"),
                Text = text,
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

}

