using System.Diagnostics;
using System.IO;
using Reports.Core.Services;
using Reports.Integration;
using Reports.Wpf.Infrastructure;

namespace Reports.Wpf.ViewModels;

/// <summary>
/// Вкладка «Налаштування»: реквізити частини, джерело даних і шаблон.
/// Постійні частини документа після зміни треба перенести у шаблон —
/// для цього є команда «Перебудувати шаблон».
/// </summary>
public sealed class SettingsViewModel : ObservableObject
{
    private readonly AppServices _services;
    private readonly Action<string> _report;

    public SettingsViewModel(AppServices services, Action<string> report)
    {
        _services = services;
        _report = report;

        SaveCommand = new RelayCommand(Save);
        RebuildTemplateCommand = new RelayCommand(RebuildTemplate);
        OpenTemplateCommand = new RelayCommand(() => OpenInShell(_services.TemplatePath));
        OpenDataFolderCommand = new RelayCommand(() => OpenInShell(_services.DataFolder));
        ForgetTokenCommand = new RelayCommand(ForgetToken);
    }

    private AppSettings S => _services.Settings;

    public string Classification
    {
        get => S.Classification;
        set { S.Classification = value; OnPropertyChanged(); }
    }

    public string Addressee
    {
        get => S.Addressee;
        set { S.Addressee = value; OnPropertyChanged(); }
    }

    public string Resolution
    {
        get => S.Resolution;
        set { S.Resolution = value; OnPropertyChanged(); }
    }

    public string ResolutionRank
    {
        get => S.ResolutionRank;
        set { S.ResolutionRank = value; OnPropertyChanged(); }
    }

    public string ResolutionName
    {
        get => S.ResolutionName;
        set { S.ResolutionName = value; OnPropertyChanged(); }
    }

    public string SubunitText
    {
        get => S.SubunitText;
        set { S.SubunitText = value; OnPropertyChanged(); }
    }

    public string OrderDefault
    {
        get => S.OrderDefault;
        set { S.OrderDefault = value; OnPropertyChanged(); }
    }

    public string SpreadsheetId
    {
        get => S.SpreadsheetId;
        set { S.SpreadsheetId = value; OnPropertyChanged(); }
    }

    public string ResponsesSheet
    {
        get => S.ResponsesSheet;
        set { S.ResponsesSheet = value; OnPropertyChanged(); }
    }

    public string DirectoriesSheet
    {
        get => S.DirectoriesSheet;
        set { S.DirectoriesSheet = value; OnPropertyChanged(); }
    }

    public string PeopleSheet
    {
        get => S.PeopleSheet;
        set { S.PeopleSheet = value; OnPropertyChanged(); }
    }

    public string OutputFolder
    {
        get => S.OutputFolder;
        set { S.OutputFolder = value; OnPropertyChanged(); }
    }

    public bool UseDemoData
    {
        get => S.UseDemoData;
        set
        {
            S.UseDemoData = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SourceDescription));
        }
    }

    public string SourceDescription => UseDemoData
        ? "Джерело: демонстраційні дані (без інтернету)."
        : "Джерело: Google Таблиця.";

    public string CredentialsStatus => File.Exists(_services.CredentialsPath)
        ? "Файл credentials.json знайдено."
        : "credentials.json відсутній — Google-синхронізація не працюватиме.";

    public string TemplatePath => _services.TemplatePath;
    public string DataFolder => _services.DataFolder;

    public RelayCommand SaveCommand { get; }
    public RelayCommand RebuildTemplateCommand { get; }
    public RelayCommand OpenTemplateCommand { get; }
    public RelayCommand OpenDataFolderCommand { get; }
    public RelayCommand ForgetTokenCommand { get; }

    private void Save()
    {
        _services.SettingsStore.Save(S);
        OnPropertyChanged(nameof(CredentialsStatus));
        _report("Налаштування збережено.");
    }

    private void RebuildTemplate()
    {
        _services.SettingsStore.Save(S);
        _services.Renderer.RebuildTemplate();
        _report("Шаблон перебудовано за поточними налаштуваннями.");
    }

    private void ForgetToken()
    {
        try
        {
            _services.Authenticator.ForgetToken();
            _report("Збережений токен Google видалено — наступного разу буде запит згоди.");
        }
        catch (Exception ex)
        {
            _report("Не вдалося видалити токен: " + ex.Message);
        }
    }

    private void OpenInShell(string path)
    {
        try
        {
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            _report("Не вдалося відкрити: " + ex.Message);
        }
    }
}
