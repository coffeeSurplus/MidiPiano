using MidiPiano.Source.MVVM;

namespace MidiPiano.Source.Models;

internal class NoteModel(int note, TimeSpan startTime) : ViewModelBase
{
	private int note = note;
	private TimeSpan startTime = startTime;
	private TimeSpan duration = TimeSpan.Zero;

	public int Note
	{
		get => note;
		set => SetValue(ref note, value);
	}
	public TimeSpan StartTime
	{
		get => startTime;
		set => SetValue(ref startTime, value);
	}
	public TimeSpan Duration
	{
		get => duration;
		set => SetValue(ref duration, value);
	}
}