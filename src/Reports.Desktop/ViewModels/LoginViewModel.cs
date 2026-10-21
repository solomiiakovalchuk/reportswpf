using Reports.Core.Abstractions;
using Reports.Desktop.Infrastructure;

namespace Reports.Desktop.ViewModels;

public sealed class LoginViewModel : ObservableObject
{
    private readonly IAuthService _auth;
    private string _username = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isLoading;

    public LoginViewModel(IAuthService auth)
    {
        _auth = auth;
        LoginCommand = new AsyncRelayCommand(p => DoLoginAsync(p as string));
    }

    public string Username
    {
        get => _username;
        set => Set(ref _username, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => Set(ref _errorMessage, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => Set(ref _isLoading, value);
    }

    public AsyncRelayCommand LoginCommand { get; }

    public event EventHandler? LoginSucceeded;

    private async Task DoLoginAsync(string? password)
    {
        ErrorMessage = string.Empty;
        IsLoading = true;
        try
        {
            var ok = await _auth.LoginAsync(Username, password ?? string.Empty);
            if (ok)
                LoginSucceeded?.Invoke(this, EventArgs.Empty);
            else
                ErrorMessage = "Невірний логін або пароль.";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
