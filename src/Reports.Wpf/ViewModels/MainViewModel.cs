using System.Windows;
using Reports.Wpf.Infrastructure;

namespace Reports.Wpf.ViewModels;

/// <summary>
/// Головна модель подання: тримає вкладки, рядок стану й команди меню.
/// </summary>
public sealed class MainViewModel : ObservableObject
{
    private readonly AppServices _services;
    private string _status = "Готово.";
    private int _selectedTab;

    public MainViewModel(AppServices services)
    {
        _services = services;

        Report = new ReportViewModel(services, SetStatus);
        Missions = new MissionsViewModel(services, SetStatus);
        Directories = new DirectoriesViewModel(services, SetStatus);
        Journal = new JournalViewModel(services, SetStatus);
        Settings = new SettingsViewModel(services, SetStatus);

        // Зміна даних в одній вкладці має бути видна в інших.
        Missions.DataChanged += async (_, _) => await RefreshDependentAsync();
        Directories.DataChanged += async (_, _) => await RefreshDependentAsync();

        ReloadAllCommand = new AsyncRelayCommand(LoadAsync);
        SyncCommand = Missions.SyncCommand;
        ExitCommand = new RelayCommand(() => Application.Current.Shutdown());
        AboutCommand = new RelayCommand(ShowAbout);
    }

    public ReportViewModel Report { get; }
    public MissionsViewModel Missions { get; }
    public DirectoriesViewModel Directories { get; }
    public JournalViewModel Journal { get; }
    public SettingsViewModel Settings { get; }

    public string Status
    {
        get => _status;
        private set => Set(ref _status, value);
    }

    public int SelectedTab
    {
        get => _selectedTab;
        set => Set(ref _selectedTab, value);
    }

    public string DataFolder => _services.DataFolder;

    public AsyncRelayCommand ReloadAllCommand { get; }
    public AsyncRelayCommand SyncCommand { get; }
    public RelayCommand ExitCommand { get; }
    public RelayCommand AboutCommand { get; }

    public void SetStatus(string message) => Status = message;

    public async Task LoadAsync()
    {
        await Directories.LoadAsync();
        await Missions.LoadAsync();
        await Report.LoadAsync();
        await Journal.LoadAsync();

        SetStatus(Settings.SourceDescription + $"  Дані: {_services.DataFolder}");
    }

    private async Task RefreshDependentAsync()
    {
        await Report.LoadAsync();
        await Journal.LoadAsync();
    }

    private void ShowAbout()
    {
        MessageBox.Show(
            "Система формування рапортів про використання БпЛА та боєприпасів\n\n" +
            "Лабораторні роботи з дисципліни «Конструювання програмного забезпечення»\n" +
            "Хмельницький національний університет\n\n" +
            "Технології: C# / .NET 8, WPF (MVVM), Entity Framework Core + SQLite,\n" +
            "Google Sheets API, DocumentFormat.OpenXml.\n\n" +
            $"Тека даних:\n{_services.DataFolder}",
            "Про програму",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
