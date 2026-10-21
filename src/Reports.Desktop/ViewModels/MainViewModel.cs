using System.Collections.ObjectModel;
using System.Windows;
using Reports.Desktop.Infrastructure;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Reports.Desktop.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly AppServices _services;
    private string _status = "Готово.";
    private NavigationItem _selected = null!;
    private bool _isDark;

    public MainViewModel(AppServices services)
    {
        _services = services;

        Report = new ReportViewModel(services, SetStatus);
        Missions = new MissionsViewModel(services, SetStatus);
        Directories = new DirectoriesViewModel(services, SetStatus);
        Journal = new JournalViewModel(services, SetStatus);
        Settings = new SettingsViewModel(services, SetStatus);

        Missions.DataChanged += async (_, _) => await RefreshDependentAsync();
        Directories.DataChanged += async (_, _) => await RefreshDependentAsync();

        Pages = new ObservableCollection<NavigationItem>
        {
            new("Формування рапорту", SymbolRegular.DocumentAdd24, Report),
            new("Відповіді", SymbolRegular.Table24, Missions),
            new("Довідники", SymbolRegular.BookQuestionMark24, Directories),
            new("Журнал", SymbolRegular.History24, Journal),
            new("Налаштування", SymbolRegular.Settings24, Settings)
        };
        _selected = Pages[0];

        _isDark = ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Dark;

        ReloadAllCommand = new AsyncRelayCommand(LoadAsync);
        SyncCommand = Missions.SyncCommand;
        ToggleThemeCommand = new RelayCommand(ToggleTheme);
        AboutCommand = new RelayCommand(ShowAbout);
    }

    public ReportViewModel Report { get; }
    public MissionsViewModel Missions { get; }
    public DirectoriesViewModel Directories { get; }
    public JournalViewModel Journal { get; }
    public SettingsViewModel Settings { get; }

    public ObservableCollection<NavigationItem> Pages { get; }

    public NavigationItem Selected
    {
        get => _selected;
        set { if (value is not null) Set(ref _selected, value); }
    }

    public string Status
    {
        get => _status;
        private set => Set(ref _status, value);
    }

    public bool IsDark
    {
        get => _isDark;
        set { if (Set(ref _isDark, value)) ApplyTheme(); }
    }

    public string DataFolder => _services.DataFolder;

    public AsyncRelayCommand ReloadAllCommand { get; }
    public AsyncRelayCommand SyncCommand { get; }
    public RelayCommand ToggleThemeCommand { get; }
    public RelayCommand AboutCommand { get; }

    public void SetStatus(string message) => Status = message;

    public async Task LoadAsync()
    {
        await Directories.LoadAsync();
        await Missions.LoadAsync();
        await Report.LoadAsync();
        await Journal.LoadAsync();

        SetStatus("Джерело: Google Таблиця.");
    }

    private async Task RefreshDependentAsync()
    {
        await Report.LoadAsync();
        await Journal.LoadAsync();
    }

    private void ToggleTheme() => IsDark = !IsDark;

    private void ApplyTheme() =>
        ApplicationThemeManager.Apply(
            _isDark ? ApplicationTheme.Dark : ApplicationTheme.Light,
            WindowBackdropType.Mica);

    private void ShowAbout()
    {
        var box = new Wpf.Ui.Controls.MessageBox
        {
            Title = "Довідка",
            Content =
                "Синхронізація: натисніть «Синхронізація». У розділі «Відповіді» перевірте записи: помилкові скасуйте кнопкою «Скасувати / повернути», а при статусі «Потребує уваги» — додайте відсутні позиції у довідники.\n" +
                "Формування рапорту: на сторінці «Формування рапорту» виберіть період, підписанта, фільтри та «Бойовий наказ». Натисніть «Переглянути текст» -> «Створити документ» -> «Відкрити файл».\n" +
                $"Тека даних:\n{_services.DataFolder}",
            CloseButtonText = "Закрити"
        };
        _ = box.ShowDialogAsync();
    }
}
