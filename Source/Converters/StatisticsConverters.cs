using MidiPiano.Source.MVVM;
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
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => $"highest note: {(((int)value) != -1 ? ((int)value).ConvertToNote() : "n/a")}";
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

internal class StatisticsLowestConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => $"lowest note: {(((int)value) != -1 ? ((int)value).ConvertToNote() : "n/a")}";
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

internal class StatisticsMaxConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => $"max frequency: {value}";
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

internal class StatisticsModeConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => $"modal {(((int[])value).Length != 1 ? "notes" : "note")}: {(((int[])value).Length != 0 ? string.Join(", ", ((int[])value).Select(x => x.ConvertToNote())) : "n/a")}";
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

internal class StatisticsTotalConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => $"total notes: {value}";
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}