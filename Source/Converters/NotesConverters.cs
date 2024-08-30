using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MidiPiano.Source.Converters;

internal class NotesCanvasDirectionConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (bool)value ? -1 : 1;
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

internal class NotesCanvasPositionConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) => new Thickness(0, (bool)values[1] ? ((TimeSpan)values[0]).TotalMilliseconds / -10 + 500 : 0, 0, (bool)values[1] ? 0 : ((TimeSpan)values[0]).TotalMilliseconds / -10);
	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => [Binding.DoNothing, Binding.DoNothing];
}

internal class NotesXPositionConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (int)value * 10;
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

internal class NotesYPositionConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => ((TimeSpan)value).TotalMilliseconds / 10;
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}