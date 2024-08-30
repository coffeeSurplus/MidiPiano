using System.Windows.Input;

namespace MidiPiano.Source.MVVM;

internal class RelayCommand(Action execute, Func<bool>? canExecute = null) : ICommand
{
	private readonly Action execute = execute;
	private readonly Func<bool>? canExecute = canExecute;

	public event EventHandler? CanExecuteChanged
	{
		add => CommandManager.RequerySuggested += value;
		remove => CommandManager.RequerySuggested -= value;
	}

	public void Execute() => execute.Invoke();
	public bool CanExecute() => canExecute is null || canExecute.Invoke();
	public void Execute(object? parameter) => execute.Invoke();
	public bool CanExecute(object? parameter) => canExecute is null || canExecute.Invoke();
}