using System.Windows.Input;

namespace MidiPiano.Source.MVVM;

internal class RelayCommand(Action execute, Func<bool> canExecute) : ICommand
{
	private readonly Action execute = execute;
	private readonly Func<bool> canExecute = canExecute;

	public event EventHandler? CanExecuteChanged
	{
		add => CommandManager.RequerySuggested += value;
		remove => CommandManager.RequerySuggested -= value;
	}

	public void Execute() => execute.Invoke();
	public void Execute(object? parameter) => execute.Invoke();
	public bool CanExecute(object? parameter) => canExecute.Invoke();
	public bool CanExecute() => canExecute.Invoke();
}