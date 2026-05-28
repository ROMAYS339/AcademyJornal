using AcademyJournal.Core.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System;
using System.Linq;
using System.Windows;
using AcademyJornal.UI.Views;

namespace AcademyJornal.UI.ViewModels
{
    public class MaterialItem : INotifyPropertyChanged
    {
        public MaterialInfo Info { get; set; } = default!;
        public string DisplayName => Info?.Name ?? "";
        public string DisplaySize => Info?.SizeFormatted ?? "";
        public string DisplayCategory => Info?.Category ?? "";
        public bool IsTextFile => Info?.Extension?.ToLower() == ".txt";

        public ICommand OpenCommand { get; set; } = null!;
        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class MaterialsViewModel : INotifyPropertyChanged
    {
        private readonly MaterialService _materialService = new();
        public ObservableCollection<MaterialItem> Materials { get; } = new();

        private string _filter = "";
        public string Filter
        {
            get => _filter;
            set { _filter = value; OnPropertyChanged(); LoadMaterials(); }
        }

        public ICommand RefreshCommand { get; }

        public MaterialsViewModel()
        {
            RefreshCommand = new RelayCommand(_ => LoadMaterials());
            LoadMaterials();
        }

        private async void LoadMaterials()
        {
            Materials.Clear();
            var files = await _materialService.ScanMaterialsAsync();
            foreach (var file in files)
            {
                var item = new MaterialItem
                {
                    Info = file,
                    OpenCommand = new RelayCommand(_ => OpenMaterial(file))
                };
                if (string.IsNullOrEmpty(Filter) || file.Name.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                    Materials.Add(item);
            }
        }

        private async void OpenMaterial(MaterialInfo info)
        {
            if (info.Extension?.ToLower() == ".txt")
            {
                var loadResult = await _materialService.LoadMaterialAsync(info.RelativePath);
                if (loadResult.Success && loadResult.Content != null)
                {
                    string content = System.Text.Encoding.UTF8.GetString(loadResult.Content);
                    var editor = new TextEditorWindow(info.FullPath, content);
                    if (editor.ShowDialog() == true)
                    {
                        // перезагружаем список после сохранения
                        LoadMaterials();
                    }
                }
                else
                {
                    MessageBox.Show("Не удалось загрузить файл", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                // Для остальных типов можно открывать стандартным приложением
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(info.FullPath) { UseShellExecute = true });
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? n = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}