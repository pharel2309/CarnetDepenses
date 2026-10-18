namespace CarnetDepenses.ViewModels;

public class RelayCommand : System.Windows.Input.ICommand
{
    private readonly Action<object?> _exec;
    private readonly Func<object?, bool>? _peutExecuter;
    public RelayCommand(Action<object?> exec, Func<object?, bool>? peutExecuter = null)
    {
        _exec = exec;
        _peutExecuter = peutExecuter;
    }

    public bool CanExecute(object? p) => _peutExecuter?.Invoke(p) ?? true;

    public void Execute(object? p) => _exec(p);

    public event EventHandler? CanExecuteChanged;

    public void Rafraichir() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
