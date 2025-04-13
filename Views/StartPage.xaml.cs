using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MemoryGame.ViewModels;

namespace MemoryGame.Views
{
    public partial class StartPage : Window
    {
        public StartPage()
        {
            InitializeComponent();
            DataContext = new StartPageViewModel();
        }
    }
}
