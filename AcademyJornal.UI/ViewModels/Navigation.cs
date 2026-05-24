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
    public class Navigation : INotifyPropertyChanged
    
    {
        private readonly UserControl _dashboardView = new DashboardView();
        private readonly UserControl _gradesView = new GradesInputView();
        private readonly UserControl _materialsView = new MaterialsView();
        private readonly UserControl _reviewsView = new ReviewsView();

        private UserControl _currentView;
        public UserControl CurrentView
        {
            get => _currentView;
            private set { _currentView = value; OnPropertyChanged(); UpdateActiveFlags(); }
        }

        private bool _isDashboardActive;
        private bool _isGradesActive;
        private bool _isMaterialsActive;
        private bool _isReviewsActive;

        public bool IsDashboardActive { get => _isDashboardActive; private set { _isDashboardActive = value; OnPropertyChanged(); } }
        public bool IsGradesActive { get => _isGradesActive; private set { _isGradesActive = value; OnPropertyChanged(); } }
        public bool IsMaterialsActive { get => _isMaterialsActive; private set { _isMaterialsActive = value; OnPropertyChanged(); } }
        public bool IsReviewsActive {get => _isReviewsActive; private set { _isReviewsActive = value; OnPropertyChanged(); } }

        public ICommand NavigateDashboardCommand { get; }
        public ICommand NavigateGradesCommand { get; }
        public ICommand NavigateMaterialsCommand { get; }
        public ICommand NavigateReviewsCommand { get; }

        public Navigation()
        {
            _currentView = _dashboardView;

            NavigateDashboardCommand = new RelayCommand(_ => CurrentView = _dashboardView);
            NavigateGradesCommand = new RelayCommand(_ => CurrentView = _gradesView);
            NavigateMaterialsCommand = new RelayCommand(_ => CurrentView = _materialsView);
            NavigateReviewsCommand = new RelayCommand(_ => CurrentView = _reviewsView);

            UpdateActiveFlags();
        }

        private void UpdateActiveFlags()
        {
            IsDashboardActive = ReferenceEquals(CurrentView, _dashboardView);
            IsGradesActive = ReferenceEquals(CurrentView, _gradesView);
            IsMaterialsActive = ReferenceEquals(CurrentView, _materialsView);
            IsReviewsActive = ReferenceEquals(CurrentView, _reviewsView);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}

