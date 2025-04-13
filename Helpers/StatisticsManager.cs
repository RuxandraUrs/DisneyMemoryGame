using System.IO;

namespace MemoryGame.Helpers
{
    public static class StatisticsManager
    {
        private static readonly string FilePath = "statistics.txt";

        public static void UpdateStatistics(string playerName, bool isWin)
        {
            var stats = new Dictionary<string, (int played, int won)>();

            if (File.Exists(FilePath))
            {
                var lines = File.ReadAllLines(FilePath);
                foreach (var line in lines)
                {
                    var parts = line.Split('-');
                    if (parts.Length >= 2)
                    {
                        var name = parts[0].Trim();
                        var numbers = parts[1].Split(',');
                        if (numbers.Length >= 2 &&
                            int.TryParse(numbers[0].Trim(), out int played) &&
                            int.TryParse(numbers[1].Trim(), out int won))
                        {
                            stats[name] = (played, won);
                        }
                    }
                }
            }

            if (stats.ContainsKey(playerName))
            {
                var current = stats[playerName];
                current.played++;
                if (isWin)
                    current.won++;
                stats[playerName] = current;
            }
            else
            {
                stats[playerName] = (1, isWin ? 1 : 0);
            }

            using (var sw = new StreamWriter(FilePath, false))
            {
                foreach (var kv in stats)
                {
                    sw.WriteLine($"{kv.Key} - {kv.Value.played}, {kv.Value.won}");
                }
            }
        }
    }
}
