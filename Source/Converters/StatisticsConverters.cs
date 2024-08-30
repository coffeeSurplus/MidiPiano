using System.Globalization;
using System.Windows.Data;

namespace MidiPiano.Source.Converters;

internal class StatisticsDifferentConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => $"different notes: {value}";
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

internal class StatisticsHighestConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => $"highest note: {(value is not null ? ((int)value).ConvertToNote() : "n/a")}";
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

internal class StatisticsLowestConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => $"lowest note: {(value is not null ? ((int)value).ConvertToNote() : "n/a")}";
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

internal class StatisticsMaxConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => $"max frequency: {value}";
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

internal class StatisticsModeConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => $"modal {(((int[])value).Length is not 1 ? "notes" : "note")}: {(((int[])value) is not [] ? string.Join(", ", ((int[])value).Select(x => x.ConvertToNote())) : "n/a")}";
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

internal class StatisticsTimerConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => $"elapsed: {(((TimeSpan)value).Hours is not 0 ? $"{value:hh':'mm':'ss':'ff}" : $"{value:mm':'ss':'ff}")}";
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

internal class StatisticsTotalConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => $"total notes: {value}";
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}