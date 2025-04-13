using System.Windows;
using MemoryGame.ViewModels;

namespace MemoryGame.Views
{
    public partial class StatisticsWindow : Window
    {
        public StatisticsWindow()
        {
            InitializeComponent();
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            var startPage = new StartPage();
            startPage.Show();
            this.Close();
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            var gameOptionsWindow = new GameOptionsWindow();
            if (gameOptionsWindow.ShowDialog() == true)
            {
                string selectedMode = gameOptionsWindow.SelectedMode;
                string category = "default";
                var gameViewModel = new GameViewModel();

                if (selectedMode.Equals("Custom", StringComparison.OrdinalIgnoreCase))
                {
                    category = "custom";
                    gameViewModel.NewGame(category, gameOptionsWindow.SelectedTime,
                                          gameOptionsWindow.SelectedRows, gameOptionsWindow.SelectedColumns);
                }
                else if (selectedMode.Equals("StandardVillains", StringComparison.OrdinalIgnoreCase))
                {
                    category = "villains";
                    gameViewModel.NewGame(category, gameOptionsWindow.SelectedTime);
                }
                else if (selectedMode.Equals("StandardPrincesses", StringComparison.OrdinalIgnoreCase))
                {
                    category = "princesses";
                    gameViewModel.NewGame(category, gameOptionsWindow.SelectedTime);
                }
                else if (selectedMode.Equals("StandardFriends", StringComparison.OrdinalIgnoreCase))
                {
                    category = "friends";
                    gameViewModel.NewGame(category, gameOptionsWindow.SelectedTime);
                }
                else
                {
                    gameViewModel.NewGame(category, gameOptionsWindow.SelectedTime);
                }

                var gameWindow = new GameWindow();
                gameWindow.DataContext = gameViewModel;
                gameWindow.Show();

                this.Close();
            }
        }

    }
}
