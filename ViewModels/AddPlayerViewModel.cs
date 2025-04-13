using MemoryGame.Helpers;
using memoryGame.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace memoryGame.ViewModels
{
    public class AddPlayerViewModel : BaseViewModel
    {
        private ObservableCollection<string> _imagePaths;
        private int _currentImageIndex;
        private string _selectedImage;
        private string _playerName;

        public AddPlayerViewModel()
        {
            NextImageCommand = new RelayCommand(ExecuteNextImage, CanExecuteImageNavigation);
            PreviousImageCommand = new RelayCommand(ExecutePreviousImage, CanExecuteImageNavigation);
            SavePlayerCommand = new RelayCommand(ExecuteSavePlayer, CanExecuteSavePlayer);

            LoadImages();
        }

        public string PlayerName
        {
            get { return _playerName; }
            set
            {
                _playerName = value;
                OnPropertyChanged();
                (SavePlayerCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string SelectedImage
        {
            get { return _selectedImage; }
            set
            {
                _selectedImage = value;
                OnPropertyChanged();
            }
        }

        public ICommand NextImageCommand { get; }
        public ICommand PreviousImageCommand { get; }
        public ICommand SavePlayerCommand { get; }

        private void LoadImages()
        {
            string folder = "D:/an2/sem2/MVP/Teme/Tema 2 - MemoryGame/memoryGame/Resources/ProfilImages";

            if (Directory.Exists(folder))
            {
                var files = Directory.GetFiles(folder, "*.jpg");

                _imagePaths = new ObservableCollection<string>(files);

                if (_imagePaths.Count > 0)
                {
                    _currentImageIndex = 0;
                    SelectedImage = _imagePaths[_currentImageIndex];
                }
            }
            else
            {
                _imagePaths = new ObservableCollection<string>();
            }
        }

        private bool CanExecuteImageNavigation(object parameter)
        {
            return _imagePaths != null && _imagePaths.Count > 0;
        }

        private void ExecuteNextImage(object parameter)
        {
            if (_imagePaths != null && _imagePaths.Count > 0)
            {
                _currentImageIndex++;
                if (_currentImageIndex >= _imagePaths.Count)
                    _currentImageIndex = 0;

                SelectedImage = _imagePaths[_currentImageIndex];
            }
        }

        private void ExecutePreviousImage(object parameter)
        {
            if (_imagePaths != null && _imagePaths.Count > 0)
            {
                _currentImageIndex--;
                if (_currentImageIndex < 0)
                    _currentImageIndex = _imagePaths.Count - 1;

                SelectedImage = _imagePaths[_currentImageIndex];
            }
        }

        private bool CanExecuteSavePlayer(object parameter)
        {
            return !string.IsNullOrWhiteSpace(PlayerName);
        }

        private void ExecuteSavePlayer(object parameter)
        {
            try
            {
                var newPlayer = new Player
                {
                    Name = PlayerName,
                    ImagePath = SelectedImage
                };

                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "players.txt");
                string playerData = $"{PlayerName};{SelectedImage}{Environment.NewLine}";
                File.AppendAllText(filePath, playerData);

                PlayerManager.CurrentPlayer = newPlayer;

                if (parameter is Window window)
                {
                    if (window is memoryGame.Views.AddPlayerWindow addPlayerWindow)
                    {
                        addPlayerWindow.NewPlayer = newPlayer;
                    }
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la salvarea datelor: {ex.Message}",
                                "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}