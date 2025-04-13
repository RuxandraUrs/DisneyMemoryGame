using System.Windows;
using memoryGame.Models;
using memoryGame.ViewModels;

namespace memoryGame.Views
{
    public partial class AddPlayerWindow : Window
    {
        public Player NewPlayer { get; set; }
        public AddPlayerWindow()
        {
            InitializeComponent();
            DataContext = new AddPlayerViewModel();
        }

    }
}

