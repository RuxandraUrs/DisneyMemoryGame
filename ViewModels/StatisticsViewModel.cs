using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace MemoryGame.ViewModels
{
    public class StatisticsEntry
    {
        public string PlayerName { get; set; }
        public int GamesPlayed { get; set; }
        public int GamesWon { get; set; }
    }

    public class StatisticsViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<StatisticsEntry> _statistics;
        public ObservableCollection<StatisticsEntry> Statistics
        {
            get => _statistics;
            set
            {
                _statistics = value;
                OnPropertyChanged(nameof(Statistics));
            }
        }

        public StatisticsViewModel()
        {
            LoadStatistics();
        }

        private void LoadStatistics()
        {
            var statsList = new ObservableCollection<StatisticsEntry>();
            string filePath = "statistics.txt";

            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    var parts = line.Split('-');
                    if (parts.Length >= 2)
                    {
                        string name = parts[0].Trim();
                        var numbers = parts[1].Split(',');
                        if (numbers.Length >= 2 &&
                            int.TryParse(numbers[0].Trim(), out int played) &&
                            int.TryParse(numbers[1].Trim(), out int won))
                        {
                            statsList.Add(new StatisticsEntry
                            {
                                PlayerName = name,
                                GamesPlayed = played,
                                GamesWon = won
                            });
                        }
                    }
                }
            }
            Statistics = statsList;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
