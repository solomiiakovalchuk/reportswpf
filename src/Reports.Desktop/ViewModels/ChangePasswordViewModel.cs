using Reports.Core.Abstractions;
using Reports.Desktop.Infrastructure;

namespace Reports.Desktop.ViewModels;

public sealed class ChangePasswordViewModel : ObservableObject
{
    private readonly IAuthService _auth;
    private readonly Action<string> _report;
    private string _status = string.Empty;
    private bool _isLoading;

    public ChangePasswordViewModel(IAuthService auth, Action<string> report)
    {
        _auth = auth;
        _report = report;
        SaveCommand = new AsyncRelayCommand(p => SaveAsync(p as string[]));
    }

    public string Status
    {
        get => _status;
        set => Set(ref _status, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => Set(ref _isLoading, value);
    }

    public AsyncRelayCommand SaveCommand { get; }

    public event EventHandler? Saved;

    private async Task SaveAsync(string[]? passwords)
    {
        if (passwords is not [var current, var next, var confirm]) return;

        if (next != confirm) { Status = "Нові паролі не співпадають."; return; }
        if (next.Length < 4)  { Status = "Пароль має бути не коротшим за 4 символи."; return; }

        IsLoading = true;
        Status = string.Empty;
        try
        {
            var ok = await _auth.ChangePasswordAsync(current, next);
            if (ok)
            {
                _report("Пароль змінено.");
                Saved?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                Status = "Поточний пароль невірний.";
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
}
