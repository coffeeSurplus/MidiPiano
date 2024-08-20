using System.Globalization;
using System.Windows.Data;

namespace MidiPiano.Source.Converters;

internal class TimerElapsedConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => $"elapsed: {(((TimeSpan)value).Hours > 0 ? $"{((TimeSpan)value).Hours:00}:" : string.Empty)}{((TimeSpan)value).Minutes:00}:{((TimeSpan)value).Seconds:00}.{((TimeSpan)value).Milliseconds / 10:00}";
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}