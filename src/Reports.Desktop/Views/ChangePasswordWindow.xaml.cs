using Reports.Desktop.ViewModels;

namespace Reports.Desktop.Views;

public partial class ChangePasswordWindow : Wpf.Ui.Controls.FluentWindow
{
    public ChangePasswordWindow()
    {
        InitializeComponent();
        DataContextChanged += (_, _) =>
        {
            if (DataContext is ChangePasswordViewModel vm)
                vm.Saved += (_, _) => Close();
        };
    }

    private void SaveBtn_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ChangePasswordViewModel vm)
            vm.SaveCommand.Execute(new[] { CurrentBox.Password, NewBox.Password, ConfirmBox.Password });
    }
}
