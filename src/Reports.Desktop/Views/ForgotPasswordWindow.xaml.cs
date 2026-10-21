using Reports.Desktop.ViewModels;

namespace Reports.Desktop.Views;

public partial class ForgotPasswordWindow : Wpf.Ui.Controls.FluentWindow
{
    public ForgotPasswordWindow() => InitializeComponent();

    private void ResetBtn_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is not ForgotPasswordViewModel vm) return;

        if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
        {
            vm.Status = "Паролі не співпадають.";
            return;
        }

        vm.ResetPasswordCommand.Execute(NewPasswordBox.Password);
    }
}
