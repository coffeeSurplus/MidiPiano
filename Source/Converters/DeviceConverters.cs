using System.Globalization;
using System.Windows.Data;
using Windows.Devices.Enumeration;
using Windows.Devices.Midi;

namespace MidiPiano.Source.Converters;

internal class DeviceButtonRecordPlayConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) => ((bool)values[0], (bool)values[1]) switch { (true, true) => "end", (true, false) => "record", (false, true) => "pause", (false, false) => "play" };
	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => [Binding.DoNothing, Binding.DoNothing];
}

internal class DeviceButtonClearRestartConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) => ((bool)values[0], (bool)values[1]) switch { (true, true) => "reset", (true, false) => "clear", (false, true) => "replay", (false, false) => "restart" };
	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => [Binding.DoNothing, Binding.DoNothing];
}

internal class DeviceInputNameConverter : IValueConverter
{
	public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) => value != null ? ((DeviceInformation)value).Name : null;
	public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value != null ? Task.Run(GetInputDevices).Result.First(x => x.Name == (string)value) : null;

	private async Task<DeviceInformationCollection> GetInputDevices() => await DeviceInformation.FindAllAsync(MidiInPort.GetDeviceSelector());
}

internal class DeviceNamesConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => ((DeviceInformationCollection)value).Select(x => x.Name);
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

internal class DeviceOutputNameConverter : IValueConverter
{
	public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) => value != null ? ((DeviceInformation)value).Name : null;
	public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value != null ? Task.Run(GetOutputDevices).Result.First(x => x.Name == (string)value) : null;

	private static async Task<DeviceInformationCollection> GetOutputDevices() => await DeviceInformation.FindAllAsync(MidiOutPort.GetDeviceSelector());
}