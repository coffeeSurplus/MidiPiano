using Windows.Devices.Midi;

namespace MidiPiano.Source.Converters;

internal static class ExtensionMethods
{
	public static string ConvertToNote(this int note) => $"{(note % 12) switch { 0 => "A", 1 => "A#", 2 => "B", 3 => "C", 4 => "C#", 5 => "D", 6 => "D#", 7 => "E", 8 => "F", 9 => "F#", 10 => "G", _ => "G#", }}{(note + 9) / 12}";
	public static int ConvertToNoteOn(this IMidiMessage args) => ((MidiNoteOnMessage)args).Note - 21;
	public static int ConvertToNoteOff(this IMidiMessage args) => ((MidiNoteOffMessage)args).Note - 21;
}