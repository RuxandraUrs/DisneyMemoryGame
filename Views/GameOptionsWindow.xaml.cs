using System.Windows;

namespace MemoryGame.Views
{
    public partial class GameOptionsWindow : Window
    {
        public string SelectedMode { get; set; }
        public int SelectedTime { get; set; }
        public int SelectedRows { get; set; }
        public int SelectedColumns { get; set; }

        public GameOptionsWindow()
        {
            InitializeComponent();
        }

        private void InitialCustom_Click(object sender, RoutedEventArgs e)
        {
            SelectedMode = "Custom";
            InitialPanel.Visibility = Visibility.Collapsed;
            CustomPanel.Visibility = Visibility.Visible;
        }

        private void CustomStart_Click(object sender, RoutedEventArgs e)
        {
            SelectedTime = (int)CustomTimeSlider.Value;
            SelectedRows = (int)RowsSlider.Value;
            SelectedColumns = (int)ColumnsSlider.Value;

            DialogResult = true;
            Close();
        }

        private void InitialStandard_Click(object sender, RoutedEventArgs e)
        {
            InitialPanel.Visibility = Visibility.Collapsed;
            StandardPanel.Visibility = Visibility.Visible;
        }

        private void StandardVillains_Click(object sender, RoutedEventArgs e)
        {
            SelectedMode = "StandardVillains";
            SelectedTime = (int)TimeSlider.Value;
            SelectedRows = 4;
            SelectedColumns = 4;
            DialogResult = true;
            Close();
        }

        private void StandardPrincesses_Click(object sender, RoutedEventArgs e)
        {
            SelectedMode = "StandardPrincesses";
            SelectedTime = (int)TimeSlider.Value;
            SelectedRows = 4;
            SelectedColumns = 4;
            DialogResult = true;
            Close();
        }

        private void StandardFriends_Click(object sender, RoutedEventArgs e)
        {
            SelectedMode = "StandardFriends";
            SelectedTime = (int)TimeSlider.Value;
            SelectedRows = 4;
            SelectedColumns = 4;
            DialogResult = true;
            Close();
        }
    }
  }
