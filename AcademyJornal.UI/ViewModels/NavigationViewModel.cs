using AcademyJornal.UI.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;

namespace AcademyJornal.UI.ViewModels
{
    public class NavigationViewModel : INotifyPropertyChanged
    {
        private readonly UserControl _dashboard = new DashboardView();
        private readonly UserControl _grades = new GradesInputView();
        private readonly UserControl _materials = new MaterialsView();
        private readonly UserControl _reviews = new ReviewsView();
        private readonly UserControl _charts = new ChartView();

        private UserControl _currentView;
        public UserControl CurrentView
        {
            get => _currentView;
            private set { _currentView = value; OnPropertyChanged(); UpdateFlags(); }
        }

        // Флаги IsChecked для RadioButton
        public bool IsDashboardActive { get => _f0; private set { _f0 = value; OnPropertyChanged(); } }
        public bool IsGradesActive { get => _f1; private set { _f1 = value; OnPropertyChanged(); } }
        public bool IsMaterialsActive { get => _f2; private set { _f2 = value; OnPropertyChanged(); } }
        public bool IsReviewsActive { get => _f3; private set { _f3 = value; OnPropertyChanged(); } }
        public bool IsChartsActive { get => _f4; private set { _f4 = value; OnPropertyChanged(); } }
        private bool _f0, _f1, _f2, _f3, _f4;

        public ICommand NavDashboardCommand { get; }
        public ICommand NavGradesCommand { get; }
        public ICommand NavMaterialsCommand { get; }
        public ICommand NavReviewsCommand { get; }
        public ICommand NavChartsCommand { get; }

        public NavigationViewModel()
        {
            _currentView = _dashboard;

            NavDashboardCommand = new RelayCommand(_ => CurrentView = _dashboard);
            NavGradesCommand = new RelayCommand(_ => CurrentView = _grades);
            NavMaterialsCommand = new RelayCommand(_ => CurrentView = _materials);
            NavReviewsCommand = new RelayCommand(_ => CurrentView = _reviews);
            NavChartsCommand = new RelayCommand(_ => CurrentView = _charts);

            UpdateFlags();
        }

        private void UpdateFlags()
        {
            IsDashboardActive = ReferenceEquals(CurrentView, _dashboard);
            IsGradesActive = ReferenceEquals(CurrentView, _grades);
            IsMaterialsActive = ReferenceEquals(CurrentView, _materials);
            IsReviewsActive = ReferenceEquals(CurrentView, _reviews);
            IsChartsActive = ReferenceEquals(CurrentView, _charts);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

}
