using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace MemoryGame.Helpers
{
    public class CardImageConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || !(values[0] is bool isFlipped) || !(values[1] is string imagePath))
                return null;

            if (!isFlipped)
                return null;

            try
            {
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(imagePath, UriKind.Absolute);
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.EndInit();
                return bi;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading image: " + ex.Message);
                return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
