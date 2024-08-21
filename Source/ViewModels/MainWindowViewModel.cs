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
	private readonly DispatcherTimer dispatcherTimer = new() { Interval = new(0, 0, 0, 0, 1) };
	private readonly Stopwatch stopwatch = new();
	private readonly List<IMidiMessage> recording = [];

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
	private bool linearGraph = true;
	private int damperLevel = 0;
	private int totalNotes = 0;
	private int maxFrequency = 0;
	private int[] modalNotes = [];
	private int lowestNote = -1;
	private int highestNote = -1;
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
	public bool LinearGraph
	{
		get => linearGraph;
		set => SetValue(ref linearGraph, value);
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
	public int LowestNote
	{
		get => lowestNote;
		set => SetValue(ref lowestNote, value);
	}
	public int HighestNote
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

	public ObservableCollection<int> Frequencies { get; } = new(new int[88]);
	public ObservableCollection<bool> Keys { get; } = new(new bool[88]);

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
		dispatcherTimer.Tick += TimerElapsed;
		deviceWatcher.Added += DeviceAdded;
		deviceWatcher.Removed += DeviceRemoved;
		deviceWatcher.Start();
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
	private bool CanRecordPlay() => RecordMode ? InputDevice != null : OutputDevice != null;
	private bool CanRecordMode() => !IsRunning && InputDevice != null;
	private bool CanPlaybackMode() => !IsRunning && OutputDevice != null;

	private void RecordStart()
	{
		midiInPort = Task.Run(GetMidiInPort).Result;
		if (midiInPort != null)
		{
			midiInPort.MessageReceived += MessageReceived;
		}
		dispatcherTimer.Start();
		stopwatch.Start();
	}
	private void RecordEnd()
	{
		dispatcherTimer.Stop();
		stopwatch.Stop();
		if (midiInPort != null)
		{
			midiInPort.MessageReceived -= MessageReceived;
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
		recording.Clear();
		Reset();
	}
	private void Restart()
	{
		PlayEnd();
		isEnded = false;
		index = 0;
		Reset();
	}

	private void DeviceAdded(DeviceWatcher sender, DeviceInformation args) => (InputDevices, OutputDevices) = (Task.Run(GetInputDevices).Result, Task.Run(GetOutputDevices).Result);
	private void DeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
	{
		(InputDevices, OutputDevices) = (Task.Run(GetInputDevices).Result, Task.Run(GetOutputDevices).Result);
		if (InputDevice != null && !InputDevices.Contains(InputDevice))
		{
			InputDevice = null;
			Application.Current.Dispatcher.Invoke(() =>
			{
				if (IsRunning)
				{
					IsRunning = false;
					ClearRestart();
				}
			});
		}
		if (OutputDevice != null && !OutputDevices.Contains(OutputDevice))
		{
			OutputDevice = null;
			Application.Current.Dispatcher.Invoke(() =>
			{
				if (IsRunning)
				{
					IsRunning = false;
					ClearRestart();
				}
			});
		}
	}
	private void MessageReceived(MidiInPort sender, MidiMessageReceivedEventArgs args)
	{
		recording.Add(args.Message);
		Application.Current.Dispatcher.Invoke(() => DistributeMessage(args.Message));
	}
	private void TimerElapsed(object? sender, EventArgs e)
	{
		CurrentTime = stopwatch.Elapsed;
		if (!RecordMode)
		{
			switch (index < recording.Count)
			{
				case true:
					while (index < recording.Count && recording[index].Timestamp <= stopwatch.Elapsed)
					{
						try
						{
							midiOutPort?.SendMessage(recording[index]);
						}
						catch
						{
							return;
						}
						DistributeMessage(recording[index]);
						index++;
					}
					break;
				case false:
					IsRunning = false;
					isEnded = true;
					PlayEnd();
					break;
			}
		}
	}
	private void DistributeMessage(IMidiMessage message)
	{
		switch (message.Type)
		{
			case MidiMessageType.NoteOn:
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
			Frequencies[index] = 0;
			Keys[index] = false;
		}
		DamperLevel = 0;
		TotalNotes = 0;
		MaxFrequency = 0;
		ModalNotes = [];
		LowestNote = -1;
		HighestNote = -1;
		DifferentNotes = 0;
		GraphHeight = 10;
		CurrentTime = TimeSpan.Zero;
		stopwatch.Reset();
		if (IsRunning)
		{
			RecordPlay();
		}
	}

	private async Task<MidiInPort?> GetMidiInPort() => InputDevice != null ? await MidiInPort.FromIdAsync(InputDevice.Id) : null;
	private async Task<MidiOutPort?> GetMidiOutPort() => OutputDevice != null ? (MidiOutPort)await MidiOutPort.FromIdAsync(OutputDevice.Id) : null;
	private static async Task<DeviceInformationCollection> GetInputDevices() => await DeviceInformation.FindAllAsync(MidiInPort.GetDeviceSelector());
	private static async Task<DeviceInformationCollection> GetOutputDevices() => await DeviceInformation.FindAllAsync(MidiOutPort.GetDeviceSelector());
}