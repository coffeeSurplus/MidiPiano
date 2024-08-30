using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MidiPiano.Source.Converters;

internal class KeysBlackConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => Application.Current.FindResource((bool)value ? "Colour2" : "Black");
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

internal class KeysWhiteConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => Application.Current.FindResource((bool)value ? "Colour1" : "White");
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}