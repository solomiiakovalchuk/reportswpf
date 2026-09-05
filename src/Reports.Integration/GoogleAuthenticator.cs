using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace Reports.Integration;

/// <summary>
/// Помилка налаштування доступу до Google — показується користувачеві як підказка,
/// що саме треба донастроїти, а не як виняток бібліотеки.
/// </summary>
public sealed class GoogleAccessException : Exception
{
    public GoogleAccessException(string message, Exception? inner = null) : base(message, inner) { }
}

/// <summary>
/// Авторизація десктопного застосунку в Google (OAuth 2.0, тип «Desktop app»).
/// <para>
/// Перший запуск відкриває браузер і просить згоду; отриманий токен
/// кешується у теці <c>token</c>, далі згода більше не потрібна.
/// </para>
/// </summary>
public sealed class GoogleAuthenticator
{
    /// <summary>Достатньо доступу лише на читання таблиці.</summary>
    private static readonly string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };

    private readonly string _credentialsPath;
    private readonly string _tokenFolder;
    private readonly string _applicationName;

    public GoogleAuthenticator(string credentialsPath, string tokenFolder, string applicationName)
    {
        _credentialsPath = credentialsPath;
        _tokenFolder = tokenFolder;
        _applicationName = applicationName;
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

        UserCredential credential;
        try
        {
            await using var stream = File.OpenRead(_credentialsPath);
            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                Scopes,
                user: "user",
                ct,
                new FileDataStore(_tokenFolder, fullPath: true));
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

    /// <summary>Забути збережений токен — знадобиться при зміні акаунта.</summary>
    public void ForgetToken()
    {
        if (Directory.Exists(_tokenFolder))
            Directory.Delete(_tokenFolder, recursive: true);
    }
}
