using System.IO;
using Reports.Core.Abstractions;
using Reports.Core.Aggregation;
using Reports.Core.Services;
using Reports.Data;
using Reports.Data.Repositories;
using Reports.Documents;
using Reports.Integration;

namespace Reports.Wpf.Infrastructure;

/// <summary>
/// Корінь композиції: створює й зв'язує всі служби застосунку.
/// Зібрано вручну, без контейнера — залежностей небагато, і так видно
/// весь граф на одному екрані.
/// </summary>
public sealed class AppServices
{
    private AppServices(
        AppSettings settings,
        SettingsStore settingsStore,
        SqliteContextFactory contextFactory,
        IReferenceRepository references,
        IPersonRepository people,
        IMissionRepository missions,
        IJournalRepository journal,
        ImportService importService,
        ReportService reportService,
        DocxReportRenderer renderer,
        GoogleAuthenticator authenticator,
        string dataFolder)
    {
        Settings = settings;
        SettingsStore = settingsStore;
        ContextFactory = contextFactory;
        References = references;
        People = people;
        Missions = missions;
        Journal = journal;
        Import = importService;
        Reports = reportService;
        Renderer = renderer;
        Authenticator = authenticator;
        DataFolder = dataFolder;
    }

    public AppSettings Settings { get; }
    public SettingsStore SettingsStore { get; }
    public SqliteContextFactory ContextFactory { get; }
    public IReferenceRepository References { get; }
    public IPersonRepository People { get; }
    public IMissionRepository Missions { get; }
    public IJournalRepository Journal { get; }
    public ImportService Import { get; }
    public ReportService Reports { get; }
    public DocxReportRenderer Renderer { get; }
    public GoogleAuthenticator Authenticator { get; }
    public string DataFolder { get; }

    public string TemplatePath => Renderer.TemplatePath;
    public string CredentialsPath => Authenticator.CredentialsPath;

    /// <summary>Джерело даних відповідно до налаштувань: демонстраційне або Google.</summary>
    public IResponseSource CreateResponseSource() =>
        Settings.UseDemoData
            ? new DemoResponseSource()
            : new GoogleSheetsResponseSource(Authenticator, Settings);

    public static AppServices Create()
    {
        var dataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Рапорти");
        Directory.CreateDirectory(dataFolder);

        var settingsStore = new SettingsStore(Path.Combine(dataFolder, "settings.json"));
        var settings = settingsStore.Load();

        if (string.IsNullOrWhiteSpace(settings.OutputFolder))
            settings.OutputFolder = Path.Combine(dataFolder, "Рапорти");

        var contextFactory = new SqliteContextFactory(Path.Combine(dataFolder, "reports.db"));

        var references = new ReferenceRepository(contextFactory);
        var people = new PersonRepository(contextFactory);
        var missions = new MissionRepository(contextFactory);
        var journal = new JournalRepository(contextFactory);

        var renderer = new DocxReportRenderer(settings, Path.Combine(dataFolder, "template.docx"));

        var authenticator = new GoogleAuthenticator(
            Path.Combine(dataFolder, "credentials.json"),
            Path.Combine(dataFolder, "token"),
            "Рапорти БпЛА");

        var reportService = new ReportService(
            missions,
            journal,
            ReportAggregator.CreateDefault(),
            new IReportRenderer[] { renderer },
            settings);

        return new AppServices(
            settings, settingsStore, contextFactory,
            references, people, missions, journal,
            new ImportService(missions, references),
            reportService, renderer, authenticator, dataFolder);
    }

    /// <summary>Створює базу з довідниками-прикладами і шаблон рапорту.</summary>
    public async Task InitializeAsync()
    {
        await DatabaseInitializer.EnsureCreatedAsync(ContextFactory);
        Renderer.EnsureTemplateExists();
    }
}
