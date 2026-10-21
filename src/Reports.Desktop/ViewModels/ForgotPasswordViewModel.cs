using Reports.Core.Abstractions;
using Reports.Core.Services;
using Reports.Desktop.Infrastructure;
using Reports.Integration;

namespace Reports.Desktop.ViewModels;

public sealed class ForgotPasswordViewModel : ObservableObject
{
    private readonly IAuthService _auth;
    private readonly GoogleAuthenticator _google;
    private readonly AppSettings _settings;
    private readonly Action<AppSettings> _saveSettings;
    private bool _googleVerified;
    private string _status = string.Empty;
    private bool _isLoading;

    public ForgotPasswordViewModel(
        IAuthService auth,
        GoogleAuthenticator google,
        AppSettings settings,
        Action<AppSettings> saveSettings)
    {
        _auth = auth;
        _google = google;
        _settings = settings;
        _saveSettings = saveSettings;
        VerifyViaGoogleCommand = new AsyncRelayCommand(_ => VerifyAsync());
        ResetPasswordCommand = new AsyncRelayCommand(p => ResetAsync(p as string));
    }

    public bool GoogleVerified
    {
        get => _googleVerified;
        private set => Set(ref _googleVerified, value);
    }

    public string Status
    {
        get => _status;
        set => Set(ref _status, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set => Set(ref _isLoading, value);
    }

    public AsyncRelayCommand VerifyViaGoogleCommand { get; }
    public AsyncRelayCommand ResetPasswordCommand { get; }

    public event EventHandler? ResetSucceeded;

    private async Task VerifyAsync()
    {
        IsLoading = true;
        Status = "Відкриваємо браузер для входу в Google…";
        try
        {
            var email = await _google.VerifyInteractiveAsync();

            var stored = _settings.AdminGoogleEmail;
            if (string.IsNullOrEmpty(stored))
            {
                // First time: remember this email as the trusted admin account
                _settings.AdminGoogleEmail = email;
                _saveSettings(_settings);
            }
            else if (!string.Equals(stored, email, StringComparison.OrdinalIgnoreCase))
            {
                Status = $"Невірний акаунт Google ({email}).\nДозволено лише: {stored}";
                return;
            }

            GoogleVerified = true;
            Status = $"Підтверджено як {email}. Введіть новий пароль.";
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ResetAsync(string? password)
    {
        if (!GoogleVerified || string.IsNullOrEmpty(password)) return;
        IsLoading = true;
        try
        {
            await _auth.ResetPasswordAsync("admin", password);
            Status = "Пароль змінено. Можна входити.";
            ResetSucceeded?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
