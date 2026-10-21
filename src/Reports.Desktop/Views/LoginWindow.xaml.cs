using System.Windows.Input;
using Reports.Desktop.Infrastructure;
using Reports.Desktop.ViewModels;

namespace Reports.Desktop.Views;

public partial class LoginWindow : Wpf.Ui.Controls.FluentWindow
{
    public AppServices? Services { get; set; }

    public LoginWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => UsernameBox.Focus();
    }

    private void LoginBtn_Click(object sender, System.Windows.RoutedEventArgs e) =>
        ExecuteLogin();

    private void UsernameBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) PasswordBox.Focus();
    }

    private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) ExecuteLogin();
    }

    private void ExecuteLogin()
    {
        if (DataContext is LoginViewModel vm)
            vm.LoginCommand.Execute(PasswordBox.Password);
    }

    private void ForgotPassword_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if (Services == null) return;
        var vm = new ForgotPasswordViewModel(
            Services.Auth,
            Services.Authenticator,
            Services.Settings,
            s => Services.SettingsStore.Save(s));
        var window = new ForgotPasswordWindow { DataContext = vm, Owner = this };
        vm.ResetSucceeded += (_, _) => window.Close();
        window.ShowDialog();
    }
}
