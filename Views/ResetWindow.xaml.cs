using System;
using System.Windows;

namespace MemoryGame.Views
{
    public partial class ResultWindow : Window
    {
        public bool NewGameRequested { get; private set; }

        public ResultWindow()
        {
            InitializeComponent();
        }

        public void SetResult(bool isWin, TimeSpan remainingTime)
        {
            if (isWin)
            {
                resultText.Text = "You win";
                timeText.Text = "Time left: " + remainingTime.ToString(@"mm\:ss");
                timeText.Visibility = Visibility.Visible;
            }
            else
            {
                resultText.Text = "You lost";
                timeText.Visibility = Visibility.Collapsed;
            }
        }

        private void btnNewGame_Click(object sender, RoutedEventArgs e)
        {
            NewGameRequested = true;
            this.Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            NewGameRequested = false;
            this.Close();
        }
    }
}
