using MemoryGame.Views;
using MemoryGame.Helpers;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using memoryGame.Models;
using memoryGame.Views;

namespace MemoryGame.ViewModels
{
    public class StartPageViewModel : BaseViewModel
    {
        public ObservableCollection<Player> Players { get; set; }

        private Player _selectedPlayer;
        public Player SelectedPlayer
        {
            get => _selectedPlayer;
            set
            {
                _selectedPlayer = value;
                OnPropertyChanged();
                (DeletePlayerCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (StartGameCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public ICommand CloseGameCommand { get; set; }
        public ICommand AddPlayerCommand { get; set; }
        public ICommand DeletePlayerCommand { get; set; }
        public ICommand StartGameCommand { get; set; }

        public StartPageViewModel()
        {
            Players = new ObservableCollection<Player>();
            CloseGameCommand = new RelayCommand(ExecuteCloseGame);
            AddPlayerCommand = new RelayCommand(ExecuteAddPlayer);
            DeletePlayerCommand = new RelayCommand(ExecuteDeletePlayer, CanExecuteDeletePlayer);
            StartGameCommand = new RelayCommand(ExecuteStartGame, CanExecuteStartGame);

            LoadPlayers();
        }

        private void ExecuteCloseGame(object parameter)
        {
            Application.Current.Shutdown();
        }

        private void ExecuteAddPlayer(object parameter)
        {
            var addPlayerWindow = new AddPlayerWindow();
            if (addPlayerWindow.ShowDialog() == true)
            {
                if (addPlayerWindow.NewPlayer != null)
                {
                    Players.Add(addPlayerWindow.NewPlayer);
                }
            }
        }

        private bool CanExecuteDeletePlayer(object parameter)
        {
            return SelectedPlayer != null;
        }

        private void ExecuteDeletePlayer(object parameter)
        {
            if (SelectedPlayer == null)
                return;

            var playerToDelete = SelectedPlayer;
            Players.Remove(playerToDelete);

            string playersFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "players.txt");
            if (File.Exists(playersFilePath))
            {
                var playerLines = File.ReadAllLines(playersFilePath).ToList();
                string targetLine = $"{playerToDelete.Name};{playerToDelete.ImagePath}".Trim();
                playerLines.RemoveAll(line => line.Trim().Equals(targetLine, StringComparison.OrdinalIgnoreCase));
                File.WriteAllLines(playersFilePath, playerLines);
            }

            SyncStatistics();
        }

        private bool CanExecuteStartGame(object parameter)
        {
            return SelectedPlayer != null;
        }

        private void ExecuteStartGame(object parameter)
        {
            var gameOptionsWindow = new GameOptionsWindow();
            if (gameOptionsWindow.ShowDialog() == true)
            {
                string selectedMode = gameOptionsWindow.SelectedMode;
                string category = "default";
                switch (selectedMode)
                {
                    case "StandardVillains":
                        category = "villains";
                        break;
                    case "StandardPrincesses":
                        category = "princesses";
                        break;
                    case "StandardFriends":
                        category = "friends";
                        break;
                    case "Custom":
                        category = "custom";
                        break;
                    default:
                        category = "default";
                        break;
                }

                PlayerManager.CurrentPlayer = SelectedPlayer;

                var gameViewModel = new GameViewModel();
                if (category.Equals("custom", StringComparison.OrdinalIgnoreCase))
                    gameViewModel.NewGame(category, gameOptionsWindow.SelectedTime, gameOptionsWindow.SelectedRows, gameOptionsWindow.SelectedColumns);
                else
                    gameViewModel.NewGame(category, gameOptionsWindow.SelectedTime);

                var gameWindow = new GameWindow
                {
                    DataContext = gameViewModel
                };
                gameWindow.Show();

                if (parameter is Window window)
                    window.Close();
            }
        }

        private void LoadPlayers()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "players.txt");
            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    var parts = line.Split(';');
                    if (parts.Length >= 2)
                    {
                        var player = new Player
                        {
                            Name = parts[0],
                            ImagePath = parts[1].Trim()
                        };
                        Players.Add(player);
                    }
                }
            }

            SyncStatistics();
        }

        private void SyncStatistics()
        {
            string statsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "statistics.txt");
            if (File.Exists(statsFilePath))
            {
                var statsLines = File.ReadAllLines(statsFilePath).ToList();
                var validPlayerNames = Players.Select(p => p.Name.Trim().ToLower()).ToList();

                statsLines.RemoveAll(line =>
                {
                    line = line.Trim();
                    int separatorIndex = line.IndexOf('-');
                    if (separatorIndex < 0)
                        return false; 

                    string statPlayerName = line.Substring(0, separatorIndex).Trim().ToLower();
                    return !validPlayerNames.Contains(statPlayerName);
                });

                File.WriteAllLines(statsFilePath, statsLines);
            }
        }
    }
}
