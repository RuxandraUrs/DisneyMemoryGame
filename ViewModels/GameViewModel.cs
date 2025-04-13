using MemoryGame.Models;
using MemoryGame.Helpers;
using MemoryGame.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace MemoryGame.ViewModels
{
    public class GameViewModel : INotifyPropertyChanged
    {
        private Game _game;
        public ObservableCollection<Card> Cards { get; set; }

        private string _timerText;
        public string TimerText
        {
            get => _timerText;
            set
            {
                _timerText = value;
                OnPropertyChanged(nameof(TimerText));
            }
        }

        private bool _isGameLost;
        public bool IsGameLost
        {
            get => _isGameLost;
            set
            {
                _isGameLost = value;
                OnPropertyChanged(nameof(IsGameLost));
            }
        }

        private int _boardRows;
        public int BoardRows
        {
            get => _boardRows;
            set
            {
                if (_boardRows != value)
                {
                    _boardRows = value;
                    OnPropertyChanged(nameof(BoardRows));
                }
            }
        }

        private int _boardColumns;
        public int BoardColumns
        {
            get => _boardColumns;
            set
            {
                if (_boardColumns != value)
                {
                    _boardColumns = value;
                    OnPropertyChanged(nameof(BoardColumns));
                }
            }
        }

        public string PlayerName => PlayerManager.CurrentPlayer?.Name;

        private bool _statsUpdated;

        public ICommand NewGameCommand { get; }
        public ICommand OpenGameCommand { get; }
        public ICommand SaveGameCommand { get; }
        public ICommand ViewStatisticsCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand FlipCardCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public GameViewModel()
        {
            NewGameCommand = new RelayCommand(param => NewGameWindow(param));
            OpenGameCommand = new RelayCommand(_ => OpenGame());
            SaveGameCommand = new RelayCommand(_ => SaveGame());
            ViewStatisticsCommand = new RelayCommand(_ => ViewStatistics());
            HomeCommand = new RelayCommand(param => Home(param));
            FlipCardCommand = new RelayCommand(cardObj => FlipCard(cardObj as Card));

            Cards = new ObservableCollection<Card>();
            TimerText = "00:00";
            IsGameLost = false;
        }
        public void NewGame(string category, int selectedTimeInSeconds = 90, int? customRows = null, int? customColumns = null)
        {
            _statsUpdated = false;
            List<string> selectedImages;

            if (string.Equals(category, "custom", StringComparison.OrdinalIgnoreCase))
            {
                if (customRows == null || customColumns == null)
                    throw new ArgumentException("Pentru modul custom trebuie specificate numarul de randuri si coloane.");

                BoardRows = customRows.Value;
                BoardColumns = customColumns.Value;

                selectedImages = GetRandomImagesForCustom(BoardRows, BoardColumns);
            }
            else
            {
                BoardRows = 4;
                BoardColumns = 4;
                selectedImages = GetRandomImagesForCategory(category);
            }


            if (selectedImages == null || selectedImages.Count == 0)
            {
                MessageBox.Show("Nu au fost gasite imagini pentru jocul selectat.");
                return;
            }

            _game = new Game();

            if (selectedTimeInSeconds > 180)
                selectedTimeInSeconds = 180;

            _game.SetStartingTime(selectedTimeInSeconds);

            _game.TimerUpdated += (time) =>
            {
                TimerText = time.ToString(@"mm\:ss");
                OnPropertyChanged(nameof(TimerText));

                if (time <= TimeSpan.Zero && !_statsUpdated)
                {
                    _statsUpdated = true;
                    IsGameLost = true;
                    StatisticsManager.UpdateStatistics(PlayerName, false);
                    DeleteSavedGameFile();
                }
            };

            _game.StartNewGame(selectedImages);

            Cards.Clear();
            foreach (var card in _game.Cards)
                Cards.Add(card);

            TimerText = TimeSpan.FromSeconds(selectedTimeInSeconds).ToString(@"mm\:ss");
            IsGameLost = false;
        }


        private List<string> GetRandomImagesForCategory(string category)
        {
            string subfolder = category.ToLower() switch
            {
                "custom" => "CustomImages",
                "friends" => "FriendsImages",
                "princesses" => "PrincessesImages",
                "villains" => "VillainsImages",
                _ => "CustomImages"
            };

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string folderPath = Path.Combine(baseDirectory, "Resources", subfolder);

            List<string> allImages = new List<string>();
            for (int i = 1; i <= 12; i++)
            {
                string filePath = Path.Combine(folderPath, $"{i}.jpg");
                if (File.Exists(filePath))
                    allImages.Add(filePath);
            }

            Random random = new Random();
            allImages = allImages.OrderBy(x => random.Next()).ToList();

            int numberOfCards = Math.Min(8, allImages.Count);
            return allImages.GetRange(0, numberOfCards);
        }

        private List<string> GetRandomImagesForCustom(int rows, int columns)
        {
            try
            {
                int numberOfPairs = (rows * columns) / 2;
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string folderPath = Path.Combine(baseDirectory, "Resources", "CustomImages");

                if (!Directory.Exists(folderPath))
                {
                    MessageBox.Show("Folder-ul CustomImages nu exista. Adauga imagini in Resources/CustomImages.");
                    return new List<string>();
                }

                List<string> allImages = new List<string>();
                allImages.AddRange(Directory.GetFiles(folderPath, "*.jpg"));
                allImages.AddRange(Directory.GetFiles(folderPath, "*.png"));

                if (allImages.Count < numberOfPairs)
                {
                    MessageBox.Show("Nu sunt suficiente imagini in folderul CustomImages pentru a incepe jocul.");
                    return new List<string>();
                }

                Random random = new Random();
                return allImages.OrderBy(x => random.Next()).Take(numberOfPairs).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("A aparut o eroare la incarca imaginilor custom: " + ex.Message);
                return new List<string>();
            }
        }

        private void FlipCard(Card card)
        {
            if (card == null)
                return;

            _game.FlipCard(card);
            OnPropertyChanged(nameof(Cards));

            if (_game.Cards.All(c => c.IsMatched) && !_statsUpdated)
            {
                _statsUpdated = true;
                StatisticsManager.UpdateStatistics(PlayerName, true);
                DeleteSavedGameFile();
            }
        }

        private void OpenGame()
        {
            string fileName = $"{PlayerName}_game.txt";
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            if (!File.Exists(filePath))
                return;

            string[] lines = File.ReadAllLines(filePath);
            if (lines.Length < 2)
                return;

            string savedTimerText = lines[0];
            TimerText = savedTimerText;
            if (!TryParseTimerText(savedTimerText, out TimeSpan remainingTime))
                return;

            List<Card> loadedCards = new List<Card>();
            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(';');
                if (parts.Length >= 4)
                {
                    int pairId = int.Parse(parts[0]);
                    bool isFlipped = bool.Parse(parts[1]);
                    bool isMatched = bool.Parse(parts[2]);
                    string imagePath = parts[3];
                    loadedCards.Add(new Card
                    {
                        PairId = pairId,
                        IsFlipped = isFlipped,
                        IsMatched = isMatched,
                        ImagePath = imagePath
                    });
                }
            }

            _game = new Game();
            var cardsField = typeof(Game).GetField("<Cards>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            if (cardsField != null)
                cardsField.SetValue(_game, loadedCards);

            var remainingTimeField = typeof(Game).GetField("_remainingTime", BindingFlags.Instance | BindingFlags.NonPublic);
            if (remainingTimeField != null)
                remainingTimeField.SetValue(_game, remainingTime);

            _game.TimerUpdated += (time) =>
            {
                TimerText = time.ToString(@"mm\:ss");
                OnPropertyChanged(nameof(TimerText));
                if (time <= TimeSpan.Zero && !_statsUpdated)
                {
                    _statsUpdated = true;
                    IsGameLost = true;
                    StatisticsManager.UpdateStatistics(PlayerName, false);
                    DeleteSavedGameFile();
                }
            };

            Cards.Clear();
            foreach (var card in loadedCards)
                Cards.Add(card);

            var timerField = typeof(Game).GetField("_timer", BindingFlags.Instance | BindingFlags.NonPublic);
            System.Timers.Timer timer = null;
            if (timerField != null)
            {
                timer = timerField.GetValue(_game) as System.Timers.Timer;
                if (!loadedCards.All(c => c.IsMatched) && remainingTime > TimeSpan.Zero)
                    _game.ResumeTimer();
                else
                {
                    timer.Stop();
                    _game.IsGameOver = true;
                    DeleteSavedGameFile();
                }
            }
        }

        private bool TryParseTimerText(string timerText, out TimeSpan time)
        {
            string[] parts = timerText.Split(':');
            if (parts.Length == 2)
            {
                if (int.TryParse(parts[0], out int minutes) && int.TryParse(parts[1], out int seconds))
                {
                    time = TimeSpan.FromSeconds(minutes * 60 + seconds);
                    return true;
                }
            }
            else if (parts.Length == 3)
            {
                if (int.TryParse(parts[0], out int hours) &&
                    int.TryParse(parts[1], out int minutes) &&
                    int.TryParse(parts[2], out int seconds))
                {
                    time = new TimeSpan(hours, minutes, seconds);
                    return true;
                }
            }
            time = TimeSpan.Zero;
            return false;
        }

        private void SaveGame()
        {
            if (_game == null)
            {
                MessageBox.Show("Nu exista un joc activ.");
                return;
            }
            if (_game.Cards.All(c => c.IsMatched))
            {
                MessageBox.Show("Jocul este finalizat. Nu puteti salva un joc finalizat.");
                return;
            }

            string fileName = $"{PlayerName}_game.txt";
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

            List<string> lines = new List<string> { TimerText };
            foreach (var card in _game.Cards)
            {
                lines.Add($"{card.PairId};{card.IsFlipped};{card.IsMatched};{card.ImagePath}");
            }
            File.WriteAllLines(filePath, lines);
            MessageBox.Show("Jocul a fost salvat cu succes!");
        }

        private void ViewStatistics()
        {
            var statisticsWindow = new StatisticsWindow();
            statisticsWindow.DataContext = new StatisticsViewModel();
            statisticsWindow.Show();
        }

        private void Home(object parameter)
        {
            var startPage = new StartPage();
            startPage.Show();
            if (parameter is Window window)
                window.Close();
        }

        public void NewGameWindow(object parameter)
        {
            var gameOptionsWindow = new GameOptionsWindow();
            if (gameOptionsWindow.ShowDialog() == true)
            {
                string category = gameOptionsWindow.SelectedMode switch
                {
                    "Custom" => "custom",
                    "StandardVillains" => "villains",
                    "StandardPrincesses" => "princesses",
                    "StandardFriends" => "friends",
                    _ => "default"
                };

                if (category.Equals("custom", StringComparison.OrdinalIgnoreCase))
                {
                    NewGame(category,
                            gameOptionsWindow.SelectedTime,
                            gameOptionsWindow.SelectedRows,
                            gameOptionsWindow.SelectedColumns);
                }
                else
                {
                    NewGame(category, gameOptionsWindow.SelectedTime);
                }

                var gameWindow = new GameWindow
                {
                    DataContext = this
                };
                gameWindow.Show();

                if (parameter is Window parentWindow)
                {
                    parentWindow.Close();
                }
            }
        }

        private void DeleteSavedGameFile()
        {
            string fileName = $"{PlayerName}_game.txt";
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
