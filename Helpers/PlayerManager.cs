using System.IO;
using memoryGame.Models;

namespace MemoryGame.Helpers
{
    public static class PlayerManager
    {
        private static readonly string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "players.txt");
        public static Player CurrentPlayer { get; set; }

        public static List<Player> LoadPlayers()
        {
            var players = new List<Player>();
            if (File.Exists(FilePath))
            {
                var lines = File.ReadAllLines(FilePath);
                foreach (var line in lines)
                {
                    var parts = line.Split(';');
                    if (parts.Length >= 2)
                    {
                        players.Add(new Player
                        {
                            Name = parts[0].Trim(),
                            ImagePath = parts[1].Trim()
                        });
                    }
                }
            }
            return players;
        }

        public static Player GetPlayerByName(string name)
        {
            return LoadPlayers().FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
