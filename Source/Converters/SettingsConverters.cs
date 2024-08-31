using System.Globalization;
using System.Windows.Data;
using Windows.Devices.Enumeration;
using Windows.Devices.Midi;

namespace MidiPiano.Source.Converters;

internal class SettingsButtonClearRestartConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) => ((bool)values[0], (bool)values[1]) switch { (true, true) => "reset", (true, false) => "clear", (false, true) => "replay", (false, false) => "restart" };
	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => [Binding.DoNothing, Binding.DoNothing];
}

internal class SettingsButtonRecordPlayConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) => ((bool)values[0], (bool)values[1]) switch { (true, true) => "end", (true, false) => "record", (false, true) => "pause", (false, false) => "play" };
	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => [Binding.DoNothing, Binding.DoNothing];
}

internal class SettingsDeviceInputNameConverter : IValueConverter
{
	public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is not null ? ((DeviceInformation)value).Name : null;
	public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value is not null ? Task.Run(GetInputDevices).Result.First(x => x.Name == (string)value) : null;

	private async Task<DeviceInformationCollection> GetInputDevices() => await DeviceInformation.FindAllAsync(MidiInPort.GetDeviceSelector());
}

internal class SettingsDeviceNamesConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => ((DeviceInformationCollection)value).Select(x => x.Name);
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

internal class SettingsDeviceOutputNameConverter : IValueConverter
{
	public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is not null ? ((DeviceInformation)value).Name : null;
	public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value is not null ? Task.Run(GetOutputDevices).Result.First(x => x.Name == (string)value) : null;

	private async Task<DeviceInformationCollection> GetOutputDevices() => await DeviceInformation.FindAllAsync(MidiOutPort.GetDeviceSelector());
}