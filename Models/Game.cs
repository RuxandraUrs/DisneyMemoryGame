using System.Windows;
using MemoryGame.Views;

namespace MemoryGame.Models
{
    public class Game
    {
        public List<Card> Cards { get; private set; }
        public bool IsTimerRunning { get; private set; }
        public bool IsGameOver { get; set; }

        private System.Timers.Timer _timer;
        private TimeSpan _remainingTime;
        private Card _firstFlippedCard;
        private Card _secondFlippedCard;
        private bool _isProcessing;

        public event Action<TimeSpan> TimerUpdated;

        public Game()
        {
            IsTimerRunning = false;
            IsGameOver = false;
            _isProcessing = false;
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += (s, e) => UpdateCountdownTime();
        }

        public void SetStartingTime(int seconds)
        {
            _remainingTime = TimeSpan.FromSeconds(seconds);
        }

        private void UpdateCountdownTime()
        {
            if (IsTimerRunning)
            {
                _remainingTime = _remainingTime - TimeSpan.FromSeconds(1);
                TimerUpdated?.Invoke(_remainingTime);

                if (_remainingTime <= TimeSpan.Zero)
                {
                    IsTimerRunning = false;
                    _timer.Stop();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var resultWindow = new ResultWindow();
                        resultWindow.SetResult(false, TimeSpan.Zero);
                        resultWindow.ShowDialog();

                        if (resultWindow.NewGameRequested)
                        {
                            OpenNewGameOptions();
                        }
                    });
                    IsGameOver = true;
                }
            }
        }

        public void StartNewGame(List<string> selectedImages)
        {
            Cards = new List<Card>();
            int id = 1;
            foreach (var image in selectedImages)
            {
                Cards.Add(new Card { PairId = id, ImagePath = image, IsFlipped = false, IsMatched = false });
                Cards.Add(new Card { PairId = id, ImagePath = image, IsFlipped = false, IsMatched = false });
                id++;
            }
            Cards = Cards.OrderBy(c => Guid.NewGuid()).ToList();

            IsTimerRunning = false;
            IsGameOver = false;
            _isProcessing = false;
            _firstFlippedCard = null;
            _secondFlippedCard = null;
        }

        public void FlipCard(Card card)
        {
            if (card == null || card.IsFlipped || card.IsMatched || IsGameOver || _isProcessing)
                return;

            if (!IsTimerRunning)
            {
                IsTimerRunning = true;
                _timer.Start();
            }

            card.IsFlipped = true;

            if (_firstFlippedCard == null)
            {
                _firstFlippedCard = card;
            }
            else if (_secondFlippedCard == null)
            {
                _secondFlippedCard = card;
                _isProcessing = true;
                CheckForMatch();
            }
        }

        private async void CheckForMatch()
        {
            if (_firstFlippedCard.PairId == _secondFlippedCard.PairId)
            {
                _firstFlippedCard.IsMatched = true;
                _secondFlippedCard.IsMatched = true;
            }
            else
            {
                await Task.Delay(700);
                _firstFlippedCard.IsFlipped = false;
                _secondFlippedCard.IsFlipped = false;
            }
            if (Cards.All(c => c.IsMatched))
            {
                IsTimerRunning = false;
                _timer.Stop();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var resultWindow = new ResultWindow();
                    resultWindow.SetResult(true, _remainingTime);
                    resultWindow.ShowDialog();

                    if (resultWindow.NewGameRequested)
                    {
                        OpenNewGameOptions();
                    }
                });
            }
            _firstFlippedCard = null;
            _secondFlippedCard = null;
            _isProcessing = false;
        }

        public void ResumeTimer()
        {
            IsTimerRunning = true;
            _timer.Start();
        }

        private void OpenNewGameOptions()
        {
            var gameOptionsWindow = new Views.GameOptionsWindow();
            if (gameOptionsWindow.ShowDialog() == true)
            {
                string selectedMode = gameOptionsWindow.SelectedMode;
                int selectedTime = gameOptionsWindow.SelectedTime;
                string category;
                var gameViewModel = new MemoryGame.ViewModels.GameViewModel();

                if (selectedMode.Equals("Custom", StringComparison.OrdinalIgnoreCase))
                {
                    category = "custom";
                    gameViewModel.NewGame(category, selectedTime, gameOptionsWindow.SelectedRows, gameOptionsWindow.SelectedColumns);
                }
                else if (selectedMode.Equals("StandardVillains", StringComparison.OrdinalIgnoreCase))
                {
                    category = "villains";
                    gameViewModel.NewGame(category, selectedTime);
                }
                else if (selectedMode.Equals("StandardPrincesses", StringComparison.OrdinalIgnoreCase))
                {
                    category = "princesses";
                    gameViewModel.NewGame(category, selectedTime);
                }
                else if (selectedMode.Equals("StandardFriends", StringComparison.OrdinalIgnoreCase))
                {
                    category = "friends";
                    gameViewModel.NewGame(category, selectedTime);
                }
                else
                {
                    category = "default";
                    gameViewModel.NewGame(category, selectedTime);
                }

                var gameWindow = new Views.GameWindow();
                gameWindow.DataContext = gameViewModel;
                gameWindow.Show();
            }
        }

    }
}
