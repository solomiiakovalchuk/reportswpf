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
            // Prevent WPF from shutting down when LoginWindow closes before MainWindow opens
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var services = AppServices.Create();
            await services.InitializeAsync();

            var loginVm = new LoginViewModel(services.Auth);
            var loginWindow = new LoginWindow { DataContext = loginVm, Services = services };

            loginVm.LoginSucceeded += (_, _) => loginWindow.Close();
            loginWindow.ShowDialog();

            if (!services.Auth.IsAuthenticated) { Shutdown(0); return; }

            var model = new MainViewModel(services);
            await model.LoadAsync();

            MainWindow = new MainWindow { DataContext = model };
            ShutdownMode = ShutdownMode.OnMainWindowClose;

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
        }
    }
}
