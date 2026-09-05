using System.IO;
using System.Windows;
using System.Windows.Threading;
using Reports.Desktop.Infrastructure;
using Reports.Desktop.ViewModels;
using Reports.Desktop.Views;
using Wpf.Ui.Appearance;

namespace Reports.Desktop;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += OnUnhandledException;

        try
        {
            var services = AppServices.Create();
            await services.InitializeAsync();

            var model = new MainViewModel(services);
            await model.LoadAsync();

            MainWindow = new MainWindow { DataContext = model };

            // Тема слідує за системною, поки користувач не перемкне вручну.
            ApplicationThemeManager.ApplySystemTheme();
            SystemThemeWatcher.Watch(MainWindow);

            MainWindow.Show();
        }
        catch (Exception ex)
        {
            Log("Помилка запуску", ex);
            MessageBox.Show(
                "Не вдалося запустити застосунок:\n\n" + ex.Message +
                "\n\nПодробиці у файлі error.log у теці даних.",
                "Помилка запуску",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log("Необроблена помилка", e.Exception);

        MessageBox.Show(
            "Сталася помилка:\n\n" + e.Exception.Message,
            "Помилка",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);

        e.Handled = true;
    }

    /// <summary>
    /// Пише виняток у журнал помилок. Без цього діагностика збою на чужій
    /// машині зводиться до переказу тексту з екрана.
    /// </summary>
    private static void Log(string title, Exception exception)
    {
        try
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Рапорти");
            Directory.CreateDirectory(folder);

            File.AppendAllText(
                Path.Combine(folder, "error.log"),
                $"=== {DateTime.Now:yyyy-MM-dd HH:mm:ss} — {title} ==={Environment.NewLine}" +
                exception + Environment.NewLine + Environment.NewLine);
        }
        catch
        {
            // Журнал помилок сам по собі не має валити застосунок.
        }
    }
}
