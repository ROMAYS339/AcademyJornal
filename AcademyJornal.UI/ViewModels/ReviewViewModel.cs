using AcademyJornal.Data;
using AcademyJournal.Core.Models;
using AcademyJournal.Core.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace AcademyJornal.UI.ViewModels
{
    public class ReviewItem : INotifyPropertyChanged
    {
        public string StudentName { get; init; } = "";
        public string ModuleName { get; init; } = "";
        public string GeneratedAt { get; init; } = "";
        public int StudentId { get; init; }
        public int ModuleId { get; init; }

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
        private readonly AppDbContext _db = new();
        private ReviewGeneratorService? _gen;

        public ObservableCollection<Student> Students { get; } = new();
        public ObservableCollection<Module> Modules { get; } = new();

        private Student? _selectedStudent;
        public Student? SelectedStudent
        {
            get => _selectedStudent;
            set { _selectedStudent = value; OnPropertyChanged(); }
        }

        private Module? _selectedModule;
        public Module? SelectedModule
        {
            get => _selectedModule;
            set { _selectedModule = value; OnPropertyChanged(); }
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

        public ICommand GenerateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CopyCommand { get; }
        public ICommand SaveCommand { get; }

        public ReviewViewModel()
        {
            _gen = new ReviewGeneratorService(_db);

            GenerateCommand = new RelayCommand(_ => Generate());
            DeleteCommand = new RelayCommand(item => { if (item is ReviewItem r) Reviews.Remove(r); });
            CopyCommand = new RelayCommand(item => { if (item is ReviewItem r) Clipboard.SetText(r.Text); });
            SaveCommand = new RelayCommand(item => Save(item as ReviewItem));

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                foreach (var s in _db.Student.OrderBy(s => s.FullName))
                    Students.Add(s);

                foreach (var m in _db.Modules.OrderBy(m => m.Name))
                    Modules.Add(m);

                SelectedStudent = Students.FirstOrDefault();
                SelectedModule = Modules.FirstOrDefault();
            }
            catch
            {
                ErrorMessage = "Нет подключения к БД. Работа в демо-режиме.";
            }
        }

        private void Generate()
        {
            ErrorMessage = "";

            if (SelectedStudent == null) { ErrorMessage = "Выберите студента."; return; }
            if (SelectedModule == null) { ErrorMessage = "Выберите модуль."; return; }

            try
            {
                var review = _gen!.GenerateReview(SelectedStudent.Id, SelectedModule.Id);

                if (review == null)
                {
                    ErrorMessage = "Нет оценок за этот модуль у студента.";
                    return;
                }

                Reviews.Insert(0, new ReviewItem
                {
                    StudentName = SelectedStudent.FullName,
                    ModuleName = SelectedModule.Name,
                    GeneratedAt = DateTime.Now.ToString("HH:mm"),
                    Text = review.Text,
                    StudentId = SelectedStudent.Id,
                    ModuleId = SelectedModule.Id,
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка генерации: {ex.Message}";
            }
        }

        private void Save(ReviewItem? item)
        {
            if (item == null || SelectedStudent == null || SelectedModule == null) return;

            try
            {
                var review = new Review
                {
                    Text = item.Text,
                    StudentId = item.StudentId,
                    ModuleId = item.ModuleId,
                    CreatedAt = DateTime.Now,
                    IsEdited = false
                };
                _db.Review.Add(review);
                _db.SaveChanges();
                MessageBox.Show("Отзыв сохранён в базу данных.", "Готово",
                    MessageBoxButton.OK, MessageBoxImage.Information);
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