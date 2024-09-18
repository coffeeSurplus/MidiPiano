using MidiPiano.Source.Converters;
using MidiPiano.Source.Models;
using MidiPiano.Source.MVVM;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using Windows.Devices.Enumeration;
using Windows.Devices.Midi;

namespace MidiPiano.Source.ViewModels;

internal class MainWindowViewModel : ViewModelBase
{
	private readonly DeviceWatcher deviceWatcher = DeviceInformation.CreateWatcher($"{MidiInPort.GetDeviceSelector()} OR {MidiOutPort.GetDeviceSelector()}");
	private readonly DispatcherTimer dispatcherTimer = new() { Interval = new(0, 0, 0, 0, 10) };
	private readonly Stopwatch stopwatch = new();

	private MidiInPort? midiInPort = null;
	private MidiOutPort? midiOutPort = null;
	private bool isEnded = false;
	private int index = 0;

	private DeviceInformationCollection inputDevices = Task.Run(GetInputDevices).Result;
	private DeviceInformationCollection outputDevices = Task.Run(GetOutputDevices).Result;
	private DeviceInformation? inputDevice = null;
	private DeviceInformation? outputDevice = null;
	private TimeSpan currentTime = TimeSpan.Zero;
	private bool recordMode = true;
	private bool isRunning = false;
	private bool notesGraph = true;
	private int damperLevel = 0;
	private int totalNotes = 0;
	private int maxFrequency = 0;
	private int[] modalNotes = [];
	private int? lowestNote = null;
	private int? highestNote = null;
	private int differentNotes = 0;
	private int graphHeight = 10;

	public DeviceInformationCollection InputDevices
	{
		get => inputDevices;
		set => SetValue(ref inputDevices, value);
	}
	public DeviceInformationCollection OutputDevices
	{
		get => outputDevices;
		set => SetValue(ref outputDevices, value);
	}
	public DeviceInformation? InputDevice
	{
		get => inputDevice;
		set => SetValue(ref inputDevice, value);
	}
	public DeviceInformation? OutputDevice
	{
		get => outputDevice;
		set => SetValue(ref outputDevice, value);
	}
	public TimeSpan CurrentTime
	{
		get => currentTime;
		set => SetValue(ref currentTime, value);
	}
	public bool RecordMode
	{
		get => recordMode;
		set => SetValue(ref recordMode, value);
	}
	public bool IsRunning
	{
		get => isRunning;
		set => SetValue(ref isRunning, value);
	}
	public bool NotesGraph
	{
		get => notesGraph;
		set => SetValue(ref notesGraph, value);
	}
	public int DamperLevel
	{
		get => damperLevel;
		set => SetValue(ref damperLevel, value);
	}
	public int TotalNotes
	{
		get => totalNotes;
		set => SetValue(ref totalNotes, value);
	}
	public int MaxFrequency
	{
		get => maxFrequency;
		set => SetValue(ref maxFrequency, value);
	}
	public int[] ModalNotes
	{
		get => modalNotes;
		set => SetValue(ref modalNotes, value);
	}
	public int? LowestNote
	{
		get => lowestNote;
		set => SetValue(ref lowestNote, value);
	}
	public int? HighestNote
	{
		get => highestNote;
		set => SetValue(ref highestNote, value);
	}
	public int DifferentNotes
	{
		get => differentNotes;
		set => SetValue(ref differentNotes, value);
	}
	public int GraphHeight
	{
		get => graphHeight;
		set => SetValue(ref graphHeight, value);
	}

	public ObservableCollection<IMidiMessage> MidiMessages { get; } = [];
	public ObservableCollection<NoteModel> Notes { get; } = [];
	public ObservableCollection<TimeSpan> LastPlayed { get; } = new(new TimeSpan[88]);
	public ObservableCollection<bool> Keys { get; } = new(new bool[88]);
	public ObservableCollection<int> Frequencies { get; } = new(new int[88]);

	public RelayCommand RecordSelectedCommand { get; }
	public RelayCommand PlaybackSelectedCommand { get; }
	public RelayCommand RecordPlayCommand { get; }
	public RelayCommand ClearRestartCommand { get; }

	public MainWindowViewModel()
	{
		RecordSelectedCommand = new(ClearRestart, CanRecordMode);
		PlaybackSelectedCommand = new(ClearRestart, CanPlaybackMode);
		RecordPlayCommand = new(RecordPlay, CanRecordPlay);
		ClearRestartCommand = new(ClearRestart, CanRecordPlay);
		deviceWatcher.Added += DeviceAdded;
		deviceWatcher.Removed += DeviceRemoved;
		dispatcherTimer.Tick += TimerElapsed;
		deviceWatcher.Start();
	}

	private void DeviceAdded(DeviceWatcher sender, DeviceInformation args) => (InputDevices, OutputDevices) = (Task.Run(GetInputDevices).Result, Task.Run(GetOutputDevices).Result);
	private void DeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
	{
		(InputDevices, OutputDevices) = (Task.Run(GetInputDevices).Result, Task.Run(GetOutputDevices).Result);
		if (InputDevice != null && !InputDevices.Contains(InputDevice))
		{
			InputDevice = null;
			Application.Current.Dispatcher.Invoke(CurrentDeviceLost);
		}
		if (OutputDevice != null && !OutputDevices.Contains(OutputDevice))
		{
			OutputDevice = null;
			Application.Current.Dispatcher.Invoke(CurrentDeviceLost);
		}
	}
	private void TimerElapsed(object? sender, EventArgs e)
	{
		CurrentTime = stopwatch.Elapsed;
		switch (recordMode)
		{
			case true:
				for (int index = 0; index < 88; index++)
				{
					if (Keys[index])
					{
						Notes.Last(x => x.Note == index).Duration = CurrentTime - LastPlayed[index];
					}
				}
				break;
			case false:
				switch (index < MidiMessages.Count)
				{
					case true:
						while (index < MidiMessages.Count && MidiMessages[index].Timestamp <= stopwatch.Elapsed)
						{
							try
							{
								midiOutPort?.SendMessage(MidiMessages[index]);
							}
							catch
							{
								return;
							}
							DistributeMessage(MidiMessages[index]);
							index++;
						}
						break;
					case false:
						IsRunning = false;
						isEnded = true;
						PlayEnd();
						break;
				}
				break;
		}
	}
	private void MidiMessageRecieved(MidiInPort sender, MidiMessageReceivedEventArgs args)
	{
		IMidiMessage message = args.Message is MidiNoteOnMessage noteOnMessage && Keys[noteOnMessage.Note - 21] ? new MidiNoteOffMessage(noteOnMessage.Channel, noteOnMessage.Note, noteOnMessage.Velocity) : args.Message;
		MidiMessages.Add(message);
		Application.Current.Dispatcher.Invoke(DistributeMessage, message);
	}

	private void RecordPlay()
	{
		switch (RecordMode, IsRunning)
		{
			case (true, true):
				RecordStart();
				break;
			case (true, false):
				RecordEnd();
				break;
			case (false, true):
				PlayStart();
				break;
			case (false, false):
				PlayEnd();
				break;
		}
	}
	private void ClearRestart()
	{
		switch (RecordMode)
		{
			case true:
				Clear();
				break;
			case false:
				Restart();
				break;
		}
	}
	private bool CanRecordPlay() => RecordMode ? InputDevice is not null : OutputDevice is not null;
	private bool CanRecordMode() => !IsRunning && InputDevice is not null;
	private bool CanPlaybackMode() => !IsRunning && OutputDevice is not null;

	private void RecordStart()
	{
		midiInPort = Task.Run(GetMidiInPort).Result;
		if (midiInPort is not null)
		{
			midiInPort.MessageReceived += MidiMessageRecieved;
			dispatcherTimer.Start();
			stopwatch.Start();
		}
	}
	private void RecordEnd()
	{
		dispatcherTimer.Stop();
		stopwatch.Stop();
		if (midiInPort is not null)
		{
			midiInPort.MessageReceived -= MidiMessageRecieved;
			midiInPort.Dispose();
			midiInPort = null;
		}
	}
	private void PlayStart()
	{
		if (isEnded)
		{
			Restart();
		}
		midiOutPort = Task.Run(GetMidiOutPort).Result;
		StopAllNotes();
		dispatcherTimer.Start();
		stopwatch.Start();
	}
	private void PlayEnd()
	{
		dispatcherTimer.Stop();
		stopwatch.Stop();
		StopAllNotes();
		midiOutPort?.Dispose();
		midiOutPort = null;
	}
	private void Clear()
	{
		RecordEnd();
		MidiMessages.Clear();
		Notes.Clear();
		Reset();
	}
	private void Restart()
	{
		PlayEnd();
		isEnded = false;
		index = 0;
		Reset();
	}

	private void DistributeMessage(IMidiMessage message)
	{
		switch (message.Type)
		{
			case MidiMessageType.NoteOn:
				Notes.Add(new(message.ConvertToNoteOn(), message.Timestamp));
				LastPlayed[message.ConvertToNoteOn()] = message.Timestamp;
				Frequencies[message.ConvertToNoteOn()]++;
				Keys[message.ConvertToNoteOn()] = true;
				TotalNotes = Frequencies.Sum();
				MaxFrequency = Frequencies.Max();
				ModalNotes = Frequencies.Select((x, i) => (x, i)).Where(x => x.x == MaxFrequency).Select(x => x.i).ToArray();
				LowestNote = Frequencies.TakeWhile(x => x == 0).Count();
				HighestNote = 87 - Frequencies.Reverse().TakeWhile(x => x == 0).Count();
				DifferentNotes = Frequencies.Count(x => x != 0);
				GraphHeight = MaxFrequency + 10;
				break;
			case MidiMessageType.NoteOff:
				Keys[message.ConvertToNoteOff()] = false;
				break;
			case MidiMessageType.ControlChange:
				if (((MidiControlChangeMessage)message).Controller is 64)
				{
					DamperLevel = ((MidiControlChangeMessage)message).ControlValue;
				}
				break;
		}
	}
	private void CurrentDeviceLost()
	{
		IsRunning = false;
		ClearRestart();
	}
	private void StopAllNotes()
	{
		try
		{
			for (byte channel = 0; channel < 16; channel++)
			{
				midiOutPort?.SendMessage(new MidiControlChangeMessage(channel, 120, 0));
			}
		}
		catch
		{
			return;
		}
	}
	private void Reset()
	{
		for (int index = 0; index < 88; index++)
		{
			LastPlayed[index] = TimeSpan.Zero;
			Keys[index] = false;
			Frequencies[index] = 0;
		}
		DamperLevel = 0;
		TotalNotes = 0;
		MaxFrequency = 0;
		ModalNotes = [];
		LowestNote = null;
		HighestNote = null;
		DifferentNotes = 0;
		GraphHeight = 10;
		CurrentTime = TimeSpan.Zero;
		stopwatch.Reset();
		if (IsRunning)
		{
			RecordPlay();
		}
	}

	private async Task<MidiInPort?> GetMidiInPort() => InputDevice is not null ? await MidiInPort.FromIdAsync(InputDevice.Id) : null;
	private async Task<MidiOutPort?> GetMidiOutPort() => OutputDevice is not null ? (MidiOutPort)await MidiOutPort.FromIdAsync(OutputDevice.Id) : null;
	private static async Task<DeviceInformationCollection> GetInputDevices() => await DeviceInformation.FindAllAsync(MidiInPort.GetDeviceSelector());
	private static async Task<DeviceInformationCollection> GetOutputDevices() => await DeviceInformation.FindAllAsync(MidiOutPort.GetDeviceSelector());
}