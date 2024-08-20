using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MidiPiano.Source.Converters;

internal class KeysBlackConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => new SolidColorBrush((Color)ColorConverter.ConvertFromString((bool)value ? "#19223F" : "#000000"));
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

internal class KeysWhiteConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => new SolidColorBrush((Color)ColorConverter.ConvertFromString((bool)value ? "#19223F" : "#FFFFFF"));
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}