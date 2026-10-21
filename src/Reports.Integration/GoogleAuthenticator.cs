using System.Text;
using System.Text.Json;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace Reports.Integration;

public sealed class GoogleAccessException : Exception
{
    public GoogleAccessException(string message, Exception? inner = null) : base(message, inner) { }
}

public sealed class GoogleAuthenticator
{
    private static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };

    private readonly string _credentialsPath;
    private readonly string _tokenFolder;
    private readonly string _applicationName;

    public GoogleAuthenticator(string credentialsPath, string tokenFolder, string applicationName)
    {
        _credentialsPath = credentialsPath;
        _tokenFolder = tokenFolder;
        _applicationName = ToHeaderSafe(applicationName);
    }

    public string CredentialsPath => _credentialsPath;

    public bool HasCredentialsFile => File.Exists(_credentialsPath);

    public async Task<SheetsService> CreateServiceAsync(CancellationToken ct = default)
    {
        if (!HasCredentialsFile)
            throw new GoogleAccessException(
                $"Не знайдено файл «{Path.GetFileName(_credentialsPath)}».\n\n" +
                "Google Cloud Console → APIs & Services → Credentials → Create credentials → " +
                "OAuth client ID → тип «Desktop app» → Download JSON.\n" +
                $"Покладіть файл сюди:\n{_credentialsPath}");

        using var consentTimeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        consentTimeout.CancelAfter(TimeSpan.FromMinutes(3));

        UserCredential credential;
        try
        {
            await using var stream = File.OpenRead(_credentialsPath);
            var secrets = (await GoogleClientSecrets.FromStreamAsync(stream, consentTimeout.Token)).Secrets;
            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                secrets,
                Scopes,
                user: "user",
                consentTimeout.Token,
                new FileDataStore(_tokenFolder, fullPath: true));
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new GoogleAccessException(
                "Не дочекалися згоди в браузері (3 хв). Натисніть кнопку ще раз " +
                "і завершіть вхід у Google у вікні браузера.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new GoogleAccessException(
                "Не вдалося авторизуватися в Google. Перевірте, що у файлі облікових даних " +
                "тип клієнта — «Desktop app», а ваш акаунт доданий у Test users на екрані згоди.",
                ex);
        }

        return new SheetsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = _applicationName
        });
    }

    private static string ToHeaderSafe(string name)
    {
        var safe = new string((name ?? string.Empty)
            .Where(c => c > 31 && c < 127 && c != '(' && c != ')')
            .ToArray())
            .Trim();

        return safe.Length == 0 ? "ReportsWpf" : safe;
    }

    // Always opens the browser — used for password-reset identity proof.
    // Returns the authenticated Google email so the caller can verify it.
    public async Task<string> VerifyInteractiveAsync(CancellationToken ct = default)
    {
        if (!HasCredentialsFile)
            throw new GoogleAccessException(
                $"Не знайдено файл «{Path.GetFileName(_credentialsPath)}».\n\n" +
                "Для скидання пароля потрібен файл credentials.json у теці даних.");

        var verifyFolder = Path.Combine(_tokenFolder, "reset_verify");

        // Always delete first to force the browser to open
        if (Directory.Exists(verifyFolder))
            Directory.Delete(verifyFolder, recursive: true);

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromMinutes(3));

        UserCredential credential;
        try
        {
            await using var stream = File.OpenRead(_credentialsPath);
            var secrets = (await GoogleClientSecrets.FromStreamAsync(stream, timeout.Token)).Secrets;
            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                secrets,
                [.. Scopes, "email", "openid"],
                user: "reset_verify",
                timeout.Token,
                new FileDataStore(verifyFolder, fullPath: true));
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new GoogleAccessException(
                "Не дочекалися входу в браузері (3 хв). Спробуйте ще раз.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new GoogleAccessException(
                "Не вдалося підтвердити особу через Google.", ex);
        }
        finally
        {
            if (Directory.Exists(verifyFolder))
                Directory.Delete(verifyFolder, recursive: true);
        }

        return ExtractEmailFromIdToken(credential.Token.IdToken)
               ?? throw new GoogleAccessException(
                   "Не вдалося отримати email з Google-токена.");
    }

    private static string? ExtractEmailFromIdToken(string? idToken)
    {
        if (string.IsNullOrEmpty(idToken)) return null;
        var parts = idToken.Split('.');
        if (parts.Length < 2) return null;

        // JWT payload: base64url-encoded JSON
        var payload = parts[1];
        var padded = payload + new string('=', (4 - payload.Length % 4) % 4);
        var json = Encoding.UTF8.GetString(Convert.FromBase64String(padded));

        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.TryGetProperty("email", out var el) ? el.GetString() : null;
    }

    public void ForgetToken()
    {
        if (Directory.Exists(_tokenFolder))
            Directory.Delete(_tokenFolder, recursive: true);
    }
}
